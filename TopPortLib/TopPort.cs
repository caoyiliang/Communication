using Communication;
using Communication.Bus;
using Communication.Interfaces;
using Parser;
using Parser.Interfaces;
using TopPortLib.Interfaces;
using System.Collections.Generic;
using System;

namespace TopPortLib
{
    /// <summary>
    /// 顶层通讯口
    /// </summary>
    public class TopPort : ITopPort, IDisposable
    {
        private readonly IBusPort _port;
        private IParser _parser;
        // 存储OnReceiveParsedData的订阅者
        private readonly List<ReceiveParsedDataEventHandler> _receiveParsedDataHandlers = new();

        /// <inheritdoc/>
        public IPhysicalPort PhysicalPort { get => _port.PhysicalPort; set => _port.PhysicalPort = value; }

        /// <inheritdoc/>
        public IParser Parser
        {
            get => _parser;
            set
            {
                if (_parser != null)
                {
                    _port.OnReceiveOriginalData -= _parser.ReceiveOriginalDataAsync;
                    foreach (var handler in _receiveParsedDataHandlers)
                    {
                        _parser.OnReceiveParsedData -= handler;
                    }
                }
                _parser = value;
                if (_parser != null)
                {
                    _port.OnReceiveOriginalData += _parser.ReceiveOriginalDataAsync;
                    foreach (var handler in _receiveParsedDataHandlers)
                    {
                        _parser.OnReceiveParsedData += handler;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public event SentDataEventHandler<byte[]>? OnSentData { add => _port.OnSentData += value; remove => _port.OnSentData -= value; }
        /// <inheritdoc/>
        public event ReceiveParsedDataEventHandler? OnReceiveParsedData
        {
            add
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _receiveParsedDataHandlers.Add(value);
                if (_parser != null)
                    _parser.OnReceiveParsedData += value;
            }
            remove
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _receiveParsedDataHandlers.Remove(value);
                if (_parser != null)
                    _parser.OnReceiveParsedData -= value;
            }
        }
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

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            await _port.CloseAsync();
        }

        /// <inheritdoc/>
        public async Task OpenAsync(bool reconnectAfterInitialFailure = false)
        {
            await _port.OpenAsync(reconnectAfterInitialFailure);
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
