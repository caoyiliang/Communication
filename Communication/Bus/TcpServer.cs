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
    /// <param name="keepAlive">是否启用KeepAlive，默认true</param>
    /// <param name="keepAliveTime">正常心跳时间，默认5s(5000ms)</param>
    /// <param name="keepAliveInterval">异常心跳间隔，默认3s(3000ms)</param>
    /// <param name="keepAliveRetryCount">异常心跳重试次数，默认1次</param>
    /// <param name="noDelay">是否禁用Nagle算法，默认true</param>
    /// <param name="reuseAddress">是否允许端口复用，默认true</param>
    /// <param name="bufferSize">缓存默认8192</param>
    public class TcpServer(
        string hostName,
        int port,
        bool keepAlive = true,
        int keepAliveTime = 5000,
        int keepAliveInterval = 3000,
        int keepAliveRetryCount = 1,
        bool noDelay = true,
        bool reuseAddress = true,
        int bufferSize = 8192) : IPhysicalPort_Server
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<TcpServer>();
        private readonly ConcurrentDictionary<Guid, (TcpClient client, string hostName, int port)> _dicClients = new();
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
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, reuseAddress);
            _listener.Server.NoDelay = noDelay;
            if (keepAlive)
            {
#if NETSTANDARD2_0
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // 在 Linux 上通过系统参数配置 TCP Keep-Alive
                    string procPath = "/proc/sys/net/ipv4/";
                    File.WriteAllText(Path.Combine(procPath, "tcp_keepalive_time"), keepAliveTime.ToString());
                    File.WriteAllText(Path.Combine(procPath, "tcp_keepalive_intvl"), keepAliveInterval.ToString());
                    File.WriteAllText(Path.Combine(procPath, "tcp_keepalive_probes"), keepAliveRetryCount.ToString());
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _ = _listener.Server.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, keepAliveTime , keepAliveInterval ), null);
                }
#else
                try
                {
                    _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    _listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, keepAliveTime / 1000);
                    _listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, keepAliveInterval / 1000);
                    _listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, keepAliveRetryCount);
                }
                catch (Exception)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _ = _listener.Server.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, keepAliveTime, keepAliveInterval), null);
                    }
                }
#endif
            }
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
        public async Task SendDataAsync(Guid clientId, byte[] data)
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
        public async Task<(string IPAddress, int Port)?> GetClientInfo(Guid clientId)
        {
            if (!_dicClients.TryGetValue(clientId, out var client)) return null;
            return await Task.FromResult((client.hostName, client.port));
        }

        /// <inheritdoc/>
        public async Task<string?> GetClientInfos(Guid clientId)
        {
            if (!_dicClients.TryGetValue(clientId, out var client)) return default;
            return await Task.FromResult($"{client.hostName}:{client.port}");
        }

        /// <summary>
        /// 获取客户端ID
        /// </summary>
        /// <param name="ip">ip</param>
        /// <param name="port">端口</param>
        /// <returns>客户端ID</returns>
        public async Task<Guid?> GetClientId(string ip, int port)
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
        public async Task<List<Guid>> GetClientsByIp(string ip)
        {
            return await Task.FromResult(_dicClients.Where(_ => _.Value.hostName == ip).Select(_ => _.Key).ToList());
        }

        /// <inheritdoc/>
        public async Task DisconnectClientAsync(Guid clientId)
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
                        var clientId = Guid.NewGuid();
                        var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                        _dicClients.TryAdd(clientId, (client, remoteEndPoint!.Address.ToString(), remoteEndPoint.Port));
                        _ = Task.Run(async () => await HandleClientAsync(client, clientId));
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

        private async Task HandleClientAsync(TcpClient client, Guid clientId)
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
