/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：NamedPipeClient.cs
********************************************************************/

using Communication.Exceptions;
using Communication.Interfaces;
using System.IO.Pipes;

namespace Communication.Bus.PhysicalPort
{
    public class NamedPipeClient : IPhysicalPort, IDisposable
    {
        private string _pipeName;
        private NamedPipeClientStream _client;
        public bool IsOpen { get => _client.IsConnected; }

        public NamedPipeClient(string pipeName)
        {
            _pipeName = pipeName;
            _client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        public async Task CloseAsync()
        {
            this._client?.Close();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

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

        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await this._client.WriteAsync(data, 0, data.Length, cancellationToken);
        }
    }
}
