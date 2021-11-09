/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：TimeParser.cs
********************************************************************/

using LogInterface;
using Parser.Interfaces;
using Parser.Timers;
using Utils;

namespace Parser.Parsers
{
    public class TimeParser : IParser, IDisposable
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<TimeParser>();
        private volatile bool _isDisposeRequested = false;
        private RemainBytes _bytes;
        private SemaphoreSlim _bytesSemaphore = new SemaphoreSlim(1, 1);
        private ITimer _timer;
        /// <summary>
        /// 时间间隔
        /// </summary>
        public int TimeInterval { get; set; }
        public event ReceiveParsedDataEventHandler OnReceiveParsedData;

        public TimeParser(int timeInterval = 20)
        {
            TimeInterval = timeInterval;
            _timer = new NormalTimer();
            _bytes = new RemainBytes();
            Task.Run(async () => await HandleDataAsync());
        }

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

        public void Dispose()
        {
            this._isDisposeRequested = true;
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
                                await this.OnReceiveParsedData?.Invoke(data);
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
