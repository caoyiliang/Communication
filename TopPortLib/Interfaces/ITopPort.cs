/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：ITopPort.cs
********************************************************************/

using Communication.Exceptions;
using Communication.Interfaces;
using Parser;

namespace TopPortLib.Interfaces
{
    public interface ITopPort : IDisposable
    {
        IPhysicalPort PhysicalPort { get; set; }

        event ReceiveParsedDataEventHandler OnReceiveParsedData;
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
