/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：ICrowLayer.cs
********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Crow.Exceptions;

namespace Crow.Interfaces
{
    /// <summary>
    /// 乌鸦层
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TRsp"></typeparam>
    public interface ICrowLayer<TReq, TRsp>
    {
        event SentDataEventHandler<TReq> OnSentData;

        event ReceivedDataEventHandler<TRsp> OnReceivedData;
        Task StartAsync();

        Task StopAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <returns></returns>
        Task<TRsp> RequestAsync(TReq req, int timeout = -1, bool background = false);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <returns></returns>
        Task SendAsync(TReq req, int timeout = -1, bool background = false);
    }
}
