using LogInterface;
using Parser.Interfaces;
using Parser.Timers;
using Utils;

namespace Parser.Parsers
{
    /// <summary>
    /// 以时间来分割数据包
    /// </summary>
    public class TimeParser : IParser, IDisposable
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<TimeParser>();
        private volatile bool _isDisposeRequested = false;
        private readonly RemainBytes _bytes;
        private readonly SemaphoreSlim _bytesSemaphore = new(1, 1);
        private readonly ITimer _timer;
        /// <summary>
        /// 时间间隔
        /// </summary>
        public int TimeInterval { get; set; }
        /// <inheritdoc/>
        public event ReceiveParsedDataEventHandler? OnReceiveParsedData;
        /// <summary>
        /// 以时间来分割数据包
        /// </summary>
        /// <param name="timeInterval">时间间隔，默认20ms</param>
        public TimeParser(int timeInterval = 20)
        {
            TimeInterval = timeInterval;
            _timer = new WinApiTimer();
            _bytes = new RemainBytes();
            Task.Run(async () => await HandleDataAsync());
        }

        /// <inheritdoc/>
        public async Task ReceiveOriginalDataAsync(byte[] data, int size)
        {
            try
            {
                await _bytesSemaphore.WaitAsync();
                _bytes.Append(data, 0, size);
            }
            finally
            {
                _bytesSemaphore.Release();
            }
            _timer.Release();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this._isDisposeRequested = true;
            GC.SuppressFinalize(this);
        }

        private async Task HandleDataAsync()
        {
            while (!_isDisposeRequested)
            {
                if (!_timer.Wait(TimeInterval))
                {
                    try
                    {
                        await _bytesSemaphore.WaitAsync();
                        if (_bytes.Count > 0)
                        {
                            byte[] data = new byte[_bytes.Count];
                            Array.Copy(_bytes.Bytes, _bytes.StartIndex, data, 0, _bytes.Count);
                            _bytes.RemoveHeader(_bytes.Count);
                            try
                            {
                                if (OnReceiveParsedData is not null)
                                {
                                    await OnReceiveParsedData.Invoke(data);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "Handle Parsed data error");
                            }
                        }
                    }
                    finally
                    {
                        _bytesSemaphore.Release();
                    }
                }
            }
        }
    }
}
