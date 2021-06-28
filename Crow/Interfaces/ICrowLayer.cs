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
 * * 说明：ICrowLayer.cs
 * *
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
