namespace Communication
{
    /// <summary>
    /// 发出的数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">发出的数据</param>
    public delegate Task SentDataEventHandler<T>(T data);

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
    public delegate Task ReceiveOriginalDataFromClientEventHandler(byte[] data, int size, Guid clientId);

    /// <summary>
    /// 服务端有新客户端连接
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    public delegate Task ClientConnectEventHandler(Guid clientId);

    /// <summary>
    /// 服务端有客户端断线
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    public delegate Task ClientDisconnectEventHandler(Guid clientId);

    /// <summary>
    /// 对端断线
    /// </summary>
    public delegate Task DisconnectEventHandler();

    /// <summary>
    /// 对端连接成功
    /// </summary>
    public delegate Task ConnectEventHandler();

    /// <summary>
    /// 接收到主动推送的数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="objects">数据</param>
    public delegate Task ActivelyPushDataEventHandler<T>(T objects);

    /// <summary>
    /// 接收到主动推送的数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="clientId">客户端ID</param>
    /// <param name="objects">数据</param>
    public delegate Task ActivelyPushDataServerEventHandler<T>(Guid clientId, T objects);
}
