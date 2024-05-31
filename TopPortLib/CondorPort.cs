using Communication;
using Communication.Interfaces;
using Parser;
using System.Reflection;
using TopPortLib.Exceptions;
using TopPortLib.Interfaces;
using Utils;

namespace TopPortLib
{
    /// <summary>
    /// 队列通讯口
    /// </summary>
    public class CondorPort : ICondorPort
    {
        private readonly object _instance;
        private readonly ITopPort_Server _topPortServer;
        private readonly int _defaultTimeout;
        private readonly Type[] _typeList;
        private readonly List<ReqInfo> _reqInfos = [];
        /// <inheritdoc/>
        public event RequestedLogServerEventHandler? OnSentData;
        /// <inheritdoc/>
        public event RespondedLogServerEventHandler? OnReceivedData;
        /// <inheritdoc/>
        public event ReceiveActivelyPushDataServerEventHandler? OnReceiveActivelyPushData;
        /// <inheritdoc/>
        public event ClientConnectEventHandler? OnClientConnect { add => _topPortServer.OnClientConnect += value; remove => _topPortServer.OnClientConnect -= value; }
        /// <inheritdoc/>
        public event ClientDisconnectEventHandler? OnClientDisconnect { add => _topPortServer.OnClientDisconnect += value; remove => _topPortServer.OnClientDisconnect -= value; }
        /// <inheritdoc/>
        public IPhysicalPort_Server PhysicalPort { get => _topPortServer.PhysicalPort; }
        /// <inheritdoc/>
        public CheckEventHandler? CheckEvent { get; set; }
        /// <summary>
        /// 队列通讯口
        /// </summary>
        /// <param name="instance">主动推出事件所在实体</param>
        /// <param name="topPortServer">通讯口</param>
        /// <param name="defaultTimeout">超时时间，默认5秒</param>
        public CondorPort(object instance, ITopPort_Server topPortServer, int defaultTimeout = 5000)
        {
            _instance = instance;
            _topPortServer = topPortServer;
            _topPortServer.OnReceiveParsedData += TopPort_OnReceiveParsedData;
            _defaultTimeout = defaultTimeout;
            _typeList = Assembly.GetCallingAssembly().GetTypes().Where(t => t.Namespace is not null && t.Namespace.EndsWith("Response")).ToArray();
        }

        private static void InitActivelyPush(object obj, Type type, object data, int clientId)
        {
            var eventMethod = obj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod).SingleOrDefault(_ => _.Name == $"{type.Name}Event");
            eventMethod?.Invoke(obj, [clientId, type.GetMethod("GetResult")!.Invoke(data, null)]);
        }

        private async Task TopPort_OnReceiveParsedData(int clientId, byte[] data)
        {
            await RespondedDataAsync(clientId, data);

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
                    if (item.GetInterface("IAsyncResponse_Server`1") is null) continue;
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
                    var rs = ((bool Type, byte[]? CheckBytes))checkMethod.Invoke(obj, [clientId, data])!;
                    if (rs.Type)
                    {
                        checkBytes = rs.CheckBytes;
                        var analyticalData = item.GetMethod("AnalyticalData");
                        var task = (Task?)analyticalData?.Invoke(obj, [clientId, data]);
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
                reqInfo = _reqInfos.Find(ri => ri.ClientId == clientId && ri.RspType.Contains(rspType) && ri.CheckBytes.ValueEqual(checkBytes));
            }
            if (reqInfo != null)
            {
                reqInfo.TaskCompletionSource.TrySetResult(rsp);
                return;
            }
            InitActivelyPush(_instance, rspType, rsp, clientId);
            if (this.OnReceiveActivelyPushData != null)
            {
                try
                {
                    await OnReceiveActivelyPushData(clientId, rspType, rsp);
                }
                catch
                {
                }
            }
        }

        private async Task RequestDataAsync(int clientId, byte[] data)
        {
            if (OnSentData is not null)
            {
                try
                {
                    await OnSentData(clientId, data);
                }
                catch
                {
                }
            }
        }

        private async Task RespondedDataAsync(int clientId, byte[] data)
        {
            if (OnReceivedData is not null)
            {
                try
                {
                    await OnReceivedData(clientId, data);
                }
                catch
                {
                }
            }
        }

