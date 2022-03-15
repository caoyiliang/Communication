using Communication.Interfaces;
using LogInterface;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Communication.Bus
{
    public class TcpServer : ITcpServer
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<TcpServer>();
        private string _hostName;
        private int _port;
        private int _bufferSize;
        private TcpListener _listener;
        private CancellationTokenSource _stopCts;
        private TaskCompletionSource<bool> _stopTcs;
        private ConcurrentDictionary<int, TcpClient> _dicClients = new ConcurrentDictionary<int, TcpClient>();
        public bool IsActive { get; private set; }

        public event ReceiveOriginalDataFromTcpClientEventHandler OnReceiveOriginalDataFromTcpClient;
        public event ClientConnectEventHandler OnClientConnect;
        public event ClientDisconnectEventHandler OnClientDisconnect;

        public TcpServer(string hostName, int port, int bufferSize = 8192)
        {
            this._hostName = hostName;
            this._port = port;
            this._bufferSize = bufferSize;
        }

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

        public async Task StopAsync()
        {
            if (!this.IsActive) return;
            this._stopCts.Cancel();
            this._listener.Stop();
            if (await Task.WhenAny(_stopTcs.Task, Task.Delay(2000)) != _stopTcs.Task)
            {
                throw new TimeoutException("Waited too long to Close. timeout = 2000");
            }
        }

        public async Task SendDataAsync(int clientId, byte[] data)
        {
            try
            {
                if (!_dicClients.TryGetValue(clientId, out TcpClient client)) return;
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

        public async Task DisconnectClientAsync(int clientId)
        {
            if (!_dicClients.TryGetValue(clientId, out TcpClient client)) return;
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
                    while (!_stopCts.IsCancellationRequested)
                    {
                        TcpClient client;
                        try
                        {
                            client = await _listener.AcceptTcpClientAsync();
                        }
                        catch (SocketException)
                        {
                            continue;
                        }
                        int clientId = clientCounter;
                        clientCounter++;
                        _dicClients.TryAdd(clientId, client);
                        var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                        try
                        {
                            await this.OnClientConnect?.Invoke(remoteEndPoint.Address.ToString(), remoteEndPoint.Port, clientId);
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
                    _stopCts.Cancel();
                    try
                    {
                        _listener.Stop();
                        _listener.Server.Close();
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
                    _stopTcs.TrySetResult(true);
                    this.IsActive = false;
                }
            });
            await Task.CompletedTask;
        }

        private async Task HandleClientAsync(TcpClient client, int clientId)
        {
            try
            {
                using (var clientStream = client.GetStream())
                using (client)
                {
                    var amountRead = 0;
                    var buf = new byte[this._bufferSize];
                    while (!this._stopCts.IsCancellationRequested && client.Connected)
                    {
                        amountRead = await clientStream.ReadAsync(buf, 0, buf.Length, _stopCts.Token);
                        if (amountRead <= 0)
                            break;
                        try
                        {
                            await OnReceiveOriginalDataFromTcpClient?.Invoke(buf, amountRead, clientId);
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
                _dicClients.TryRemove(clientId, out TcpClient value);
                try
                {
                    await this.OnClientDisconnect?.Invoke(clientId);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Handle client disconnect error");
                }
            }
        }
    }
}
