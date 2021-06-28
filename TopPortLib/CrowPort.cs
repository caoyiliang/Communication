/********************************************************************
 * *
 * * 使本项目源码或本项目生成的DLL前请仔细阅读以下协议内容，如果你同意以下协议才能使用本项目所有的功能，
 * * 否则如果你违反了以下协议，有可能陷入法律纠纷和赔偿，作者保留追究法律责任的权利。
 * *
 * * 1、你可以在开发的软件产品中使用和修改本项目的源码和DLL，但是请保留所有相关的版权信息。
 * * 2、不能将本项目源码与作者的其他项目整合作为一个单独的软件售卖给他人使用。
 * * 3、不能传播本项目的源码和DLL，包括上传到网上、拷贝给他人等方式。
 * * 4、以上协议暂时定制，由于还不完善，作者保留以后修改协议的权利。
 * *
 * * Copyright ©2013-? yzlm Corporation All rights reserved.
 * * 作者： 曹一梁 QQ：347739303
 * * 请保留以上版权信息，否则作者将保留追究法律责任。
 * *
 * * 创建时间：2021-06-28
 * * 说明：CrowPort.cs
 * *
********************************************************************/

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

        public event SentDataEventHandler<byte[]> OnSentData;
        public event ReceivedDataEventHandler<byte[]> OnReceivedData;

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
                 if (!(OnSentData is null))
                 {
                     await OnSentData(data);
                 }
             };
            _crowLayer.OnReceivedData += async data =>
            {
                if (!(OnReceivedData is null))
                {
                    await OnReceivedData(data);
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
