namespace Communication.Interfaces
{
    /// <summary>
    /// 多对多通讯接口
    /// </summary>
    public interface IPhysicalPort_M2M
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
        /// <exception cref="DriveNotFoundException">未找到clientId对应设备</exception>
        Task SendDataAsync(Guid clientId, byte[] data);

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="hostName">目标地址</param>
        /// <param name="port">目标端口</param>
        /// <param name="data">目标数据</param>
        Task<Guid> SendDataAsync(string hostName, int port, byte[] data);

        /// <summary>
        /// 获取客户端信息
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <returns>客户端信息</returns>
        Task<string?> GetClientInfos(Guid clientId);
    }
}
