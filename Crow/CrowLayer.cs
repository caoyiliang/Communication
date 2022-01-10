using Crow.Exceptions;
using Crow.Interfaces;
using System.Collections.Concurrent;

namespace Crow
{
    public class CrowLayer<TReq, TRsp> : ICrowLayer<TReq, TRsp>, IDisposable
    {
        private ITilesLayer<TReq, TRsp> _port;
        private int _defaultTimeout;
        private int _timeDelayAfterSending;
        private TaskCompletionSource<bool> _startStop;
        private TaskCompletionSource<bool> _completeStop;
        private TaskCompletionSource<TRsp> _rsp;
        private ConcurrentQueue<ReqInfo> _queue;
        private volatile bool _isActive = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="defaultTimeout">默认的超时时间</param>
        /// <param name="timeDelayAfterSending">当调用SendAsync时，防止数据黏在一起，要设置一个发送时间间隔</param>
        public CrowLayer(ITilesLayer<TReq, TRsp> port, int defaultTimeout = 5000, int timeDelayAfterSending = 20)
        {
            _port = port;
            _port.OnReceiveData += async data => _rsp?.TrySetResult(data);
            _defaultTimeout = defaultTimeout;
            _timeDelayAfterSending = timeDelayAfterSending;

        }

        public event SentDataEventHandler<TReq> OnSentData;
        public event ReceivedDataEventHandler<TRsp> OnReceivedData;

        public void Dispose()
        {
            var task = this.StopAsync();
            task.ConfigureAwait(false);
            task.Wait();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <returns></returns>
        public async Task<TRsp> RequestAsync(TReq req, int timeout = -1, bool background = false)
        {
            return await RequestAsync(req, true, timeout, background);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <returns></returns>
        public async Task SendAsync(TReq req, int timeout = -1, bool background = false)
        {
            await RequestAsync(req, false, timeout, background);
        }

        public async Task StartAsync()
        {
            if (_isActive) return;

            _isActive = true;
            _queue = new ConcurrentQueue<ReqInfo>();
            _startStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (!_queue.TryDequeue(out ReqInfo data))
                    {
                        if (await Task.WhenAny(Task.Delay(100), _startStop.Task) == _startStop.Task)
                            break;
                        else
                            continue;
                    }
                    if (!data.Background)
                        if (data.Timeout.IsCompleted)
                            continue;
                    _rsp?.TrySetCanceled();
                    _rsp = new TaskCompletionSource<TRsp>(TaskCreationOptions.RunContinuationsAsynchronously);
                    try
                    {
                        await _port.SendAsync(data.Req);
                        if (!(OnSentData is null))
                        {
                            try
                            {
                                await OnSentData(data.Req);
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        data.Rsp.TrySetException(new TilesSendException("", ex));
                        continue;
                    }
                    if (data.NeedRsp)
                    {
                        if (data.Background)
                            data.Timeout = Task.Delay(data.Time);
                        var task = await Task.WhenAny(data.Timeout, _rsp.Task, _startStop.Task);
                        if (task == _startStop.Task)
                        {
                            data.Rsp.TrySetException(new CrowStopWorkingException());
                        }
                        else if (task == _rsp.Task)
                        {
                            var rsp = await _rsp.Task;
                            data.Rsp.TrySetResult(rsp);
                            if (!(OnReceivedData is null))
                            {
                                try
                                {
                                    await OnReceivedData(rsp);
                                }
                                catch { }
                            }
                        }
                        else if (data.Background && (task == data.Timeout))
                        {
                            data.Rsp.TrySetException(new TimeoutException($"background timeout"));
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        data.Rsp.TrySetResult(default);
                        if (_startStop.Task.IsCompleted)
                            break;
                        await Task.Delay(_timeDelayAfterSending);
                    }
                }
                _completeStop.TrySetResult(true);
            });
        }

        public async Task StopAsync()
        {
            if (!_isActive) return;
            _isActive = false;
            _completeStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _startStop.TrySetResult(true);

            await _completeStop.Task;
        }
        private async Task<TRsp> RequestAsync(TReq req, bool needRsp, int timeout, bool background)
        {
            if (!_isActive) throw new CrowStopWorkingException();
            if (_queue.Count > 10) throw new CrowBusyException();
            var rsp = new TaskCompletionSource<TRsp>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tm = timeout == -1 ? _defaultTimeout : timeout;
            var data = new ReqInfo() { NeedRsp = needRsp, Req = req, Rsp = rsp, Time = tm, Background = background };
            _queue.Enqueue(data);
            if (!background)
            {
                data.Timeout = Task.Delay(tm);
                if (await Task.WhenAny(rsp.Task, data.Timeout) == data.Timeout)
                    throw new TimeoutException($"timeout={tm}");
            }
            return await rsp.Task;
        }
        class ReqInfo
        {
            public TReq Req { get; set; }
            public bool NeedRsp { get; set; }
            public TaskCompletionSource<TRsp> Rsp { get; set; }
            public Task Timeout { get; set; }
            public int Time { get; set; }
            public bool Background { get; set; }
        }
    }
}
