using Crow.Exceptions;
using Crow.Interfaces;
using System.Collections.Concurrent;

namespace Crow
{
    /// <summary>
    /// 通讯队列
    /// </summary>
    /// <typeparam name="TReq">请求处理</typeparam>
    /// <typeparam name="TRsp">接收处理</typeparam>
    public class CrowLayer<TReq, TRsp> : ICrowLayer<TReq, TRsp>, IDisposable
    {
        private readonly ITilesLayer<TReq, TRsp> _port;
        private readonly int _defaultTimeout;
        private readonly int _timeDelayAfterSending;
        private TaskCompletionSource<bool>? _startStop;
        private TaskCompletionSource<bool>? _completeStop;
        private TaskCompletionSource<TRsp>? _rsp;
        private ConcurrentQueue<ReqInfo>? _queue;
        private volatile bool _isActive = false;
        /// <inheritdoc/>
        public event SentDataEventHandler<TReq>? OnSentData;
        /// <inheritdoc/>
        public event ReceivedDataEventHandler<TRsp>? OnReceivedData;

        /// <summary>
        /// 通讯队列
        /// </summary>
        /// <param name="port">通讯队列和通讯口之间的联系层</param>
        /// <param name="defaultTimeout">默认的超时时间</param>
        /// <param name="timeDelayAfterSending">当调用SendAsync时，防止数据黏在一起，要设置一个发送时间间隔</param>
        public CrowLayer(ITilesLayer<TReq, TRsp> port, int defaultTimeout = 5000, int timeDelayAfterSending = 20)
        {
            _port = port;
            _port.OnReceiveData += async data => { _rsp?.TrySetResult(data); await Task.CompletedTask; };
            _defaultTimeout = defaultTimeout;
            _timeDelayAfterSending = timeDelayAfterSending;

        }

        /// <inheritdoc/>
        public void Dispose()
        {
            var task = StopAsync();
            task.ConfigureAwait(false);
            task.Wait();
        }

        /// <inheritdoc/>
        public async Task<TRsp> RequestAsync(TReq req, int timeout = -1, bool background = false)
        {
            var rsp = await RequestAsync(req, true, timeout, background);
            return rsp!;
        }

        /// <inheritdoc/>
        public async Task SendAsync(TReq req, int timeout = -1, bool background = false)
        {
            await RequestAsync(req, false, timeout, background);
        }

        /// <inheritdoc/>
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
                    if (!_queue.TryDequeue(out var data))
                    {
                        if (await Task.WhenAny(Task.Delay(100), _startStop.Task) == _startStop.Task)
                            break;
                        else
                            continue;
                    }
                    if (!data.Background)
                        if (data.Timeout!.IsCompleted)
                            continue;
                    _rsp?.TrySetCanceled();
                    _rsp = new TaskCompletionSource<TRsp>(TaskCreationOptions.RunContinuationsAsynchronously);
                    try
                    {
                        await _port.SendAsync(data.Req);
                        if (OnSentData is not null)
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
                        var tasks = new List<Task>() { _rsp.Task, _startStop.Task };
                        if (data.Background)
                        {
                            data.Timeout = Task.Delay(data.Time);
                            tasks.Add(data.Timeout);
                        }
                        var task = await Task.WhenAny(tasks);
                        if (task == _startStop.Task)
                        {
                            data.Rsp.TrySetException(new CrowStopWorkingException());
                        }
                        else if (task == _rsp.Task)
                        {
                            var rsp = await _rsp.Task;
                            data.Rsp.TrySetResult(rsp);
                            if (OnReceivedData is not null)
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
                _completeStop?.TrySetResult(true);
            });
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            if (!_isActive) return;
            _completeStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _startStop?.TrySetResult(true);
            await _completeStop.Task;
            _isActive = false;
        }

        private async Task<TRsp?> RequestAsync(TReq req, bool needRsp, int timeout, bool background)
        {
            if (!_isActive) throw new CrowStopWorkingException();
            if (_queue!.Count > 10) throw new CrowBusyException();
            var rsp = new TaskCompletionSource<TRsp?>(TaskCreationOptions.RunContinuationsAsynchronously);
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
            public TReq Req { get; set; } = default!;
            public bool NeedRsp { get; set; }
            public TaskCompletionSource<TRsp?> Rsp { get; set; } = null!;
            public Task? Timeout { get; set; }
            public int Time { get; set; }
            public bool Background { get; set; }
        }
    }
}
