using Communication.Interfaces;

namespace TopPortLib.Interfaces
{
    /// <summary>
    /// 队列通讯口
    /// </summary>
    public interface IPigeonPort
    {
        /// <summary>
        /// 物理口
        /// </summary>
        IPhysicalPort PhysicalPort { get; set; }

        /// <summary>
        /// 请求数据
        /// </summary>
        event RequestedLogEventHandler OnRequestedData;

        /// <summary>
        /// 接收数据
        /// </summary>
        event RespondedLogEventHandler OnRespondedData;

        /// <summary>
        /// 接收有效数据
        /// </summary>
        event ReceiveResponseDataEventHandler OnReceiveResponseData;

        /// <summary>
        /// 开启
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 停止
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 队列请求接收
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <typeparam name="TRsp">接收类型</typeparam>
        /// <param name="req">请求处理</param>
        /// <param name="timeout">超时，默认使用构造传入</param>
        /// <returns>接收类型</returns>
        Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, int timeout = -1) where TReq : IByteStream;

        /// <summary>
        /// 队列只发
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <param name="req">请求处理</param>
        /// <param name="timeout">超时，默认使用构造传入</param>
        Task SendAsync<TReq>(TReq req, int timeout = -1) where TReq : IByteStream;
    }
}
