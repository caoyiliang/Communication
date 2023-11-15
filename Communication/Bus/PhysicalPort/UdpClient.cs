using Communication.Exceptions;
using Communication.Interfaces;

namespace Communication.Bus.PhysicalPort
{
    /// <summary>
    /// UDP
    /// </summary>
    /// <param name="hostName">HostName</param>
    /// <param name="port">Port</param>
    public class UdpClient(string hostName, int port) : IPhysicalPort, IDisposable
    {
        private System.Net.Sockets.UdpClient? _client;
        private bool _disposed = false;
        /// <inheritdoc/>
        public bool IsOpen { get; private set; }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            _client?.Close();
            IsOpen = false;
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            try
            {
                _client = new System.Net.Sockets.UdpClient(hostName, port);
                IsOpen = true;
            }
            catch (Exception e)
            {
                throw new ConnectFailedException($"建立UDP连接失败:{hostName}:{port}", e);
            }
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken)
        {
            var result = await _client!.ReceiveAsync();
            return new ReadDataResult
            {
                Data = result.Buffer,
                Length = result.Buffer.Length
            };
        }

        /// <inheritdoc/>
        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await _client!.SendAsync(data, data.Length);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    _client?.Dispose();
                }

                // 释放其他非托管资源

                _disposed = true;
            }
        }

        /// <inheritdoc/>
        ~UdpClient()
        {
            Dispose(false);
        }
    }
}
