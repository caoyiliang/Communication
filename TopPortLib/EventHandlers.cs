namespace TopPortLib
{
    /// <summary>
    /// 请求数据推送
    /// </summary>
    /// <param name="data">请求字节数组</param>
    public delegate Task RequestedDataEventHandler(byte[] data);
    /// <summary>
    /// 接收数据推送
    /// </summary>
    /// <param name="data">接收字节数组</param>
    public delegate Task RespondedDataEventHandler(byte[] data);
    /// <summary>
    /// 接收有效数据推送
    /// </summary>
    /// <param name="type">接收数据类型</param>
    /// <param name="data">接收数据</param>
    public delegate Task ReceiveResponseDataEventHandler(Type type, object data);
}
