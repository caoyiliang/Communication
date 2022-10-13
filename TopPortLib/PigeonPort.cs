using Communication;
using Communication.Interfaces;
using TopPortLib.Exceptions;
using TopPortLib.Interfaces;

namespace TopPortLib
{
    /// <summary>
    /// 队列通讯口
    /// </summary>
    public class PigeonPort : IPigeonPort
    {
        private readonly ITopPort _topPort;
        private readonly int _defaultTimeout;
        private readonly int _timeDelayAfterSending;
        private readonly Func<byte[], Type> _getRspTypeByRspBytes;
        private readonly List<ReqInfo> _reqInfos = new();
        /// <inheritdoc/>
        public event RequestedLogEventHandler? OnRequestedData;
        /// <inheritdoc/>
        public event RespondedLogEventHandler? OnRespondedData;
        /// <inheritdoc/>
        public event ReceiveActivelyPushDataEventHandler? OnReceiveActivelyPushData;
        /// <inheritdoc/>
        public event DisconnectEventHandler? OnDisconnect { add => _topPort.OnDisconnect += value; remove => _topPort.OnDisconnect -= value; }
        /// <inheritdoc/>
        public event ConnectEventHandler? OnConnect { add => _topPort.OnConnect += value; remove => _topPort.OnConnect -= value; }
        /// <inheritdoc/>
        public IPhysicalPort PhysicalPort { get => _topPort.PhysicalPort; set => _topPort.PhysicalPort = value; }
        /// <summary>
        /// 队列通讯口
        /// </summary>
        /// <param name="topPort">通讯口</param>
        /// <param name="getRspTypeByRspBytes">根据返回命令获取返回类型</param>
        /// <param name="defaultTimeout">超时时间，默认5秒</param>
        /// <param name="timeDelayAfterSending">发送后强制延时，默认20ms</param>
        public PigeonPort(ITopPort topPort, Func<byte[], Type> getRspTypeByRspBytes, int defaultTimeout = 5000, int timeDelayAfterSending = 20)
        {
            _topPort = topPort;
            _topPort.OnReceiveParsedData += TopPort_OnReceiveParsedData;
            _getRspTypeByRspBytes = getRspTypeByRspBytes;
            _defaultTimeout = defaultTimeout;
            _timeDelayAfterSending = timeDelayAfterSending;
        }

        private async Task TopPort_OnReceiveParsedData(byte[] data)
        {
            await RespondedDataAsync(data);
            Type rspType;
            try
            {
                rspType = _getRspTypeByRspBytes(data);
            }
            catch (Exception ex)
            {
                throw new GetRspTypeByRspBytesFailedException("通过响应的字节来获取响应类型失败", ex);
            }
            object? rsp = null;
            try
            {
                var constructors = rspType.GetConstructors();
                foreach (var constructor in constructors)
                {
                    var args = constructor.GetParameters();
                    if (args.Length == 1)
                    {
                        rsp = constructor.Invoke(new object[] { data });
                    }
                }
                if (rsp == null)
                    throw new ResponseParameterCreateFailedException("缺少一个参数的构造器");
            }
            catch (Exception ex)
            {
                throw new ResponseParameterCreateFailedException("ResponseParameterCreateFailedException", ex);
            }
            ReqInfo? reqInfo;
            lock (_reqInfos)
            {
                reqInfo = _reqInfos.Find(ri => ri.RspType == rspType);
            }
            if (reqInfo != null)
            {
                reqInfo.TaskCompletionSource.TrySetResult(rsp);
                return;
            }
            if (this.OnReceiveActivelyPushData != null)
            {
                try
                {
                    await OnReceiveActivelyPushData(rspType, rsp);
                }
                catch
                {
                }
            }
        }

        /// <inheritdoc/>
        public async Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, int timeout = -1) where TReq : IByteStream
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var reqInfo = new ReqInfo()
            {
                RspType = typeof(TRsp),
                TaskCompletionSource = tcs,
            };
            lock (_reqInfos)
            {
                _reqInfos.Add(reqInfo);
            }
            var timeoutTask = Task.Delay(to);
            var bytes = req.ToBytes();
            try
            {
                var sendTask = _topPort.SendAsync(bytes, _timeDelayAfterSending);
                if (timeoutTask == await Task.WhenAny(timeoutTask, sendTask))
                    throw new TimeoutException($"timeout={to}");
                await sendTask;
                await RequestDataAsync(bytes);
                if (timeoutTask == await Task.WhenAny(timeoutTask, tcs.Task))
                    throw new TimeoutException($"timeout={to}");
                return (TRsp)await tcs.Task;
            }
            finally
            {
                lock (_reqInfos)
                {
                    _reqInfos.Remove(reqInfo);
                }
            }
        }

        /// <inheritdoc/>
        public async Task SendAsync<TReq>(TReq req, int timeout = -1) where TReq : IByteStream
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var timeoutTask = Task.Delay(to);
            var bytes = req.ToBytes();
            var sendTask = _topPort.SendAsync(bytes, _timeDelayAfterSending);
            if (timeoutTask == await Task.WhenAny(timeoutTask, sendTask))
                throw new TimeoutException($"timeout={to}");
            await sendTask;
            await RequestDataAsync(bytes);
        }

        private async Task RequestDataAsync(byte[] data)
        {
            if (OnRequestedData is not null)
            {
                try
                {
                    await OnRequestedData(data);
                }
                catch
                {
                }
            }
        }

        private async Task RespondedDataAsync(byte[] data)
        {
            if (this.OnRespondedData != null)
            {
                try
                {
                    await OnRespondedData(data);
                }
                catch
                {
                }
            }
        }

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            await _topPort.OpenAsync();
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            await _topPort.CloseAsync();
            lock (_reqInfos)
            {
                _reqInfos.Clear();
            }
        }

        class ReqInfo
        {
            public Type RspType { get; set; } = null!;
            public TaskCompletionSource<object> TaskCompletionSource { get; set; } = null!;
        }
    }
}
