using Communication.Exceptions;
using Communication.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace Communication.Bus.PhysicalPort
{
    /// <summary>
    /// UDP
    /// </summary>
    /// <param name="hostName">目标HostName</param>
    /// <param name="port">目标Port</param>
    /// <param name="iPEndPoint">本地IPEndPoint</param>
    public class UdpClient(string hostName, int port, IPEndPoint? iPEndPoint = null) : IPhysicalPort, IDisposable
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
                if (iPEndPoint != null)
                {
                    _client = new System.Net.Sockets.UdpClient(iPEndPoint);
                }
                else
                {
                    _client = new System.Net.Sockets.UdpClient();
                }
                IsOpen = true;
            }
            catch (Exception e)
            {
                throw new ConnectFailedException($"UDP打开失败", e);
            }
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken)
        {
            UdpReceiveResult result = new();
            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        result = await _client!.ReceiveAsync();
                        break;
                    }
                    catch (Exception)
                    {
                    }
                    await Task.Delay(50, cancellationToken);
                }
            }, cancellationToken);
            return new ReadDataResult
            {
                Data = result.Buffer,
                Length = result.Buffer.Length
            };
        }

        /// <inheritdoc/>
        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await _client!.SendAsync(data, data.Length, hostName, port);
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
