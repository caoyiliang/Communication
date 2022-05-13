namespace Communication
{
    /// <summary>
    /// 接收数据推送
    /// </summary>
    /// <param name="data">收到字节数组</param>
    /// <param name="size">收到字节个数</param>
    public delegate Task ReceiveOriginalDataEventHandler(byte[] data, int size);

    /// <summary>
    /// 服务端接收数据推送
    /// </summary>
    /// <param name="data">收到字节数组</param>
    /// <param name="size">收到字节个数</param>
    /// <param name="clientId">客户端ID</param>
    public delegate Task ReceiveOriginalDataFromClientEventHandler(byte[] data, int size, int clientId);

    /// <summary>
    /// 服务端有新客户端连接
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    public delegate Task ClientConnectEventHandler(int clientId);

    /// <summary>
    /// 服务端有客户端断线
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    public delegate Task ClientDisconnectEventHandler(int clientId);
}
