/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：IPhysicalPort.cs
********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication.Interfaces
{
    public interface IPhysicalPort : IDisposable
    {
        bool IsOpen { get; }

        Task OpenAsync();

        Task CloseAsync();

        Task SendDataAsync(byte[] data, CancellationToken cancellationToken);

        Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken);
    }
    public class ReadDataResult
    {
        public byte[] Data { get; set; }
        public int Length { get; set; }
    }
}
