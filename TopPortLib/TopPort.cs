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
        private readonly bool _autogesto;

        /// <inheritdoc/>
        public IPhysicalPort PhysicalPort { get => _port.PhysicalPort; set => _port.PhysicalPort = value; }
        /// <inheritdoc/>
        public event ReceiveParsedDataEventHandler? OnReceiveParsedData { add => _parser.OnReceiveParsedData += value; remove => _parser.OnReceiveParsedData -= value; }
        /// <inheritdoc/>
        public event DisconnectEventHandler? OnDisconnect { add => _port.OnDisconnect += value; remove => _port.OnDisconnect -= value; }
        /// <inheritdoc/>
        public event ConnectEventHandler? OnConnect { add => _port.OnConnect += value; remove => _port.OnConnect -= value; }

        /// <summary>
        /// 顶层通讯口
        /// </summary>
        /// <param name="physicalPort">物理口</param>
        /// <param name="parser">解析器</param>
        public TopPort(IPhysicalPort physicalPort, IParser parser)
        {
            _parser = parser;
            _port = new BusPort(physicalPort);
            _port.OnReceiveOriginalData += parser.ReceiveOriginalDataAsync;
        }

        /// <summary>
        /// 该构造为485等共用通讯链路的情况准备
        /// 使用该构造IBusPort的开关将自行管理
        /// </summary>
        /// <param name="busPort">物理口总线</param>
        /// <param name="parser">解析器</param>
        public TopPort(IBusPort busPort, IParser parser)
        {
            _autogesto = true;
            _parser = parser;
            _port = busPort;
            _port.OnReceiveOriginalData += parser.ReceiveOriginalDataAsync;
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            if (!_autogesto) await _port.CloseAsync();
        }

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            if (!_autogesto) await _port.OpenAsync();
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
