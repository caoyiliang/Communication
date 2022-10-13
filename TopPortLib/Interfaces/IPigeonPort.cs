using Communication;
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
        event RequestedLogEventHandler OnSentData;

        /// <summary>
        /// 接收数据
        /// </summary>
        event RespondedLogEventHandler OnReceivedData;

        /// <summary>
        /// 接收到主动上传的数据
        /// </summary>
        event ReceiveActivelyPushDataEventHandler OnReceiveActivelyPushData;

        /// <summary>
        /// 对端掉线
        /// </summary>
        event DisconnectEventHandler? OnDisconnect;

        /// <summary>
        /// 对端连接成功
        /// </summary>
        event ConnectEventHandler? OnConnect;

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
