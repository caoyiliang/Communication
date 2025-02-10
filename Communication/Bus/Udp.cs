using Communication.Interfaces;
using LogInterface;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Communication.Bus
{
    /// <summary>
    /// UDP
    /// </summary>
    /// <param name="hostName"></param>
    /// <param name="port"></param>
    public class Udp(string hostName, int port) : IPhysicalPort_M2M
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<Udp>();
        private UdpClient? _client;
        private readonly ConcurrentDictionary<Guid, (IPEndPoint EndPoint, DateTime LastReceived)> _dicClients = new();
        private CancellationTokenSource? _stopCts;

        /// <inheritdoc/>
        public bool IsActive { get; private set; }

        /// <inheritdoc/>
        public event ReceiveOriginalDataFromClientEventHandler? OnReceiveOriginalDataFromClient;
        /// <inheritdoc/>
        public event ClientConnectEventHandler? OnClientConnect;

        /// <inheritdoc/>
        public async Task DisconnectClientAsync(Guid clientId)
        {
            _ = _dicClients.TryRemove(clientId, out _);
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
            return await Task.FromResult((client.EndPoint.Address.ToString(), client.EndPoint.Port));
        }

        /// <inheritdoc/>
        public async Task<string?> GetClientInfos(Guid clientId)
        {
            if (!_dicClients.TryGetValue(clientId, out var client)) return default;
            return await Task.FromResult($"{client.EndPoint.Address}:{client.EndPoint.Port}");
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
                var pair = _dicClients.Single(_ => (_.Value.EndPoint.Address.ToString() == ip) && (_.Value.EndPoint.Port == port));
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
            return await Task.FromResult(_dicClients.Where(_ => _.Value.EndPoint.Address.ToString() == ip).Select(_ => _.Key).ToList());
        }

        /// <inheritdoc/>
        public async Task SendDataAsync(Guid clientId, byte[] data)
        {
            if (!_dicClients.TryGetValue(clientId, out var remoteEndPoint))
            {
                _logger.Error("Client not found");
                return;
            }
            await _client!.SendAsync(data, data.Length, Dns.GetHostEntry(remoteEndPoint.EndPoint.Address).HostName, remoteEndPoint.EndPoint.Port);
        }

        /// <inheritdoc/>
        public async Task<Guid> SendDataAsync(string hostName, int port, byte[] data)
        {
            var remoteEndPoint = _dicClients.SingleOrDefault(p => p.Value.EndPoint.Address.ToString() == hostName && p.Value.EndPoint.Port == port);
            var clientId = Guid.NewGuid();
            if (remoteEndPoint.Value.EndPoint == null)
            {
                _dicClients.TryAdd(clientId, (new IPEndPoint(IPAddress.Parse(hostName), port), DateTime.UtcNow));
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
            else
            {
                clientId = remoteEndPoint.Key;
            }
            await SendDataAsync(clientId, data);
            return clientId;
        }

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            if (this.IsActive) return;
            _client = new UdpClient(new IPEndPoint(IPAddress.Parse(hostName), port));
            this.IsActive = true;
            _stopCts = new CancellationTokenSource();
            _ = Task.Run(RemoveInactiveClientsAsync);
            await AcceptClientAsync();
        }

        private async Task AcceptClientAsync()
        {
            _ = Task.Run(async () =>
            {
                while (!_stopCts!.IsCancellationRequested)
                {
                    UdpReceiveResult result;
                    try
                    {
                        result = await _client!.ReceiveAsync();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    var remoteEndPoint = _dicClients.SingleOrDefault(p => p.Value.EndPoint.Address.ToString() == result.RemoteEndPoint.Address.ToString() && p.Value.EndPoint.Port == result.RemoteEndPoint.Port);
                    var clientId = Guid.NewGuid();
                    if (remoteEndPoint.Value.EndPoint == null)
                    {
                        _dicClients.TryAdd(clientId, (result.RemoteEndPoint, DateTime.UtcNow));
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
                    else
                    {
                        clientId = remoteEndPoint.Key;
                        _dicClients[clientId] = (result.RemoteEndPoint, DateTime.UtcNow);
                    }

                    if (result.Buffer.Length <= 0)
                        break;
                    try
                    {
                        if (OnReceiveOriginalDataFromClient is not null)
                        {
                            await OnReceiveOriginalDataFromClient.Invoke(result.Buffer, result.Buffer.Length, clientId);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Handle original data error");
                    }
                }
                this.IsActive = false;
            });
            await Task.CompletedTask;
        }

        private async Task RemoveInactiveClientsAsync()
        {
            while (!_stopCts!.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var inactiveClients = _dicClients.Where(c => (now - c.Value.LastReceived).TotalSeconds > 120).Select(c => c.Key).ToList();
                foreach (var clientId in inactiveClients)
                {
                    _dicClients.TryRemove(clientId, out _);
                }
                await Task.Delay(10000); // 每10秒检查一次
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            if (!this.IsActive) return;
            _client?.Close();
            _client?.Dispose();
            _stopCts?.Cancel();
            _dicClients.Clear();
            await Task.CompletedTask;
        }
    }
}
