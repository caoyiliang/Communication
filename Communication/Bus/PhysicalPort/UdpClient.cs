using Communication.Exceptions;
using Communication.Interfaces;

namespace Communication.Bus.PhysicalPort
{
    /// <summary>
    /// UDP
    /// </summary>
    public class UdpClient : IPhysicalPort, IDisposable
    {
        private System.Net.Sockets.UdpClient? _client;
        private readonly string _hostName;
        private readonly int _port;
        /// <inheritdoc/>
        public bool IsOpen { get; private set; }
        /// <summary>
        /// UDP
        /// </summary>
        /// <param name="hostName">HostName</param>
        /// <param name="port">Port</param>
        public UdpClient(string hostName, int port)
        {
            this._hostName = hostName;
            this._port = port;
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            this._client?.Close();
            this.IsOpen = false;
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
                _client = new System.Net.Sockets.UdpClient(this._hostName, this._port);
                IsOpen = true;
            }
            catch (Exception e)
            {
                throw new ConnectFailedException($"建立UDP连接失败:{this._hostName}:{this._port}", e);
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
    }
}
