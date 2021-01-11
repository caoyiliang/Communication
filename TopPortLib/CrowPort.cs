using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Communication.Exceptions;
using Communication.Interfaces;
using Crow;
using Crow.Interfaces;
using Parser.Interfaces;
using TopPortLib.Exceptions;
using TopPortLib.Interfaces;

namespace TopPortLib
{
    public class CrowPort : ICrowPort, IDisposable
    {
        private ICrowLayer<byte[], byte[]> _crowLayer;
        private ITilesLayer<byte[], byte[]> _tilesLayer;
        private ITopPort _topPort;

        public event SentDataEventHandler<byte[]> OnRequestedData;
        public event ReceivedDataEventHandler<byte[]> OnRespondedData;

        public IPhysicalPort PhysicalPort { get => _topPort.PhysicalPort; set => _topPort.PhysicalPort = value; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="topPort"></param>
        /// <param name="defaultTimeout"></param>
        /// <param name="timeDelayAfterSending">当调用SendAsync时，防止数据黏在一起，要设置一个发送时间间隔</param>
        public CrowPort(ITopPort topPort, int defaultTimeout = 5000, int timeDelayAfterSending = 20)
        {
            _topPort = topPort;
            _tilesLayer = new TilesLayer(_topPort);
            _crowLayer = new CrowLayer<byte[], byte[]>(_tilesLayer, defaultTimeout, timeDelayAfterSending);
            _crowLayer.OnSentData += async data =>
             {
                 if (!(OnRequestedData is null))
                 {
                     await OnRequestedData(data);
                 }
             };
            _crowLayer.OnReceivedData += async data =>
            {
                if (!(OnRespondedData is null))
                {
                    await OnRespondedData(data);
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="physicalPort"></param>
        /// <param name="parser"></param>
        /// <param name="defaultTimeout"></param>
        /// <param name="timeDelayAfterSending">当调用SendAsync时，防止数据黏在一起，要设置一个发送时间间隔</param>
        public CrowPort(IPhysicalPort physicalPort, IParser parser, int defaultTimeout = 5000, int timeDelayAfterSending = 20) : this(new TopPort(physicalPort, parser), defaultTimeout, timeDelayAfterSending)
        {
        }

        public async Task CloseAsync()
        {
            await _crowLayer.StopAsync();
            await _topPort.CloseAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ConnectFailedException"></exception>
        /// <returns></returns>
        public async Task OpenAsync()
        {
            await _topPort.OpenAsync();
            await _crowLayer.StartAsync();
        }

        public async Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<byte[], TRsp> makeRsp, int timeout = -1) where TReq : IByteStream
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
            var rspBytes = await _crowLayer.RequestAsync(reqBytes, timeout);
            try
            {
                return makeRsp(rspBytes);
            }
            catch (Exception ex)
            {
                throw new ResponseParameterCreateFailedException("ResponseParameterCreateFailedException", ex);
            }
        }

        public async Task RequestAsync<TReq>(TReq req, int timeout = -1) where TReq : IByteStream
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
            await _crowLayer.SendAsync(reqBytes, timeout);
        }
        public async Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<byte[], byte[], TRsp> makeRsp, int timeout = -1) where TReq : IByteStream
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
            var rspBytes = await _crowLayer.RequestAsync(reqBytes, timeout);
            try
            {
                return makeRsp(reqBytes, rspBytes);
            }
            catch (Exception ex)
            {
                throw new ResponseParameterCreateFailedException("ResponseParameterCreateFailedException", ex);
            }
        }
        public async Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<string, byte[], TRsp> makeRsp, int timeout = -1) where TReq : IByteStream
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
            var rspBytes = await _crowLayer.RequestAsync(reqBytes, timeout);
            try
            {
                return makeRsp(req.ToString(), rspBytes);
            }
            catch (Exception ex)
            {
                throw new ResponseParameterCreateFailedException("ResponseParameterCreateFailedException", ex);
            }
        }
        public void Dispose()
        {
            var task = this.CloseAsync();
            task.ConfigureAwait(false);
            task.Wait();
        }
    }
}
