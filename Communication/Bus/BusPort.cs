using Communication.Exceptions;
using Communication.Interfaces;
using LogInterface;
using System.Threading.Channels;

namespace Communication.Bus
{
    /// <summary>
    /// 处理总线
    /// </summary>
    /// <param name="physicalPort">物理口</param>
    /// <exception cref="NullReferenceException"></exception>
    public class BusPort(IPhysicalPort physicalPort) : IBusPort
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<BusPort>();
        private const int BUFFER_SIZE = 8192;
        private CancellationTokenSource? _closeCts;
        private TaskCompletionSource<bool>? _closeTcs;
        private Task? _sendTask;
        private volatile bool _isActiveClose = true;//是否主动断开
        private readonly Channel<(byte[] data, int timeInterval, TaskCompletionSource<bool> tsc)> _channel = Channel.CreateUnbounded<(byte[] data, int timeInterval, TaskCompletionSource<bool> tsc)>();
        private IPhysicalPort _physicalPort = physicalPort ?? throw new NullReferenceException("physicalPort is null");
        private bool IsOpen { get => _physicalPort.IsOpen; }

        /// <inheritdoc/>
        public IPhysicalPort PhysicalPort { get => _physicalPort; set { _physicalPort?.CloseAsync().Wait(); _physicalPort = value; } }
        /// <inheritdoc/>
        public event ReceiveOriginalDataEventHandler? OnReceiveOriginalData;
        /// <inheritdoc/>
        public event DisconnectEventHandler? OnDisconnect;
        /// <inheritdoc/>
        public event ConnectEventHandler? OnConnect;
        /// <inheritdoc/>
        public event SentDataEventHandler<byte[]>? OnSentData;

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            if (!_isActiveClose) return;
            _isActiveClose = false;
            try
            {
                await OpenAsync_(true);
            }
            catch (ConnectFailedException)
            {
                _isActiveClose = true;
                throw;
            }
            _ = Task.Run(async () =>
            {
                while (!_isActiveClose)
                {
                    await OpenAsync_();
                    await ReadBusAsync();
                    if (OnDisconnect is not null)
                    {
                        await OnDisconnect.Invoke();
                    }
                }
            });
        }

        private async Task SendProcessDataAsync()
        {
            await foreach (var (data, timeInterval, tsc) in _channel.Reader.ReadAllAsync(_closeCts!.Token))
            {
                try
                {
                    await _physicalPort.SendDataAsync(data, _closeCts!.Token);
                    if (OnSentData is not null) await OnSentData.Invoke(data);
                    if (timeInterval > 0) await Task.Delay(timeInterval);
                    tsc.TrySetResult(true);
                }
                catch (Exception e)
                {
                    tsc.TrySetException(e);
                }
            }
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            if (_isActiveClose) return;
            _isActiveClose = true;
            _closeCts?.Cancel();
            if (IsOpen)
                await _physicalPort.CloseAsync();
            if (_sendTask is not null) await _sendTask;
            if (_closeTcs is not null && _closeCts is not null && (!_closeCts.IsCancellationRequested))
                if (await Task.WhenAny(_closeTcs.Task, Task.Delay(2000)) != _closeTcs.Task)
                {
                    throw new TimeoutException("Waited too long to Close. timeout = 2000");
                }
        }

        /// <inheritdoc/>
        public async Task SendAsync(byte[] data, int timeInterval = 0)
        {
            if (!IsOpen)
            {
                throw new NotConnectedException("Bus is not connected!");
            }
            var tcs = new TaskCompletionSource<bool>();
            await _channel.Writer.WriteAsync((data, timeInterval, tcs));
            await tcs.Task;
        }

        private async Task ReadBusAsync()
        {
            try
            {
                while (!_closeCts!.IsCancellationRequested)
                {
                    var result = await _physicalPort.ReadDataAsync(BUFFER_SIZE, _closeCts.Token);
                    if (result.Length <= 0) break;
                    try
                    {
                        if (OnReceiveOriginalData is not null)
                        {
                            await OnReceiveOriginalData.Invoke(result.Data, result.Length);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Handle original data error");
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                _closeCts?.Cancel();
                _closeTcs?.TrySetResult(true);
            }
        }

        private async Task OpenAsync_(bool isThrow = false)
        {
            if (IsOpen) return;
            while (!_isActiveClose)
            {
                try
                {
                    await _physicalPort.OpenAsync();
                    _closeCts = new CancellationTokenSource();
                    _closeTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    _sendTask = Task.Run(SendProcessDataAsync, _closeCts.Token);
                    if (OnConnect is not null)
                    {
                        await OnConnect.Invoke();
                    }
                    return;
                }
                catch (Exception ex)
                {
                    if (isThrow) throw new ConnectFailedException("bus connect failed!", ex);
                    await Task.Delay(50);
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            var task = this.CloseAsync();
            task.ConfigureAwait(false);
            task.Wait();

            GC.SuppressFinalize(this);
        }
    }
}
