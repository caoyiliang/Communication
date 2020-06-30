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

        Task<int> ReadDataAsync(byte[] data, int count, CancellationToken cancellationToken);
    }
}
