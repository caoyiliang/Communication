using Communication;
using Communication.Interfaces;
using System.Reflection;
using System.Threading.Channels;
using TopPortLib.Exceptions;
using TopPortLib.Interfaces;
using Utils;

namespace TopPortLib
{
    /// <summary>
    /// 队列通讯口
    /// </summary>
    public class SparrowPort : ISparrowPort
    {
        private readonly object _instance;
        private readonly ITopPort_M2M _topPortM2M;
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
        public event ClientConnectEventHandler? OnClientConnect { add => _topPortM2M.OnClientConnect += value; remove => _topPortM2M.OnClientConnect -= value; }
        /// <inheritdoc/>
        public IPhysicalPort_M2M PhysicalPort { get => _topPortM2M.PhysicalPort; }
        /// <inheritdoc/>
        public CheckEventHandler? CheckEvent { get; set; }
        /// <summary>
        /// 队列通讯口
        /// </summary>
        /// <param name="instance">主动推出事件所在实体</param>
        /// <param name="topPortM2M">通讯口</param>
        /// <param name="defaultTimeout">超时时间，默认5秒</param>
        public SparrowPort(object instance, ITopPort_M2M topPortM2M, int defaultTimeout = 5000)
        {
            _instance = instance;
            _topPortM2M = topPortM2M;
            _topPortM2M.OnReceiveParsedData += TopPort_OnReceiveParsedData;
            _defaultTimeout = defaultTimeout;
            _typeList = instance.GetType().Assembly.GetTypes().Where(t => t.Namespace is not null && t.Namespace.EndsWith("Response")).ToArray();
        }

        private static void InitActivelyPush(object obj, Type type, object data, Guid clientId)
        {
            var eventMethod = obj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod).SingleOrDefault(_ => _.Name == $"{type.Name}Event");
            eventMethod?.Invoke(obj, [clientId, type.GetMethod("GetResult")!.Invoke(data, null)]);
        }

