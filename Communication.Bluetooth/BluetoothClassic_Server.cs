using Communication.Interfaces;
using LogInterface;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;

namespace Communication.Bluetooth
{
    public class BluetoothClassic_Server(int bufferSize = 8192) : IPhysicalPort_Server
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<BluetoothClassic_Server>();
        private readonly ConcurrentDictionary<Guid, BluetoothClient> _dicClients = new();
        private BluetoothListener? _listener;
        private CancellationTokenSource? _stopCts;
        private TaskCompletionSource<bool>? _stopTcs;

        public bool IsActive { get; private set; }

        public event ReceiveOriginalDataFromClientEventHandler? OnReceiveOriginalDataFromClient;
        public event ClientConnectEventHandler? OnClientConnect;
        public event ClientDisconnectEventHandler? OnClientDisconnect;

        public async Task DisconnectClientAsync(Guid clientId)
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

        /// <inheritdoc/>
        public async Task<string?> GetClientInfos(Guid clientId)
        {
            if (!_dicClients.TryGetValue(clientId, out var client)) return default;
            return await Task.FromResult($"{client.Client.AddressFamily}");
        }

        public async Task SendDataAsync(Guid clientId, byte[] data)
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

        public async Task StartAsync()
        {
            if (this.IsActive) return;

            _listener = new BluetoothListener(BluetoothService.SerialPort);
            _listener.Start();

            _stopCts = new CancellationTokenSource();
            _stopTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            IsActive = true;
            await AcceptClientAsync();
        }

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

        private async Task AcceptClientAsync()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    while (!_stopCts!.IsCancellationRequested)
                    {
                        BluetoothClient client;
                        try
                        {
                            client = _listener!.AcceptBluetoothClient();
                        }
                        catch (SocketException)
                        {
                            continue;
                        }
                        var clientId = Guid.NewGuid();
                        var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
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
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex);
                    }
                    foreach (var client in _dicClients.Values)
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

        private async Task HandleClientAsync(BluetoothClient client, Guid clientId)
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
