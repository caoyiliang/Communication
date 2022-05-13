using Communication.Exceptions;
using Communication.Interfaces;
using LogInterface;

namespace Communication.Bus
{
    /// <summary>
    /// 处理总线
    /// </summary>
    public class BusPort : IBusPort
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<BusPort>();
        private const int BUFFER_SIZE = 8192;
        private CancellationTokenSource? _closeCts;
        private TaskCompletionSource<bool>? _closeTcs;
        private volatile bool _isActiveClose = true;//是否主动断开
        private readonly SemaphoreSlim _semaphore4Write = new(1, 1);
        private IPhysicalPort _physicalPort;
        private bool IsOpen { get => _physicalPort.IsOpen; }

        /// <inheritdoc/>
        public IPhysicalPort PhysicalPort { get => _physicalPort; set { _physicalPort?.CloseAsync().Wait(); _physicalPort = value; } }
        /// <inheritdoc/>
        public event ReceiveOriginalDataEventHandler? OnReceiveOriginalData;

        /// <summary>
        /// 处理总线
        /// </summary>
        /// <param name="physicalPort">物理口</param>
        /// <exception cref="NullReferenceException"></exception>
        public BusPort(IPhysicalPort physicalPort)
        {
            this._physicalPort = physicalPort ?? throw new NullReferenceException("physicalPort is null");
        }

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
                }
            });
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            if (_isActiveClose) return;
            _isActiveClose = true;
            _closeCts?.Cancel();
            if (IsOpen)
                await _physicalPort.CloseAsync();
            if (_closeTcs is not null)
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
            try
            {
                try
                {
                    await _semaphore4Write.WaitAsync();
                    await _physicalPort.SendDataAsync(data, _closeCts!.Token);
                    if (timeInterval > 0)
                        await Task.Delay(timeInterval);
                }
                finally
                {
                    _semaphore4Write.Release();
                }
            }
            catch (Exception e)
            {
                throw new SendException("send failed", e);
            }
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
        }
    }
}