        private async Task TopPort_OnReceiveParsedData(Guid clientId, byte[] data)
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
                    var clientInfo = await _topPortM2M.PhysicalPort.GetClientInfos(clientId);
                    var rs = ((bool Type, byte[]? CheckBytes))checkMethod.Invoke(obj, [clientInfo, data])!;
                    if (rs.Type)
                    {
                        checkBytes = rs.CheckBytes;
                        var analyticalData = item.GetMethod("AnalyticalData");
                        var task = (Task?)analyticalData?.Invoke(obj, [clientInfo, data]);
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
            Channel<object>? responseChannel = null;
            lock (_reqInfos)
            {
                if (rsp is IRspEnumerable rspEnumerable)
                {
                    if (rspEnumerable.NeedCheck())
                    {
                        reqInfo = _reqInfos.Find(ri => ri.ClientId == clientId && ri.RspType.Contains(rspType) && ri.CheckBytes.ValueEqual(checkBytes));
                    }
                    else
                    {
                        reqInfo = _reqInfos.Find(ri => ri.ClientId == clientId && ri.RspType.Contains(rspType));
                    }
                }
                else
                {
                    reqInfo = _reqInfos.Find(ri => ri.ClientId == clientId && ri.RspType.Contains(rspType) && ri.CheckBytes.ValueEqual(checkBytes));
                }
                if (reqInfo is not null)
                {
                    responseChannel = reqInfo.ResponseChannel;
                }
            }
            if (responseChannel is not null)
            {
                responseChannel.Writer.TryWrite(rsp);
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

        private async Task RequestDataAsync(Guid clientId, byte[] data)
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

        private async Task RespondedDataAsync(Guid clientId, byte[] data)
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

        private void AddReqInfo(ReqInfo reqInfo)
        {
            lock (_reqInfos)
            {
                _reqInfos.Add(reqInfo);
            }
        }

        private void RemoveReqInfo(ReqInfo reqInfo)
        {
            lock (_reqInfos)
            {
                _reqInfos.Remove(reqInfo);
            }
            reqInfo.ResponseChannel.Writer.TryComplete();
        }

        private async Task SendWithTimeoutAsync(Guid clientId, byte[] bytes, int timeout)
        {
            using var cts = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeout, cts.Token);
            var sendTask = _topPortM2M.SendAsync(clientId, bytes);
            if (timeoutTask == await Task.WhenAny(timeoutTask, sendTask))
            {
                throw new TimeoutException($"{await _topPortM2M.PhysicalPort.GetClientInfos(clientId)} send timeout={timeout}");
            }
            cts.Cancel();
            await sendTask;
        }

        private async Task<TResponse> ReadResponseWithTimeoutAsync<TResponse>(
            Guid clientId,
            ChannelReader<object> reader,
            int timeout,
            string timeoutHint,
            List<object>? pendingResponses = null)
        {
            pendingResponses ??= [];
            for (int i = 0; i < pendingResponses.Count; i++)
            {
                if (pendingResponses[i] is TResponse pending)
                {
                    pendingResponses.RemoveAt(i);
                    return pending;
                }
            }

            while (true)
            {
                using var cts = new CancellationTokenSource(timeout);
                object response;
                try
                {
                    response = await reader.ReadAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException($"{await _topPortM2M.PhysicalPort.GetClientInfos(clientId)} {timeoutHint} timeout={timeout}");
                }
                catch (ChannelClosedException ex) when (ex.InnerException is OperationCanceledException inner)
                {
                    throw inner;
                }
                catch (ChannelClosedException ex)
                {
                    throw new TimeoutException($"{await _topPortM2M.PhysicalPort.GetClientInfos(clientId)} {timeoutHint} channel closed", ex);
                }

                if (response is TResponse matched)
                {
                    return matched;
                }

                pendingResponses.Add(response);
            }
        }

        /// <inheritdoc/>
        public async Task<TRsp> RequestAsync<TReq, TRsp>(Guid clientId, TReq req, int timeout = -1) where TReq : IAsyncRequest
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var reqInfo = new ReqInfo()
            {
                ClientId = clientId,
                CheckBytes = req.Check(),
                RspType = [typeof(TRsp)],
            };
            AddReqInfo(reqInfo);
            var bytes = req.ToBytes();
            try
            {
                await SendWithTimeoutAsync(clientId, bytes, to);
                await RequestDataAsync(clientId, bytes);
                return await ReadResponseWithTimeoutAsync<TRsp>(clientId, reqInfo.ResponseChannel.Reader, to, "rec");
            }
            finally
            {
                RemoveReqInfo(reqInfo);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TRsp>> RequestEnumerableAsync<TReq, TRsp>(Guid clientId, TReq req, int timeout = -1)
            where TReq : IAsyncRequest
            where TRsp : IRspEnumerable
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var reqInfo = new ReqInfo()
            {
                ClientId = clientId,
                CheckBytes = req.Check(),
                RspType = [typeof(TRsp)],
            };
            AddReqInfo(reqInfo);
            var bytes = req.ToBytes();
            try
            {
                await SendWithTimeoutAsync(clientId, bytes, to);
                await RequestDataAsync(clientId, bytes);

                var rs = new List<TRsp>();
                TRsp trs;
                do
                {
                    trs = await ReadResponseWithTimeoutAsync<TRsp>(clientId, reqInfo.ResponseChannel.Reader, to, "rspEnumerable");
                    rs.Add(trs);
                } while (!await trs.IsFinish());

                return rs;
            }
            finally
            {
                RemoveReqInfo(reqInfo);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TRsp>> RequestEnumerableWithTimeOutAsync<TReq, TRsp>(Guid clientId, TReq req, int timeout = -1) where TReq : IAsyncRequest
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var reqInfo = new ReqInfo()
            {
                ClientId = clientId,
                CheckBytes = req.Check(),
                RspType = [typeof(TRsp)],
            };
            AddReqInfo(reqInfo);
            var bytes = req.ToBytes();
            var results = new List<TRsp>();
            try
            {
                await SendWithTimeoutAsync(clientId, bytes, to);
                await RequestDataAsync(clientId, bytes);

                while (true)
                {
                    using var readCts = new CancellationTokenSource(to);
                    object response;
                    try
                    {
                        response = await reqInfo.ResponseChannel.Reader.ReadAsync(readCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (ChannelClosedException ex) when (ex.InnerException is OperationCanceledException inner)
                    {
                        throw inner;
                    }
                    catch (ChannelClosedException)
                    {
                        break;
                    }
                    results.Add((TRsp)response);
                }
            }
            finally
            {
                RemoveReqInfo(reqInfo);
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<(TRsp1 Rsp1, TRsp2 Rsp2)> RequestAsync<TReq, TRsp1, TRsp2>(Guid clientId, TReq req, int timeout = -1) where TReq : IAsyncRequest
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var reqInfo = new ReqInfo()
            {
                ClientId = clientId,
                CheckBytes = req.Check(),
                RspType = [typeof(TRsp1), typeof(TRsp2)],
            };
            AddReqInfo(reqInfo);
            var bytes = req.ToBytes();
            try
            {
                await SendWithTimeoutAsync(clientId, bytes, to);
                await RequestDataAsync(clientId, bytes);
                var pendingResponses = new List<object>();
                var rs1 = await ReadResponseWithTimeoutAsync<TRsp1>(clientId, reqInfo.ResponseChannel.Reader, to, "rec", pendingResponses);
                var rs2 = await ReadResponseWithTimeoutAsync<TRsp2>(clientId, reqInfo.ResponseChannel.Reader, to, "exe", pendingResponses);
                return (rs1, rs2);
            }
            finally
            {
                RemoveReqInfo(reqInfo);
            }
        }

        /// <inheritdoc/>
        public async Task<(TRsp1 Rsp1, IEnumerable<TRsp2> Rsp2, TRsp3 Rsp3)> RequestAsync<TReq, TRsp1, TRsp2, TRsp3>(Guid clientId, TReq req, int timeout = -1)
            where TReq : IAsyncRequest
            where TRsp2 : IRspEnumerable
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var reqInfo = new ReqInfo()
            {
                ClientId = clientId,
                CheckBytes = req.Check(),
                RspType = [typeof(TRsp1), typeof(TRsp2), typeof(TRsp3)],
            };
            AddReqInfo(reqInfo);
            var bytes = req.ToBytes();
            try
            {
                await SendWithTimeoutAsync(clientId, bytes, to);
                await RequestDataAsync(clientId, bytes);
                var pendingResponses = new List<object>();
                var rs1 = await ReadResponseWithTimeoutAsync<TRsp1>(clientId, reqInfo.ResponseChannel.Reader, to, "rec", pendingResponses);
                var rs2 = new List<TRsp2>();
                TRsp2 trs2;
                do
                {
                    trs2 = await ReadResponseWithTimeoutAsync<TRsp2>(clientId, reqInfo.ResponseChannel.Reader, to, "rspEnumerable", pendingResponses);
                    rs2.Add(trs2);
                } while (!await trs2.IsFinish());
                var rs3 = await ReadResponseWithTimeoutAsync<TRsp3>(clientId, reqInfo.ResponseChannel.Reader, to, "exe", pendingResponses);
                return (rs1, rs2, rs3);
            }
            finally
            {
                RemoveReqInfo(reqInfo);
            }
        }

        /// <inheritdoc/>
        public async Task SendAsync<TReq>(Guid clientId, TReq req, int timeout = -1) where TReq : IByteStream
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var bytes = req.ToBytes();
            await SendWithTimeoutAsync(clientId, bytes, to);
            await RequestDataAsync(clientId, bytes);
        }

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            await _topPortM2M.OpenAsync();
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            List<ReqInfo> pendingRequests;
            lock (_reqInfos)
            {
                pendingRequests = [.. _reqInfos];
                _reqInfos.Clear();
            }
            foreach (var pendingRequest in pendingRequests)
            {
                pendingRequest.ResponseChannel.Writer.TryComplete(new OperationCanceledException("SparrowPort stopped"));
            }
            await _topPortM2M.CloseAsync();
        }

        /// <inheritdoc/>
        public async Task<string?> GetClientInfos(Guid clientId)
        {
            return await PhysicalPort.GetClientInfos(clientId);
        }

        /// <inheritdoc/>
        public async Task RemoveClientAsync(Guid clientId)
        {
            await _topPortM2M.RemoveClientAsync(clientId);
        }

        /// <inheritdoc/>
        public async Task<Guid> AddClientAsync(string hostName, int port)
        {
            return await _topPortM2M.AddClientAsync(hostName, port);
        }

        class ReqInfo
        {
            public Guid ClientId;
            public byte[]? CheckBytes { get; set; }
            public List<Type> RspType { get; set; } = [];
            public Channel<object> ResponseChannel { get; set; } = Channel.CreateUnbounded<object>();
        }
    }
}

