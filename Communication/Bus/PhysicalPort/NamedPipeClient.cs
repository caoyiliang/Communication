using Communication.Exceptions;
using Communication.Interfaces;
using System.IO.Pipes;

namespace Communication.Bus.PhysicalPort
{
    /// <summary>
    /// 命名管道客户端
    /// </summary>
    /// <param name="pipeName">名称</param>
    public class NamedPipeClient(string pipeName) : IPhysicalPort, IDisposable
    {
        private readonly NamedPipeClientStream _client = new(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        private bool _disposed = false;
        /// <inheritdoc/>
        public bool IsOpen { get => _client.IsConnected; }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            _client?.Close();
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            try
            {
                await _client.ConnectAsync();
            }
            catch (Exception e)
            {
                throw new ConnectFailedException($"建立NamedPipe连接失败:{pipeName}", e);
            }
        }

        /// <inheritdoc/>
        public async Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken)
        {
            var data = new byte[count];
            int length = await _client.ReadAsync(data, 0, count, cancellationToken);
            return new ReadDataResult
            {
                Length = length,
                Data = data
            };
        }

        /// <inheritdoc/>
        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await _client.WriteAsync(data, 0, data.Length, cancellationToken);
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
        ~NamedPipeClient()
        {
            Dispose(false);
        }
    }
}
