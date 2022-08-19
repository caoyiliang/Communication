using Crow.Exceptions;

namespace Crow.Interfaces
{
    /// <summary>
    /// 通讯队列
    /// </summary>
    /// <typeparam name="TReq">请求处理</typeparam>
    /// <typeparam name="TRsp">接收处理</typeparam>
    public interface ICrowLayer<TReq, TRsp>
    {
        /// <summary>
        /// 发出的数据
        /// </summary>
        event SentDataEventHandler<TReq> OnSentData;

        /// <summary>
        /// 收到的数据
        /// </summary>
        event ReceivedDataEventHandler<TRsp> OnReceivedData;

        /// <summary>
        /// 开启队列
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 停止队列
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 队列请求接收
        /// </summary>
        /// <param name="req">请求处理</param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <returns>接收类型</returns>
        Task<TRsp> RequestAsync(TReq req, int timeout = -1, bool background = true);

        /// <summary>
        /// 队列只发不收
        /// </summary>
        /// <param name="req">请求处理</param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        Task SendAsync(TReq req, int timeout = -1, bool background = true);
    }
}
