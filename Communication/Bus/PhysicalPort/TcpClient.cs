using Communication.Exceptions;
using Communication.Interfaces;
using System.Net.Sockets;

namespace Communication.Bus.PhysicalPort
{
    /// <summary>
    /// TCP客户端
    /// </summary>
    public class TcpClient : IPhysicalPort, IDisposable
    {
        private System.Net.Sockets.TcpClient? _client;
        private NetworkStream? _networkStream;
        private string _hostName;
        private int _port;
        /// <summary>
        /// TCP客户端
        /// </summary>
        /// <param name="hostName">域名/IP</param>
        /// <param name="port">端口</param>
        public TcpClient(string hostName, int port)
        {
            _hostName = hostName;
            _port = port;
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            this._networkStream?.Close();
            this._client?.Close();
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
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
                    if (_client.Client.Poll(1, SelectMode.SelectRead) && (_client.Available == 0))
                        return false;
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            try
            {
                _client = new System.Net.Sockets.TcpClient();
                await _client.ConnectAsync(_hostName, _port);
                _networkStream = _client.GetStream();
            }
            catch (Exception e)
            {
                throw new ConnectFailedException($"建立TCP连接失败:{_hostName}:{_port}", e);
            }
        }

        /// <inheritdoc/>
        public async Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken)
        {
            var data = new byte[count];
            int length = await _networkStream!.ReadAsync(data, 0, count, cancellationToken);
            return new ReadDataResult
            {
                Length = length,
                Data = data
            };
        }

        /// <inheritdoc/>
        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await _networkStream!.WriteAsync(data, 0, data.Length, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _networkStream?.Dispose();
            _client?.Dispose();
        }
    }
}
