using Communication.Exceptions;
using Communication.Interfaces;
using System.IO.Pipes;

namespace Communication.Bus.PhysicalPort
{
    /// <summary>
    /// 命名管道客户端
    /// </summary>
    public class NamedPipeClient : IPhysicalPort, IDisposable
    {
        private string _pipeName;
        private NamedPipeClientStream _client;
        /// <inheritdoc/>
        public bool IsOpen { get => _client.IsConnected; }

        /// <summary>
        /// 命名管道客户端
        /// </summary>
        /// <param name="pipeName">名称</param>
        public NamedPipeClient(string pipeName)
        {
            _pipeName = pipeName;
            _client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            this._client?.Close();
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _client?.Dispose();
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
                throw new ConnectFailedException($"建立NamedPipe连接失败:{this._pipeName}", e);
            }
        }

        /// <inheritdoc/>
        public async Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken)
        {
            var data = new byte[count];
            int length = await this._client.ReadAsync(data, 0, count, cancellationToken);
            return new ReadDataResult
            {
                Length = length,
                Data = data
            };
        }

        /// <inheritdoc/>
        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await this._client.WriteAsync(data, 0, data.Length, cancellationToken);
        }
    }
}
