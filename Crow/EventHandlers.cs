namespace Crow
{
    /// <summary>
    /// 发出的数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">发出的数据</param>
    public delegate Task SentDataEventHandler<T>(T data);

    /// <summary>
    /// 收到的数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">收到的数据</param>
    public delegate Task ReceivedDataEventHandler<T>(T data);
}
