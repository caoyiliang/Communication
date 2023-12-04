using Communication.Exceptions;
using Communication.Interfaces;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Communication.Bus.PhysicalPort
{
    /// <summary>
    /// TCP客户端
    /// </summary>
    /// <param name="hostName">域名/IP</param>
    /// <param name="port">端口</param>
    public class TcpClient(string hostName, int port) : IPhysicalPort, IDisposable
    {
        private System.Net.Sockets.TcpClient? _client;
        private NetworkStream? _networkStream;
        private bool _disposed = false;

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            _networkStream?.Close();
            _client?.Close();
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
                // 获取底层Socket对象
                Socket socket = _client.Client;

                // 启用Keep-Alive
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                // 设置Keep-Alive参数
                //socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 100);
                //socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 100);
                //socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 1);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    _ = socket.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 100, 3), null);

                await _client.ConnectAsync(hostName, port);
                _networkStream = _client.GetStream();
            }
            catch (Exception e)
            {
                throw new ConnectFailedException($"建立TCP连接失败:{hostName}:{port}", e);
            }
        }

        private static byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
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
                    _networkStream?.Dispose();
                    _client?.Dispose();
                }

                // 释放其他非托管资源

                _disposed = true;
            }
        }

        /// <inheritdoc/>
        ~TcpClient()
        {
            Dispose(false);
        }
    }
}
