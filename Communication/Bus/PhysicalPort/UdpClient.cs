using Communication.Exceptions;
using Communication.Interfaces;

namespace Communication.Bus.PhysicalPort
{
    public class UdpClient : IPhysicalPort, IDisposable
    {
        private System.Net.Sockets.UdpClient _client;
        private string _hostName;
        private int _port;
        public bool IsOpen { get; private set; }
        public UdpClient(string hostName, int port)
        {
            this._hostName = hostName;
            this._port = port;
        }
        public async Task CloseAsync()
        {
            this._client?.Close();
            this.IsOpen = false;
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public async Task OpenAsync()
        {
            try
            {
                this._client = new System.Net.Sockets.UdpClient(this._hostName, this._port);
                this.IsOpen = true;
            }
            catch (Exception e)
            {
                throw new ConnectFailedException($"建立UDP连接失败:{this._hostName}:{ this._port}", e);
            }
            await Task.CompletedTask;
        }

        public async Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken)
        {
            var result = await _client.ReceiveAsync();
            return new ReadDataResult
            {
                Data = result.Buffer,
                Length = result.Buffer.Length
            };
        }

        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await _client.SendAsync(data, data.Length);
        }
    }
}
