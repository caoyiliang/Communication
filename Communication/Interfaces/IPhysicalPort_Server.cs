namespace Communication.Interfaces
{
    /// <summary>
    /// 服务端物理口接口
    /// </summary>
    public interface IPhysicalPort_Server
    {
        /// <summary>
        /// 服务器的状态，true表示启动
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 服务端接收数据推送
        /// </summary>
        event ReceiveOriginalDataFromClientEventHandler? OnReceiveOriginalDataFromClient;

        /// <summary>
        /// 服务端有新客户端连接
        /// </summary>
        event ClientConnectEventHandler? OnClientConnect;

        /// <summary>
        /// 服务端有客户端断线
        /// </summary>
        event ClientDisconnectEventHandler? OnClientDisconnect;

        /// <summary>
        /// 启动Server，以监听客户端的连接
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 停止接收客户端的连接
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 断开客户端
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        Task DisconnectClientAsync(Guid clientId);

        /// <summary>
        /// 向客户端发送数据
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="data">发送字节数组</param>
        Task SendDataAsync(Guid clientId, byte[] data);

        /// <summary>
        /// 获取客户端信息
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <returns>客户端信息</returns>
        Task<string?> GetClientInfos(Guid clientId);
    }
}
