namespace Communication.Interfaces
{
    public interface ITcpServer
    {
        /// <summary>
        /// 服务器的状态，true表示启动
        /// </summary>
        bool IsActive { get; }

        event ReceiveOriginalDataFromTcpClientEventHandler OnReceiveOriginalDataFromTcpClient;

        event ClientConnectEventHandler OnClientConnect;

        event ClientDisconnectEventHandler OnClientDisconnect;

        /// <summary>
        /// 启动Server，以监听客户端的连接
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// 停止接收客户端的连接
        /// </summary>
        /// <returns></returns>
        Task StopAsync();

        Task DisconnectClientAsync(int clientId);

        Task SendDataAsync(int clientId, byte[] data);
    }
}
