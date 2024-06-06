using Communication;
using Communication.Exceptions;
using Communication.Interfaces;
using Parser;

namespace TopPortLib.Interfaces
{
    /// <summary>
    /// 通讯口
    /// </summary>
    public interface ITopPort : IDisposable
    {
        /// <summary>
        /// 物理口
        /// </summary>
        IPhysicalPort PhysicalPort { get; set; }

        /// <summary>
        /// 请求数据
        /// </summary>
        event SentDataEventHandler<byte[]> OnSentData;

        /// <summary>
        /// 数据接收
        /// </summary>
        event ReceiveParsedDataEventHandler OnReceiveParsedData;

        /// <summary>
        /// 对端掉线
        /// </summary>
        event DisconnectEventHandler OnDisconnect;

        /// <summary>
        /// 对端连接成功
        /// </summary>
        event ConnectEventHandler OnConnect;

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
        /// <param name="data">要发送的字节数组</param>
        /// <param name="timeInterval">发送后强制间隔时间(单位毫秒)</param>
        /// <returns></returns>
        Task SendAsync(byte[] data, int timeInterval = 0);
    }
}
