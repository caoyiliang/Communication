using Communication.Interfaces;
using LogInterface;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Communication.Bus
{
    /// <summary>
    /// TCP服务端
    /// </summary>
    public class TcpServer : IPhysicalPort_Server
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<TcpServer>();
        private readonly ConcurrentDictionary<int, TcpClient> _dicClients = new();
        private readonly string _hostName;
        private readonly int _port;
        private readonly int _bufferSize;
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

        /// <summary>
        /// TCP服务端
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        /// <param name="bufferSize"></param>
        public TcpServer(string hostName, int port, int bufferSize = 8192)
        {
            _hostName = hostName;
            _port = port;
            _bufferSize = bufferSize;
        }

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            if (this.IsActive) return;

            _listener = new TcpListener(IPAddress.Parse(_hostName), _port);
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
                var stream = client.GetStream();
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
            var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            return await Task.FromResult((remoteEndPoint!.Address.ToString(), remoteEndPoint.Port));
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
                var pair = _dicClients.Single(_ =>
                {
                    var remoteEndPoint = _.Value.Client.RemoteEndPoint as IPEndPoint;
                    return (remoteEndPoint!.Address.ToString() == ip) && (remoteEndPoint.Port == port);
                });
                return await Task.FromResult(pair.Key);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task DisconnectClientAsync(int clientId)
        {
            if (!_dicClients.TryGetValue(clientId, out var client)) return;
            try
            {
                client.Close();
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
                        _dicClients.TryAdd(clientId, client);
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
                    foreach (var c in _dicClients.Values)
                    {
                        c.Close();
                    }
                    _dicClients.Clear();
                    _stopTcs?.TrySetResult(true);
                    this.IsActive = false;
                }
            });
            await Task.CompletedTask;
        }

        private async Task HandleClientAsync(TcpClient client, int clientId)
        {
            try
            {
                using var clientStream = client.GetStream();
                using (client)
                {
                    var amountRead = 0;
                    var buf = new byte[this._bufferSize];
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
                _dicClients.TryRemove(clientId, out var value);
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
            }
        }
    }
}
