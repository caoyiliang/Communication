/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：ITopPort.cs
********************************************************************/

using Communication.Exceptions;
using Communication.Interfaces;
using Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
