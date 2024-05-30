using Communication;
using Communication.Interfaces;
using System.Reflection;
using TopPortLib.Exceptions;
using TopPortLib.Interfaces;
using Utils;

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
        private readonly List<ReqInfo> _reqInfos = [];
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
        /// <inheritdoc/>
        public CheckEventHandler? CheckEvent { get; set; }
        /// <summary>
        /// 队列通讯口
        /// </summary>
        /// <param name="instance">主动推出事件所在实体</param>
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
            _typeList = Assembly.GetCallingAssembly().GetTypes().Where(t => t.Namespace is not null && t.Namespace.EndsWith("Response")).ToArray();
        }

        private static void InitActivelyPush(object obj, Type type, object data)
        {
            var eventMethod = obj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod).SingleOrDefault(_ => _.Name == $"{type.Name}Event");
            eventMethod?.Invoke(obj, [type.GetMethod("GetResult")!.Invoke(data, null)]);
        }

        private async Task TopPort_OnReceiveParsedData(byte[] data)
        {
            await RespondedDataAsync(data);

            if (CheckEvent != null)
            {
                if (!await CheckEvent.Invoke(data))
                {
                    throw new Exception("crc error");
                }
            }

            Type? rspType = null;
            object? rsp = null;
            byte[]? checkBytes = null;
            try
            {
                foreach (var item in _typeList)
                {
                    if (item.GetInterface("IAsyncResponse`1") is null) continue;
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
                    var checkMethod = item.GetMethod("Check") ?? throw new CheckMethodNotFoundException("Check方法不存在");
                    var rs = ((bool Type, byte[]? CheckBytes))checkMethod.Invoke(obj, [data])!;
                    if (rs.Type)
                    {
                        checkBytes = rs.CheckBytes;
                        var analyticalData = item.GetMethod("AnalyticalData");
                        var task = (Task?)analyticalData?.Invoke(obj, [data]);
                        if (task != null) await task;
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
                reqInfo = _reqInfos.Find(ri => ri.RspType == rspType && ri.CheckBytes.ValueEqual(checkBytes));
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
        public async Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, int timeout = -1) where TReq : IAsyncRequest
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var reqInfo = new ReqInfo()
            {
                CheckBytes = req.Check(),
                RspType = typeof(TRsp),
                TaskCompletionSource = tcs,
            };
            lock (_reqInfos)
            {
                _reqInfos.Add(reqInfo);
            }
            var bytes = req.ToBytes();
            var timeoutTask = Task.Delay(to);
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
        public async Task<(TRsp1 Rsp1, TRsp2 Rsp2)> RequestAsync<TReq, TRsp1, TRsp2>(TReq req, int timeout = -1) where TReq : IAsyncRequest
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var reqInfo = new ReqInfo()
            {
                CheckBytes = req.Check(),
                RspType = typeof(TRsp1),
                TaskCompletionSource = tcs,
            };
            lock (_reqInfos)
            {
                _reqInfos.Add(reqInfo);
            }
            var bytes = req.ToBytes();
            try
            {
                var timeoutTask = Task.Delay(to);
                var sendTask = _topPort.SendAsync(bytes, _timeDelayAfterSending);
                if (timeoutTask == await Task.WhenAny(timeoutTask, sendTask))
                    throw new TimeoutException($"timeout={to}");
                await sendTask;
                await RequestDataAsync(bytes);
                if (timeoutTask == await Task.WhenAny(timeoutTask, tcs.Task))
                    throw new TimeoutException($"timeout={to}");
                var rs1 = (TRsp1)await tcs.Task;
                var timeoutTask1 = Task.Delay(to);
                tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                reqInfo.RspType = typeof(TRsp2);
                reqInfo.TaskCompletionSource = tcs;
                if (timeoutTask1 == await Task.WhenAny(timeoutTask1, tcs.Task))
                    throw new TimeoutException($"exe time out={to}");
                var rs2 = (TRsp2)await tcs.Task;
                return (rs1, rs2);
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
            if (this.OnReceivedData is not null)
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
            public byte[]? CheckBytes { get; set; }
            public Type RspType { get; set; } = null!;
            public TaskCompletionSource<object> TaskCompletionSource { get; set; } = null!;
        }
    }
}
