using Communication;
using Communication.Exceptions;
using Communication.Interfaces;
using Crow;
using Crow.Exceptions;
using TopPortLib.Exceptions;

namespace TopPortLib.Interfaces
{
    /// <summary>
    /// 带队列的通讯口
    /// </summary>
    public interface ICrowPort
    {
        /// <summary>
        /// 物理口
        /// </summary>
        IPhysicalPort PhysicalPort { get; set; }
        /// <summary>
        /// 发送的数据
        /// </summary>
        event SentDataEventHandler<byte[]> OnSentData;
        /// <summary>
        /// 接收的数据
        /// </summary>
        event ReceivedDataEventHandler<byte[]> OnReceivedData;
        /// <summary>
        /// 打开
        /// </summary>
        /// <exception cref="ConnectFailedException"></exception>
        Task OpenAsync();
        /// <summary>
        /// 关闭
        /// </summary>
        Task CloseAsync();
        /// <summary>
        /// 对端掉线
        /// </summary>
        event DisconnectEventHandler? OnDisconnect;
        /// <summary>
        /// 对端连接成功
        /// </summary>
        event ConnectEventHandler? OnConnect;
        /// <summary>
        /// 队列请求接收
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <typeparam name="TRsp">接收类型</typeparam>
        /// <param name="req">请求处理</param>
        /// <param name="makeRsp">接收处理</param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <exception cref="RequestParameterToBytesFailedException">Request parameter to bytes failed</exception>
        /// <exception cref="ResponseParameterCreateFailedException">Response parameter create failed</exception>
        /// <returns>接收类型</returns>
        Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<byte[], TRsp> makeRsp, int timeout = -1, bool background = false) where TReq : IByteStream;
        /// <summary>
        /// 队列请求接收
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <typeparam name="TRsp">接收类型</typeparam>
        /// <param name="req">请求处理</param>
        /// <param name="makeRsp">接收处理</param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <exception cref="RequestParameterToBytesFailedException">Request parameter to bytes failed</exception>
        /// <exception cref="ResponseParameterCreateFailedException">Response parameter create failed</exception>
        /// <returns>接收类型</returns>
        Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<byte[], byte[], TRsp> makeRsp, int timeout = -1, bool background = false) where TReq : IByteStream;
        /// <summary>
        /// 队列请求接收
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <typeparam name="TRsp">接收类型</typeparam>
        /// <param name="req">请求处理</param>
        /// <param name="makeRsp">接收处理</param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <exception cref="RequestParameterToBytesFailedException">Request parameter to bytes failed</exception>
        /// <exception cref="ResponseParameterCreateFailedException">Response parameter create failed</exception>
        /// <returns>接收类型</returns>
        Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, Func<string, byte[], TRsp> makeRsp, int timeout = -1, bool background = false) where TReq : IByteStream;
        /// <summary>
        /// 队列只发不收
        /// </summary>
        /// <typeparam name="TReq">请求类型</typeparam>
        /// <param name="req">请求处理</param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <param name="background">后台任务，当指示为true时，超时时间从发送开始计算，否则，从加入队列开始计算</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <exception cref="RequestParameterToBytesFailedException">Request parameter to bytes failed</exception>
        Task RequestAsync<TReq>(TReq req, int timeout = -1, bool background = false) where TReq : IByteStream;
    }
}
