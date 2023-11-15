using Communication.Interfaces;
using LogInterface;
using System.Collections.Concurrent;
using System.IO.Pipes;

namespace Communication.Bus
{
    /// <summary>
    /// 命名管道服务端
    /// </summary>
    /// <param name="pipeName">名称</param>
    /// <param name="maxNumberOfServerInstances">最大服务数</param>
    /// <param name="bufferSize">读缓存</param>
    public class NamedPipeServer(string pipeName, int maxNumberOfServerInstances = 100, int bufferSize = 8192) : IPhysicalPort_Server
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<NamedPipeServer>();
        private readonly ConcurrentDictionary<int, NamedPipeServerStream> _dicClients = new();
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
            if (_stopTcs is not null)
                if (await Task.WhenAny(_stopTcs.Task, Task.Delay(2000)) != _stopTcs.Task)
                {
                    throw new TimeoutException("Waited too long to Close. timeout = 2000");
                }
        }

        /// <inheritdoc/>
        public async Task SendDataAsync(int clientId, byte[] data)
        {
            try
            {
                if (!_dicClients.TryGetValue(clientId, out var client)) return;
                lock (client)
                {
                    client.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Send data");
            }
            await Task.CompletedTask;
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
                        if (clientCounter >= maxNumberOfServerInstances)
                        {
                            await Task.Delay(1000);
                            continue;
                        }
                        var client = new NamedPipeServerStream(pipeName, PipeDirection.InOut, maxNumberOfServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
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

        private async Task HandleClientAsync(NamedPipeServerStream client, int clientId)
        {
            try
            {
                using (client)
                {
                    var amountRead = 0;
                    var buf = new byte[bufferSize];
                    while (!_stopCts!.IsCancellationRequested && client.IsConnected)
                    {
                        amountRead = await client.ReadAsync(buf, 0, buf.Length, _stopCts.Token);
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
