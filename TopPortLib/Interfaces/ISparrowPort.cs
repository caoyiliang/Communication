using Communication;
using Communication.Interfaces;

namespace TopPortLib.Interfaces
{
    /// <summary>
    /// 队列通讯口
    /// </summary>
    public interface ISparrowPort
    {
        /// <summary>
        /// 物理口
        /// </summary>
        IPhysicalPort_M2M PhysicalPort { get; }

        /// <summary>
        /// 设置校验接收数据方法
        /// </summary>
        CheckEventHandler? CheckEvent { get; set; }

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
        /// 开启监听
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 停止监听
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 获取客户端信息
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <returns>客户端信息</returns>
        Task<string?> GetClientInfos(Guid clientId);

        /// <summary>
        /// 队列请求接收
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <typeparam name="TRsp">接收类型</typeparam>
        /// <param name="clientId">客户端ID</param>
        /// <param name="req">请求处理</param>
        /// <param name="timeout">超时，默认使用构造传入</param>
        /// <returns>接收类型</returns>
        Task<TRsp> RequestAsync<TReq, TRsp>(Guid clientId, TReq req, int timeout = -1) where TReq : IAsyncRequest;

        /// <summary>
        /// 队列请求接收(2次返回)
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <typeparam name="TRsp1">接收类型1</typeparam>
        /// <typeparam name="TRsp2">接收类型2</typeparam>
        /// <param name="clientId">客户端ID</param>
        /// <param name="req">请求处理</param>
        /// <param name="timeout">超时，默认使用构造传入</param>
        /// <returns>接收类型元组</returns>
        Task<(TRsp1 Rsp1, TRsp2 Rsp2)> RequestAsync<TReq, TRsp1, TRsp2>(Guid clientId, TReq req, int timeout = -1) where TReq : IAsyncRequest;

        /// <summary>
        /// 队列请求接收(多次返回)
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <typeparam name="TRsp1">接收类型1</typeparam>
        /// <typeparam name="TRsp2">接收队列类型2</typeparam>
        /// <typeparam name="TRsp3">接收类型3</typeparam>
        /// <param name="clientId">客户端ID</param>
        /// <param name="req">请求处理</param>
        /// <param name="timeout">超时，默认使用构造传入</param>
        /// <returns>接收类型元组</returns>
        Task<(TRsp1 Rsp1, IEnumerable<TRsp2> Rsp2, TRsp3 Rsp3)> RequestAsync<TReq, TRsp1, TRsp2, TRsp3>(Guid clientId, TReq req, int timeout = -1)
            where TReq : IAsyncRequest
            where TRsp2 : IRspEnumerable;

        /// <summary>
        /// 队列只发
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <param name="clientId">客户端ID</param>
        /// <param name="req">请求处理</param>
        /// <param name="timeout">超时，默认使用构造传入</param>
        Task SendAsync<TReq>(Guid clientId, TReq req, int timeout = -1) where TReq : IByteStream;
    }
}
