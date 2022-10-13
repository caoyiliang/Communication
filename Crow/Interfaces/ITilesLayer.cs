namespace Crow.Interfaces
{
    /// <summary>
    /// 队列连接层
    /// </summary>
    /// <typeparam name="TReq">请求处理</typeparam>
    /// <typeparam name="TRsp">接收处理</typeparam>
    public interface ITilesLayer<TReq, TRsp>
    {
        /// <summary>
        /// 接收数据
        /// </summary>
        event Action<TRsp> OnReceiveData;

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="data">请求处理</param>
        /// <param name="timeDelayAfterSending">发送后强制间隔时间(单位毫秒)</param>
        Task SendAsync(TReq data, int timeDelayAfterSending);
    }
}
