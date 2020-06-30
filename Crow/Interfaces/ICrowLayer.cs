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
        event RequestedDataEventHandler<TReq> OnRequestedData;

        event RespondedDataEventHandler<TRsp> OnRespondedData;
        Task StartAsync();

        Task StopAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <returns></returns>
        Task<TRsp> RequestAsync(TReq req, int timeout = -1);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <returns></returns>
        Task SendAsync(TReq req, int timeout = -1);
    }
}
