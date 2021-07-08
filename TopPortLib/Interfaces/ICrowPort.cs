/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：ICrowPort.cs
********************************************************************/

using Communication.Interfaces;
using System;
using Crow.Exceptions;
using System.Threading.Tasks;
using TopPortLib.Exceptions;
using Communication.Exceptions;
using Crow;

namespace TopPortLib.Interfaces
{
    public interface ICrowPort
    {
        IPhysicalPort PhysicalPort { get; set; }

        event SentDataEventHandler<byte[]> OnSentData;

        event ReceivedDataEventHandler<byte[]> OnReceivedData;
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ConnectFailedException"></exception>
        /// <returns></returns>
        Task OpenAsync();

        Task CloseAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TReq"></typeparam>
        /// <typeparam name="TRsp"></typeparam>
        /// <param name="req"></param>
        /// <param name="makeRsp"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <exception cref="RequestParameterToBytesFailedException">Request parameter to bytes failed</exception>
        /// <exception cref="ResponseParameterCreateFailedException">Response parameter create failed</exception>
        /// <returns></returns>
        Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<byte[], TRsp> makeRsp, int timeout = -1, bool background = false) where TReq : IByteStream;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TReq"></typeparam>
        /// <typeparam name="TRsp"></typeparam>
        /// <param name="req"></param>
        /// <param name="makeRsp"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <exception cref="RequestParameterToBytesFailedException">Request parameter to bytes failed</exception>
        /// <exception cref="ResponseParameterCreateFailedException">Response parameter create failed</exception>
        /// <returns></returns>
        Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<byte[], byte[], TRsp> makeRsp, int timeout = -1, bool background = false) where TReq : IByteStream;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TReq"></typeparam>
        /// <typeparam name="TRsp"></typeparam>
        /// <param name="req"></param>
        /// <param name="makeRsp"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <exception cref="RequestParameterToBytesFailedException">Request parameter to bytes failed</exception>
        /// <exception cref="ResponseParameterCreateFailedException">Response parameter create failed</exception>
        /// <returns></returns>
        Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<string, byte[], TRsp> makeRsp, int timeout = -1, bool background = false) where TReq : IByteStream;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TReq"></typeparam>
        /// <param name="req"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <exception cref="RequestParameterToBytesFailedException">Request parameter to bytes failed</exception>
        /// <returns></returns>
        Task RequestAsync<TReq>(TReq req, int timeout = -1, bool background = false) where TReq : IByteStream;
    }
}
