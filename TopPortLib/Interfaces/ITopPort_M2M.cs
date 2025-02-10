using Communication;
using Communication.Exceptions;
using Communication.Interfaces;

namespace TopPortLib.Interfaces
{
    /// <summary>
    /// 多对对通讯口
    /// </summary>
    public interface ITopPort_M2M : IDisposable
    {
        /// <summary>
        /// 物理口
        /// </summary>
        IPhysicalPort_M2M PhysicalPort { get; }

        /// <summary>
        /// 发送数据
        /// </summary>
        event SentDataToClientEventHandler<byte[]>? OnSentData;

        /// <summary>
        /// 推送解析数据
        /// </summary>
        event ReceiveParsedDataFromClientEventHandler? OnReceiveParsedData;

        /// <summary>
        /// 服务端有新客户端连接
        /// </summary>
        event ClientConnectEventHandler? OnClientConnect;

        /// <summary>
        /// 打开通讯口
        /// </summary>
        /// <exception cref="ConnectFailedException"></exception>
        Task OpenAsync();

        /// <summary>
        /// 关闭通讯口
        /// </summary>
        Task CloseAsync();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="data">要发送的字节数组</param>
        /// <exception cref="DriveNotFoundException">未找到clientId对应设备</exception>
        /// <returns></returns>
        Task SendAsync(Guid clientId, byte[] data);

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="hostName">目标地址</param>
        /// <param name="port">目标端口</param>
        /// <param name="data">目标数据</param>
        Task SendAsync(string hostName, int port, byte[] data);

        /// <summary>
        /// 获取客户端信息
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <returns>客户端信息</returns>
        Task<string?> GetClientInfos(Guid clientId);
    }
}
