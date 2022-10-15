using Communication;
using Communication.Interfaces;
using Crow;
using Crow.Interfaces;
using Parser.Interfaces;
using TopPortLib.Exceptions;
using TopPortLib.Interfaces;

namespace TopPortLib
{
    /// <summary>
    /// 带队列的通讯口
    /// </summary>
    public class CrowPort : ICrowPort, IDisposable
    {
        private readonly ICrowLayer<byte[], byte[]> _crowLayer;
        private readonly ITilesLayer<byte[], byte[]> _tilesLayer;
        private readonly ITopPort _topPort;
        /// <inheritdoc/>
        public event SentDataEventHandler<byte[]>? OnSentData;
        /// <inheritdoc/>
        public event ReceivedDataEventHandler<byte[]>? OnReceivedData;
        /// <inheritdoc/>
        public event DisconnectEventHandler? OnDisconnect { add => _topPort.OnDisconnect += value; remove => _topPort.OnDisconnect -= value; }
        /// <inheritdoc/>
        public event ConnectEventHandler? OnConnect { add => _topPort.OnConnect += value; remove => _topPort.OnConnect -= value; }
        /// <inheritdoc/>
        public IPhysicalPort PhysicalPort { get => _topPort.PhysicalPort; set => _topPort.PhysicalPort = value; }
        /// <summary>
        /// 带队列的通讯口
        /// </summary>
        /// <param name="topPort">通讯口</param>
        /// <param name="defaultTimeout">默认超时时间，默认为5秒</param>
        /// <param name="timeDelayAfterSending">防止数据黏在一起，设置一个发送时间间隔</param>
        public CrowPort(ITopPort topPort, int defaultTimeout = 5000, int timeDelayAfterSending = 20)
        {
            _topPort = topPort;
            _tilesLayer = new TilesLayer(_topPort);
            _crowLayer = new CrowLayer<byte[], byte[]>(_tilesLayer, defaultTimeout, timeDelayAfterSending);
            _crowLayer.OnSentData += async data =>
            {
                if (OnSentData is not null)
                {
                    await OnSentData(data);
                }
            };
            _crowLayer.OnReceivedData += async data =>
            {
                if (OnReceivedData is not null)
                {
                    await OnReceivedData(data);
                }
            };
        }

        /// <summary>
        /// 带队列的通讯口
        /// </summary>
        /// <param name="physicalPort">物理口</param>
        /// <param name="parser">解析器</param>
        /// <param name="defaultTimeout">默认超时时间，默认为5秒</param>
        /// <param name="timeDelayAfterSending">防止数据黏在一起，设置一个发送时间间隔</param>
        public CrowPort(IPhysicalPort physicalPort, IParser parser, int defaultTimeout = 5000, int timeDelayAfterSending = 20) : this(new TopPort(physicalPort, parser), defaultTimeout, timeDelayAfterSending)
        {
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            await _crowLayer.StopAsync();
            await _topPort.CloseAsync();
        }

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            await _topPort.OpenAsync();
            await _crowLayer.StartAsync();
        }

        /// <inheritdoc/>
        public async Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, int timeout = -1, bool background = true) where TReq : IByteStream
        {
            byte[] reqBytes;
            try
            {
                reqBytes = req.ToBytes();
            }
            catch (Exception ex)
            {
                throw new RequestParameterToBytesFailedException("Request parameter to bytes failed", ex);
            }
            var rspBytes = await _crowLayer.RequestAsync(reqBytes, timeout, background);

            try
            {
                var rsp = typeof(TRsp).GetConstructor(new Type[] { typeof(TReq), typeof(byte[]) });
                if (rsp is not null)
                {
                    return (TRsp)rsp.Invoke(new object[] { req, rspBytes });
                }
                else
                {
                    rsp = typeof(TRsp).GetConstructor(new Type[] { typeof(byte[]), typeof(byte[]) });
                    if (rsp is not null)
                    {
                        return (TRsp)rsp.Invoke(new object[] { reqBytes, rspBytes });
                    }
                    else
                    {
                        rsp = typeof(TRsp).GetConstructor(new Type[] { typeof(string), typeof(byte[]) });
                        if (rsp is not null)
                        {
                            return (TRsp)rsp.Invoke(new object[] { req.ToString(), rspBytes });
                        }
                        else
                        {
                            rsp = typeof(TRsp).GetConstructor(new Type[] { typeof(byte[]) });
                            return (TRsp)rsp.Invoke(new object[] { rspBytes });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ResponseParameterCreateFailedException("ResponseParameterCreateFailedException", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<byte[], TRsp> makeRsp, int timeout = -1, bool background = true) where TReq : IByteStream
        {
            byte[] reqBytes;
            try
            {
                reqBytes = req.ToBytes();
            }
            catch (Exception ex)
            {
                throw new RequestParameterToBytesFailedException("Request parameter to bytes failed", ex);
            }
            var rspBytes = await _crowLayer.RequestAsync(reqBytes, timeout, background);
            try
            {
                return makeRsp(rspBytes);
            }
            catch (Exception ex)
            {
                throw new ResponseParameterCreateFailedException("ResponseParameterCreateFailedException", ex);
            }
        }

        /// <inheritdoc/>
        public async Task RequestAsync<TReq>(TReq req, int timeout = -1, bool background = true) where TReq : IByteStream
        {
            byte[] reqBytes;
            try
            {
                reqBytes = req.ToBytes();
            }
            catch (Exception ex)
            {
                throw new RequestParameterToBytesFailedException("Request parameter to bytes failed", ex);
            }
            await _crowLayer.SendAsync(reqBytes, timeout, background);
        }

        /// <inheritdoc/>
        public async Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<byte[], byte[], TRsp> makeRsp, int timeout = -1, bool background = true) where TReq : IByteStream
        {
            byte[] reqBytes;
            try
            {
                reqBytes = req.ToBytes();
            }
            catch (Exception ex)
            {
                throw new RequestParameterToBytesFailedException("Request parameter to bytes failed", ex);
            }
            var rspBytes = await _crowLayer.RequestAsync(reqBytes, timeout, background);
            try
            {
                return makeRsp(reqBytes, rspBytes);
            }
            catch (Exception ex)
            {
                throw new ResponseParameterCreateFailedException("ResponseParameterCreateFailedException", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<string, byte[], TRsp> makeRsp, int timeout = -1, bool background = true) where TReq : IByteStream
        {
            byte[] reqBytes;
            try
            {
                reqBytes = req.ToBytes();
            }
            catch (Exception ex)
            {
                throw new RequestParameterToBytesFailedException("Request parameter to bytes failed", ex);
            }
            var rspBytes = await _crowLayer.RequestAsync(reqBytes, timeout, background);
            try
            {
                return makeRsp(req.ToString()!, rspBytes);
            }
            catch (Exception ex)
            {
                throw new ResponseParameterCreateFailedException("ResponseParameterCreateFailedException", ex);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            var task = this.CloseAsync();
            task.ConfigureAwait(false);
            task.Wait();
            GC.SuppressFinalize(this);
        }
    }
}
