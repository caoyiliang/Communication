using Communication;
using Communication.Bus;
using Communication.Interfaces;
using Parser;
using Parser.Interfaces;
using TopPortLib.Interfaces;

namespace TopPortLib
{
    /// <summary>
    /// 顶层通讯口
    /// </summary>
    public class TopPort : ITopPort, IDisposable
    {
        private readonly IBusPort _port;
        private readonly IParser _parser;

        /// <inheritdoc/>
        public IPhysicalPort PhysicalPort { get => _port.PhysicalPort; set => _port.PhysicalPort = value; }
        /// <inheritdoc/>
        public event ReceiveParsedDataEventHandler? OnReceiveParsedData;
        /// <inheritdoc/>
        public event DisconnectEventHandler? OnDisconnect;
        /// <inheritdoc/>
        public event ConnectEventHandler? OnConnect;

        /// <summary>
        /// 顶层通讯口
        /// </summary>
        /// <param name="physicalPort">物理口</param>
        /// <param name="parser">解析器</param>
        public TopPort(IPhysicalPort physicalPort, IParser parser)
        {
            this._parser = parser;
            parser.OnReceiveParsedData += async data =>
            {
                if (OnReceiveParsedData is not null)
                {
                    await OnReceiveParsedData.Invoke(data);
                }
            };

            this._port = new BusPort(physicalPort);
            _port.OnDisconnect += OnDisconnect;
            _port.OnConnect += OnConnect;
            _port.OnReceiveOriginalData += parser.ReceiveOriginalDataAsync;
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            await _port.CloseAsync();
        }

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            await _port.OpenAsync();
        }

        /// <inheritdoc/>
        public async Task SendAsync(byte[] data, int timeInterval = 0)
        {
            await _port.SendAsync(data, timeInterval);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_parser is IDisposable needDisposingParser)
            {
                needDisposingParser.Dispose();
            }
            var task = this.CloseAsync();
            task.ConfigureAwait(false);
            task.Wait();
            GC.SuppressFinalize(this);
        }
    }
}
