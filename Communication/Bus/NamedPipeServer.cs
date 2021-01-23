using Communication.Interfaces;
using LogInterface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Communication.Bus
{
    public class NamedPipeServer : INamedPipeServer
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<TcpServer>();
        private int _bufferSize;
        private string _pipeName;
        private int _maxNumberOfServerInstances;
        private CancellationTokenSource _stopCts;
        private TaskCompletionSource<bool> _stopTcs;
        private ConcurrentDictionary<int, NamedPipeServerStream> _dicClients = new ConcurrentDictionary<int, NamedPipeServerStream>();
        public bool IsActive { get; private set; }

        public event ReceiveOriginalDataFromTcpClientEventHandler OnReceiveOriginalDataFromTcpClient;
        public event NamedPipeClientConnectEventHandler OnClientConnect;
        public event ClientDisconnectEventHandler OnClientDisconnect;

        public NamedPipeServer(string pipeName, int maxNumberOfServerInstances = 100, int bufferSize = 8192)
        {
            this._pipeName = pipeName;
            this._maxNumberOfServerInstances = maxNumberOfServerInstances;
            this._bufferSize = bufferSize;
        }

        public async Task StartAsync()
        {
            if (this.IsActive) return;
            _stopCts = new CancellationTokenSource();
            _stopTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            IsActive = true;
            this.AcceptClientAsync();
        }

        public async Task StopAsync()
        {
            if (!this.IsActive) return;
            this._stopCts.Cancel();
            if (await Task.WhenAny(_stopTcs.Task, Task.Delay(2000)) != _stopTcs.Task)
            {
                throw new TimeoutException("Waited too long to Close. timeout = 2000");
            }
        }

        public async Task SendDataAsync(int clientId, byte[] data)
        {
            try
            {
                if (!_dicClients.TryGetValue(clientId, out NamedPipeServerStream client)) return;
                lock (client)
                {
                    client.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Send data");
            }
        }

        public async Task DisconnectClientAsync(int clientId)
        {
            if (!_dicClients.TryGetValue(clientId, out NamedPipeServerStream client)) return;
            try
            {
                client.Close();
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Disconnect Client Async error{clientId}");
            }
            await TaskUtils.NullTask;
        }

        private async Task AcceptClientAsync()
        {
            await Task.Run(async () =>
            {
                var clientCounter = 0;
                try
                {
                    while (!_stopCts.IsCancellationRequested)
                    {
                        if (clientCounter >= _maxNumberOfServerInstances)
                        {
                            await Task.Delay(1000);
                            continue;
                        }
                        var client = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, _maxNumberOfServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                        try
                        {
                            await client.WaitForConnectionAsync();
                        }
                        catch
                        {
                            continue;
                        }
                        int clientId = clientCounter;
                        clientCounter++;
                        _dicClients.TryAdd(clientId, client);
                        try
                        {
                            await this.OnClientConnect?.Invoke(clientId);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Handle client connect error");
                        }
                        Task.Run(async () => await HandleClientAsync(client, clientId));
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Handle client connect error");
                }
                finally
                {
                    _stopCts.Cancel();
                    foreach (var c in _dicClients.Values)
                    {
                        c.Close();
                    }
                    _dicClients.Clear();
                    _stopTcs.TrySetResult(true);
                    this.IsActive = false;
                }
            });
        }

        private async Task HandleClientAsync(NamedPipeServerStream client, int clientId)
        {
            try
            {
                using (client)
                {
                    var amountRead = 0;
                    var buf = new byte[this._bufferSize];
                    while (!this._stopCts.IsCancellationRequested && client.IsConnected)
                    {
                        amountRead = await client.ReadAsync(buf, 0, buf.Length, _stopCts.Token);
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
                _dicClients.TryRemove(clientId, out NamedPipeServerStream value);
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