        /// <inheritdoc/>
        public async Task<TRsp> RequestAsync<TReq, TRsp>(int clientId, TReq req, int timeout = -1) where TReq : IAsyncRequest
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var reqInfo = new ReqInfo()
            {
                ClientId = clientId,
                CheckBytes = req.Check(),
                RspType = [typeof(TRsp)],
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
                var sendTask = _topPortServer.SendAsync(clientId, bytes);
                if (timeoutTask == await Task.WhenAny(timeoutTask, sendTask))
                    throw new TimeoutException($"timeout={to}");
                await sendTask;
                await RequestDataAsync(clientId, bytes);
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
        public async Task<(TRsp1 Rsp1, TRsp2 Rsp2)> RequestAsync<TReq, TRsp1, TRsp2>(int clientId, TReq req, int timeout = -1) where TReq : IAsyncRequest
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var reqInfo = new ReqInfo()
            {
                ClientId = clientId,
                CheckBytes = req.Check(),
                RspType = [typeof(TRsp1), typeof(TRsp2)],
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
                var sendTask = _topPortServer.SendAsync(clientId, bytes);
                if (sendTask != await Task.WhenAny(timeoutTask, sendTask))
                    throw new TimeoutException($"timeout={to}");
                await sendTask;
                await RequestDataAsync(clientId, bytes);
                if (tcs.Task != await Task.WhenAny(timeoutTask, tcs.Task))
                    throw new TimeoutException($"rec time out={to}");
                var rs1 = (TRsp1)await tcs.Task;
                var timeoutTask1 = Task.Delay(to);
                tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
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
        public async Task<(TRsp1 Rsp1, IEnumerable<TRsp2> Rsp2, TRsp3 Rsp3)> RequestAsync<TReq, TRsp1, TRsp2, TRsp3>(int clientId, TReq req, int timeout = -1)
            where TReq : IAsyncRequest
            where TRsp2 : IRspEnumerable
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var reqInfo = new ReqInfo()
            {
                ClientId = clientId,
                CheckBytes = req.Check(),
                RspType = [typeof(TRsp1), typeof(TRsp2), typeof(TRsp3)],
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
                var sendTask = _topPortServer.SendAsync(clientId, bytes);
                if (sendTask != await Task.WhenAny(timeoutTask, sendTask))
                    throw new TimeoutException($"timeout={to}");
                await sendTask;
                await RequestDataAsync(clientId, bytes);
                if (tcs.Task != await Task.WhenAny(timeoutTask, tcs.Task))
                    throw new TimeoutException($"rec time out={to}");
                var rs1 = (TRsp1)await tcs.Task;
                var rs2 = new List<TRsp2>();
                TRsp2 trs2;
                do
                {
                    var timeoutTask1 = Task.Delay(to);
                    tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                    reqInfo.TaskCompletionSource = tcs;
                    if (timeoutTask1 == await Task.WhenAny(timeoutTask1, tcs.Task))
                        throw new TimeoutException($"rspEnumerable time out={to}");
                    trs2 = (TRsp2)await tcs.Task;
                    rs2.Add(trs2);
                } while (!await trs2.IsFinish());
                var timeoutTask2 = Task.Delay(to);
                tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                reqInfo.TaskCompletionSource = tcs;
                if (timeoutTask2 == await Task.WhenAny(timeoutTask2, tcs.Task))
                    throw new TimeoutException($"exe time out={to}");
                var rs3 = (TRsp3)await tcs.Task;
                return (rs1, rs2, rs3);
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
        public async Task SendAsync<TReq>(int clientId, TReq req, int timeout = -1) where TReq : IByteStream
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var timeoutTask = Task.Delay(to);
            var bytes = req.ToBytes();
            var sendTask = _topPortServer.SendAsync(clientId, bytes);
            if (timeoutTask == await Task.WhenAny(timeoutTask, sendTask))
                throw new TimeoutException($"send timeout={to}");
            await sendTask;
            await RequestDataAsync(clientId, bytes);
        }

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            await _topPortServer.OpenAsync();
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            await _topPortServer.CloseAsync();
            lock (_reqInfos)
            {
                _reqInfos.Clear();
            }
        }

        class ReqInfo
        {
            public int ClientId;
            public byte[]? CheckBytes { get; set; }
            public List<Type> RspType { get; set; } = [];
            public TaskCompletionSource<object> TaskCompletionSource { get; set; } = null!;
        }
    }
}
