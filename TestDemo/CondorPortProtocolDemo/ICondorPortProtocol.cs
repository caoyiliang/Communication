using Communication;

namespace CondorPortProtocolDemo
{
    public interface ICondorPortProtocol
    {
        /// <summary>
        /// 设备是否监听
        /// </summary>
        public bool IsListened { get; }

        /// <summary>
        /// 打开监听
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 关闭监听
        /// </summary>
        Task CloseAsync();

        /// <summary>
        /// 主动上传的数据比如读浓度
        /// </summary>
        event ActivelyPushDataServerEventHandler<(List<decimal> recData, int result)>? OnReadValue;

        /// <summary>
        /// 服务端有新客户端连接
        /// </summary>
        event ClientConnectEventHandler? OnClientConnect;

        /// <summary>
        /// 服务端有客户端断线
        /// </summary>
        event ClientDisconnectEventHandler? OnClientDisconnect;

        /// <summary>
        /// 读信号量
        /// </summary>
        /// <param name="clientId">客户端Id</param>
        /// <param name="tryCount">重试次数</param>
        /// <param name="timeOut">超时时间(-1则使用构造传入超时)</param>
        /// <param name="cancelToken">取消</param>
        /// <returns>信号量值</returns>
        Task<List<decimal>?> ReadSignalValueAsync(Guid clientId, int tryCount = 0, int timeOut = -1, CancellationToken cancelToken = default);
    }
}
