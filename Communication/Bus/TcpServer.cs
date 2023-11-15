using Communication.Interfaces;
using LogInterface;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Communication.Bus
{
    /// <summary>
    /// TCP服务端
    /// </summary>
    /// <param name="hostName">服务端IP</param>
    /// <param name="port">服务端端口</param>
    /// <param name="bufferSize">缓存默认8192</param>
    public class TcpServer(string hostName, int port, int bufferSize = 8192) : IPhysicalPort_Server
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<TcpServer>();
        private readonly ConcurrentDictionary<int, (TcpClient client, string hostName, int port)> _dicClients = new();
        private TcpListener? _listener;
        private CancellationTokenSource? _stopCts;
        private TaskCompletionSource<bool>? _stopTcs;

        /// <inheritdoc/>
        public bool IsActive { get; private set; }

        /// <inheritdoc/>
        public event ReceiveOriginalDataFromClientEventHandler? OnReceiveOriginalDataFromClient;
        /// <inheritdoc/>
        public event ClientConnectEventHandler? OnClientConnect;
        /// <inheritdoc/>
        public event ClientDisconnectEventHandler? OnClientDisconnect;

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            if (this.IsActive) return;

            _listener = new TcpListener(IPAddress.Parse(hostName), port);
            _listener.Start();
            _stopCts = new CancellationTokenSource();
            _stopTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            IsActive = true;
            await AcceptClientAsync();
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            if (!this.IsActive) return;
            _stopCts?.Cancel();
            _listener?.Stop();
            if (_stopTcs is not null)
            {
                if (await Task.WhenAny(_stopTcs.Task, Task.Delay(2000)) != _stopTcs.Task)
                {
                    throw new TimeoutException("Waited too long to Close. timeout = 2000");
                }
            }
        }

        /// <inheritdoc/>
        public async Task SendDataAsync(int clientId, byte[] data)
        {
            try
            {
                if (!_dicClients.TryGetValue(clientId, out var client)) return;
                var stream = client.client.GetStream();
                lock (stream)
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Send data");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取客户端信息
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <returns>(IP,端口)</returns>
        public async Task<(string IPAddress, int Port)?> GetClientInfo(int clientId)
        {
            if (!_dicClients.TryGetValue(clientId, out var client)) return null;
            return await Task.FromResult((client.hostName, client.port));
        }

        /// <summary>
        /// 获取客户端ID
        /// </summary>
        /// <param name="ip">ip</param>
        /// <param name="port">端口</param>
        /// <returns>客户端ID</returns>
        public async Task<int?> GetClientId(string ip, int port)
        {
            try
            {
                var pair = _dicClients.Single(_ => (_.Value.hostName == ip) && (_.Value.port == port));
                return await Task.FromResult(pair.Key);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取IP下所有客户端
        /// </summary>
        /// <param name="ip">查询ip</param>
        /// <returns>IP下所有客户端</returns>
        public async Task<List<int>> GetClientsByIp(string ip)
        {
            return await Task.FromResult(_dicClients.Where(_ => _.Value.hostName == ip).Select(_ => _.Key).ToList());
        }

        /// <inheritdoc/>
        public async Task DisconnectClientAsync(int clientId)
        {
            if (!_dicClients.TryGetValue(clientId, out var client)) return;
            try
            {
                client.client.Close();
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Disconnect Client Async error{clientId}");
            }
            await Task.CompletedTask;
        }

        private async Task AcceptClientAsync()
        {
            _ = Task.Run(async () =>
            {
                var clientCounter = 0;
                try
                {
                    while (!_stopCts!.IsCancellationRequested)
                    {
                        TcpClient client;
                        try
                        {
                            client = await _listener!.AcceptTcpClientAsync();
                        }
                        catch (SocketException)
                        {
                            continue;
                        }
                        int clientId = clientCounter;
                        clientCounter++;
                        var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                        _dicClients.TryAdd(clientId, (client, remoteEndPoint!.Address.ToString(), remoteEndPoint.Port));
                        try
                        {
                            if (OnClientConnect is not null)
                            {
                                await OnClientConnect.Invoke(clientId);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Handle client connect error");
                        }
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            _ = client.Client.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 100, 100), null);
                        }
                        _ = Task.Run(async () => await HandleClientAsync(client, clientId));
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Handle client connect error");
                }
                finally
                {
                    _stopCts?.Cancel();
                    try
                    {
                        _listener?.Stop();
                        _listener?.Server.Close();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex);
                    }
                    foreach (var (client, hostName, port) in _dicClients.Values)
                    {
                        client.Close();
                    }
                    _dicClients.Clear();
                    _stopTcs?.TrySetResult(true);
                    this.IsActive = false;
                }
            });
            await Task.CompletedTask;
        }

        private static byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }

        private async Task HandleClientAsync(TcpClient client, int clientId)
        {
            try
            {
                using var clientStream = client.GetStream();
                using (client)
                {
                    var amountRead = 0;
                    var buf = new byte[bufferSize];
                    while (!_stopCts!.IsCancellationRequested && client.Connected)
                    {
                        amountRead = await clientStream.ReadAsync(buf, 0, buf.Length, _stopCts.Token);
                        if (amountRead <= 0)
                            break;
                        try
                        {
                            if (OnReceiveOriginalDataFromClient is not null)
                            {
                                await OnReceiveOriginalDataFromClient.Invoke(buf, amountRead, clientId);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Handle original data error");
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                try
                {
                    if (OnClientDisconnect is not null)
                    {
                        await OnClientDisconnect.Invoke(clientId);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Handle client disconnect error");
                }
                _dicClients.TryRemove(clientId, out _);
            }
        }
    }
}
