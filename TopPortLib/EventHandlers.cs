using Parser.Interfaces;

namespace TopPortLib
{
    /// <summary>
    /// 请求日志推送
    /// </summary>
    /// <param name="data">请求字节数组</param>
    public delegate Task RequestedLogEventHandler(byte[] data);
    /// <summary>
    /// 接收日志推送
    /// </summary>
    /// <param name="data">接收字节数组</param>
    public delegate Task RespondedLogEventHandler(byte[] data);
    /// <summary>
    /// 接收到主动上传数据推送
    /// </summary>
    /// <param name="type">接收数据类型</param>
    /// <param name="data">接收数据</param>
    public delegate Task ReceiveActivelyPushDataEventHandler(Type type, object data);
    /// <summary>
    /// 请求日志推送
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="data">请求字节数组</param>
    public delegate Task RequestedLogServerEventHandler(int clientId, byte[] data);
    /// <summary>
    /// 接收日志推送
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="data">接收字节数组</param>
    public delegate Task RespondedLogServerEventHandler(int clientId, byte[] data);
    /// <summary>
    /// 接收到主动上传数据推送
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="type">接收数据类型</param>
    /// <param name="data">接收数据</param>
    public delegate Task ReceiveActivelyPushDataServerEventHandler(int clientId, Type type, object data);
    /// <summary>
    /// 设置解析器
    /// </summary>
    /// <returns>解析器</returns>
    public delegate Task<IParser> GetParserEventHandler();
    /// <summary>
    /// 服务端解析数据推送
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="data">收到字节数组</param>
    public delegate Task ReceiveParsedDataFromClientEventHandler(int clientId, byte[] data);
    /// <summary>
    /// 校验接收数据是否正确
    /// </summary>
    /// <param name="data">待校验数据</param>
    /// <returns>接收是否正确</returns>
    public delegate Task<bool> CheckEventHandler(byte[] data);
}
