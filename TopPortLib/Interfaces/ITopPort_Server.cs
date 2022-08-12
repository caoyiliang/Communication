using Communication;
using Communication.Exceptions;
using Communication.Interfaces;

namespace TopPortLib.Interfaces
{
    /// <summary>
    /// 通讯口
    /// </summary>
    public interface ITopPort_Server : IDisposable
    {
        /// <summary>
        /// 物理口
        /// </summary>
        IPhysicalPort_Server PhysicalPort { get; }

        /// <summary>
        /// 推送解析数据
        /// </summary>
        event ReceiveParsedDataFromClientEventHandler? OnReceiveParsedData;

        /// <summary>
        /// 服务端有新客户端连接
        /// </summary>
        event ClientConnectEventHandler? OnClientConnect;

        /// <summary>
        /// 服务端有客户端断线
        /// </summary>
        event ClientDisconnectEventHandler? OnClientDisconnect;

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
        /// <returns></returns>
        Task SendAsync(int clientId, byte[] data);
    }
}
