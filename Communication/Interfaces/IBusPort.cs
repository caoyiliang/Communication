/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：IBusPort.cs
********************************************************************/

using Communication.Exceptions;

namespace Communication.Interfaces
{
    public interface IBusPort : IDisposable
    {
        /// <summary>
        /// 物理口
        /// </summary>
        IPhysicalPort PhysicalPort { get; set; }

        event ReceiveOriginalDataEventHandler OnReceiveOriginalData;
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ConnectFailedException"></exception>
        /// <returns></returns>
        Task OpenAsync();

        Task CloseAsync();

        Task SendAsync(byte[] data, int timeInterval = 0);
    }
}
