using Communication.Exceptions;
using Communication.Interfaces;
using System.Net.Sockets;

namespace Communication.Bus.PhysicalPort
{
    public class TcpClient : IPhysicalPort, IDisposable
    {
        private System.Net.Sockets.TcpClient _client;
        private string _hostName;
        private NetworkStream _networkStream;
        private int _port;

        public TcpClient(string hostName, int port)
        {
            this._hostName = hostName;
            this._port = port;
        }

        public async Task CloseAsync()
        {
            this._networkStream?.Close();
            this._client?.Close();
            await Task.CompletedTask;
        }

        public bool IsOpen
        {
            get
            {
                if (_client == null)
                    return false;

                if (!_client.Connected) return false;
                // 另外说明：tcpc.Connected同tcpc.Client.Connected；
                // tcpc.Client.Connected只能表示Socket上次操作(send,recieve,connect等)时是否能正确连接到资源,
                // 不能用来表示Socket的实时连接状态。
                try
                {
                    if ((_client.Client.Poll(1, SelectMode.SelectRead)) && (_client.Available == 0))
                        return false;
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }

        public async Task OpenAsync()
        {
            try
            {
                this._client = new System.Net.Sockets.TcpClient();
                await this._client.ConnectAsync(this._hostName, this._port);
                this._networkStream = this._client.GetStream();
            }
            catch (Exception e)
            {
                throw new ConnectFailedException($"建立TCP连接失败:{this._hostName}:{ this._port}", e);
            }
        }

        public async Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken)
        {
            var data = new byte[count];
            int length = await this._networkStream.ReadAsync(data, 0, count, cancellationToken);
            return new ReadDataResult
            {
                Length = length,
                Data = data
            };
        }

        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await this._networkStream.WriteAsync(data, 0, data.Length, cancellationToken);
        }

        public void Dispose()
        {
            _networkStream?.Dispose();
            _client?.Dispose();
        }
    }
}
