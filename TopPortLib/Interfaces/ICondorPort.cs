using Communication;
using Communication.Interfaces;

namespace TopPortLib.Interfaces
{
    /// <summary>
    /// 队列通讯口
    /// </summary>
    public interface ICondorPort
    {
        /// <summary>
        /// 物理口
        /// </summary>
        IPhysicalPort_Server PhysicalPort { get; }

        /// <summary>
        /// 请求数据
        /// </summary>
        event RequestedLogServerEventHandler OnSentData;

        /// <summary>
        /// 接收数据
        /// </summary>
        event RespondedLogServerEventHandler OnReceivedData;

        /// <summary>
        /// 接收到主动上传的数据
        /// </summary>
        event ReceiveActivelyPushDataServerEventHandler OnReceiveActivelyPushData;

        /// <summary>
        /// 服务端有新客户端连接
        /// </summary>
        event ClientConnectEventHandler? OnClientConnect;

        /// <summary>
        /// 服务端有客户端断线
        /// </summary>
        event ClientDisconnectEventHandler? OnClientDisconnect;

        /// <summary>
        /// 开启监听
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 停止监听
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 队列请求接收
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <typeparam name="TRsp">接收类型</typeparam>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="clientId">客户端ID</param>
        /// <param name="req">请求处理</param>
        /// <param name="timeout">超时，默认使用构造传入</param>
        /// <returns>接收类型</returns>
        Task<TRsp> RequestAsync<TReq, TRsp, T>(int clientId, TReq req, int timeout = -1)
            where TReq : IByteStream
            where TRsp : IAsyncResponse<T>;

        /// <summary>
        /// 队列只发
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <param name="clientId">客户端ID</param>
        /// <param name="req">请求处理</param>
        /// <param name="timeout">超时，默认使用构造传入</param>
        Task SendAsync<TReq>(int clientId, TReq req, int timeout = -1) where TReq : IByteStream;
    }
}
