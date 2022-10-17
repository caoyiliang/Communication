using Communication;
using Communication.Interfaces;
using System.Reflection;
using TopPortLib.Exceptions;
using TopPortLib.Interfaces;

namespace TopPortLib
{
    /// <summary>
    /// 队列通讯口
    /// </summary>
    public class PigeonPort : IPigeonPort
    {
        private readonly object _instance;
        private readonly ITopPort _topPort;
        private readonly int _defaultTimeout;
        private readonly int _timeDelayAfterSending;
        private readonly Type[] _typeList;
        private readonly List<ReqInfo> _reqInfos = new();
        /// <inheritdoc/>
        public event RequestedLogEventHandler? OnSentData;
        /// <inheritdoc/>
        public event RespondedLogEventHandler? OnReceivedData;
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
        /// <param name="defaultTimeout">超时时间，默认5秒</param>
        /// <param name="timeDelayAfterSending">发送后强制延时，默认20ms</param>
        public PigeonPort(object instance, ITopPort topPort, int defaultTimeout = 5000, int timeDelayAfterSending = 20)
        {
            _instance = instance;
            _topPort = topPort;
            _topPort.OnReceiveParsedData += TopPort_OnReceiveParsedData;
            _defaultTimeout = defaultTimeout;
            _timeDelayAfterSending = timeDelayAfterSending;
            _typeList = Assembly.GetCallingAssembly().GetTypes().Where(t => t.Namespace.EndsWith("Response")).ToArray();
        }

        private void InitActivelyPush(object obj, Type type, object data)
        {
            var eventMethod = obj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod).SingleOrDefault(_ => _.Name == $"{type.Name}Event");
            eventMethod?.Invoke(obj, new object?[] { type.GetMethod("GetResult")!.Invoke(data, null) });
        }

        private async Task TopPort_OnReceiveParsedData(byte[] data)
        {
            await RespondedDataAsync(data);
            Type? rspType = null;
            object? rsp = null;
            try
            {
                foreach (var item in _typeList)
                {
                    var a = item.GetInterface("IPigeonResponse`1");
                    if (a is null) continue;
                    object? obj = null;
                    try
                    {
                        obj = Activator.CreateInstance(item);
                    }
                    catch (Exception)
                    {

                    }
                    if (obj is null)
                        throw new ResponseCreateFailedException("Response创建失败");
                    var checkMethod = item.GetMethod("Check");
                    if ((bool)checkMethod.Invoke(obj, new object[] { data }))
                    {
                        var analyticalData = item.GetMethod("AnalyticalData");
                        analyticalData.Invoke(obj, new object[] { data });
                        rspType = item;
                        rsp = obj;
                        break;
                    }
                }
                if (rspType is null)
                    throw new GetRspTypeByRspBytesFailedException("收到未知命令");
                if (rsp is null)
                    throw new GetRspTypeByRspBytesFailedException("解析失败");
            }
            catch (Exception ex)
            {
                throw new GetRspTypeByRspBytesFailedException("通过响应的字节来获取响应类型失败", ex);
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
            InitActivelyPush(_instance, rspType, rsp);
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
            if (OnSentData is not null)
            {
                try
                {
                    await OnSentData(data);
                }
                catch
                {
                }
            }
        }

        private async Task RespondedDataAsync(byte[] data)
        {
            if (this.OnReceivedData != null)
            {
                try
                {
                    await OnReceivedData(data);
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
