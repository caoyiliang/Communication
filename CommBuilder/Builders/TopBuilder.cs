using CommBuilder.Interfaces;
using Communication.Bus;
using Communication.Interfaces;
using Parser;
using Parser.Interfaces;
using Parser.Parsers;
using System.IO.Ports;
using TopPortLib;
using TopPortLib.Interfaces;

namespace CommBuilder.Builders
{
    #region 顶层通讯口（点对点）

    /// <summary>
    /// 顶层通讯口 Builder - 无队列，直接收发（点对点）
    /// </summary>
    public class TopBuilder : ITopPhysicalPortStep, ITopParserStep, ITopConfigStep
    {
        private IPhysicalPort? _physicalPort;
        private IParser? _parser;
        private bool _reconnect;
        private int _sendInterval;
        private Action<byte[]>? _onReceived;
        private Func<byte[], Task>? _onReceivedAsync;
        private Action? _onConnected;
        private Func<Task>? _onConnectedAsync;
        private Action? _onDisconnected;
        private Func<Task>? _onDisconnectedAsync;
        private Action<byte[]>? _onSent;

        #region ITopPhysicalPortStep

        /// <inheritdoc/>
        public ITopParserStep UseSerial(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _physicalPort = new Communication.Bus.PhysicalPort.SerialPort(portName, baudRate, parity, dataBits, stopBits);
            return this;
        }

        /// <inheritdoc/>
        public ITopParserStep UseTcp(string host, int port)
        {
            _physicalPort = new Communication.Bus.PhysicalPort.TcpClient(host, port);
            return this;
        }

        /// <inheritdoc/>
        public ITopParserStep UseNamedPipe(string pipeName)
        {
            _physicalPort = new Communication.Bus.PhysicalPort.NamedPipeClient(pipeName);
            return this;
        }

        /// <inheritdoc/>
        public ITopParserStep FromConnectionString(string connectionString)
        {
            _physicalPort = ConnectionStringParser.Parse(connectionString);
            return this;
        }

        #endregion

        #region ITopParserStep

        /// <inheritdoc/>
        public ITopConfigStep WithHeadLengthParser(byte[] head, Func<byte[], int> lengthGetter)
        {
            _parser = new HeadLengthParser(head, data =>
            {
                try
                {
                    var len = lengthGetter(data);
                    return Task.FromResult(new GetDataLengthRsp { Length = len, StateCode = Parser.StateCode.Success });
                }
                catch
                {
                    return Task.FromResult(new GetDataLengthRsp { Length = 0, StateCode = Parser.StateCode.LengthNotEnough });
                }
            });
            return this;
        }

        /// <inheritdoc/>
        public ITopConfigStep WithHeadFootParser(byte[] head, byte[] foot)
        {
            _parser = new HeadFootParser(head, foot);
            return this;
        }

        /// <inheritdoc/>
        public ITopConfigStep WithTimeParser(int intervalMs = 50)
        {
            _parser = new TimeParser(intervalMs);
            return this;
        }

        /// <inheritdoc/>
        public ITopConfigStep WithFootParser(byte[] foot)
        {
            _parser = new FootParser(foot);
            return this;
        }

        /// <inheritdoc/>
        public ITopConfigStep WithParser(IParser parser)
        {
            _parser = parser;
            return this;
        }

        /// <inheritdoc/>
        public ITopConfigStep WithNoParser()
        {
            _parser = new NoParser();
            return this;
        }

        #endregion

        #region ITopConfigStep

        /// <inheritdoc/>
        public ITopConfigStep SendInterval(int ms) { _sendInterval = ms; return this; }

        /// <inheritdoc/>
        public ITopConfigStep AutoReconnect(bool enable = true) { _reconnect = enable; return this; }

        /// <inheritdoc/>
        public ITopConfigStep OnReceived(Action<byte[]> handler) { _onReceived = handler; return this; }

        /// <inheritdoc/>
        public ITopConfigStep OnReceived(Func<byte[], Task> handler) { _onReceivedAsync = handler; return this; }

        /// <inheritdoc/>
        public ITopConfigStep OnConnected(Action handler) { _onConnected = handler; return this; }

        /// <inheritdoc/>
        public ITopConfigStep OnConnected(Func<Task> handler) { _onConnectedAsync = handler; return this; }

        /// <inheritdoc/>
        public ITopConfigStep OnDisconnected(Action handler) { _onDisconnected = handler; return this; }

        /// <inheritdoc/>
        public ITopConfigStep OnDisconnected(Func<Task> handler) { _onDisconnectedAsync = handler; return this; }

        /// <inheritdoc/>
        public ITopConfigStep OnSent(Action<byte[]> handler) { _onSent = handler; return this; }

        /// <inheritdoc/>
        public ITopPort Build()
        {
            if (_physicalPort == null)
                throw new InvalidOperationException("必须选择物理口类型，请调用 UseSerial/UseTcp/UseNamedPipe/FromConnectionString");

            if (_parser == null)
                throw new InvalidOperationException("必须选择分包器，请调用 WithHeadLengthParser/WithHeadFootParser/WithTimeParser/WithFootParser/WithParser/WithNoParser");

            var topPort = new TopPort(_physicalPort, _parser);

            if (_onReceived != null)
                topPort.OnReceiveParsedData += data => { _onReceived(data); return Task.CompletedTask; };

            if (_onReceivedAsync != null)
                topPort.OnReceiveParsedData += async data => await _onReceivedAsync(data);

            if (_onConnected != null)
                topPort.OnConnect += () => { _onConnected(); return Task.CompletedTask; };

            if (_onConnectedAsync != null)
                topPort.OnConnect += async () => await _onConnectedAsync();

            if (_onDisconnected != null)
                topPort.OnDisconnect += () => { _onDisconnected(); return Task.CompletedTask; };

            if (_onDisconnectedAsync != null)
                topPort.OnDisconnect += async () => await _onDisconnectedAsync();

            if (_onSent != null)
                topPort.OnSentData += data => { _onSent(data); return Task.CompletedTask; };

            return topPort;
        }

        #endregion
    }

    #endregion

    #region 顶层通讯口（TCP 服务端）

    /// <summary>
    /// 顶层通讯口 Builder - TCP 服务端模式（一对多，管理多个客户端连接）
    /// </summary>
    public class TopServerBuilder : ITopServerPhysicalPortStep, ITopServerParserStep, ITopServerConfigStep
    {
        private IPhysicalPort_Server? _physicalPortServer;
        private Func<IParser>? _parserFactory;
        private IParser? _sharedParser;
        private int _sendInterval;
        private Action<Guid, byte[]>? _onReceived;
        private Func<Guid, byte[], Task>? _onReceivedAsync;
        private Action<Guid>? _onClientConnected;
        private Func<Guid, Task>? _onClientConnectedAsync;
        private Action<Guid>? _onClientDisconnected;
        private Func<Guid, Task>? _onClientDisconnectedAsync;
        private Action<byte[], Guid>? _onSent;

        #region ITopServerPhysicalPortStep

        /// <inheritdoc/>
        public ITopServerParserStep UseTcpServer(string host, int port)
        {
            _physicalPortServer = new TcpServer(host, port);
            return this;
        }

        #endregion

        #region ITopServerParserStep

        /// <inheritdoc/>
        public ITopServerConfigStep WithParserFactory(Func<IParser> parserFactory)
        {
            _parserFactory = parserFactory;
            return this;
        }

        /// <inheritdoc/>
        public ITopServerConfigStep WithHeadLengthParser(byte[] head, Func<byte[], int> lengthGetter)
        {
            _parserFactory = () => new HeadLengthParser(head, data =>
            {
                try
                {
                    var len = lengthGetter(data);
                    return Task.FromResult(new GetDataLengthRsp { Length = len, StateCode = Parser.StateCode.Success });
                }
                catch
                {
                    return Task.FromResult(new GetDataLengthRsp { Length = 0, StateCode = Parser.StateCode.LengthNotEnough });
                }
            });
            return this;
        }

        /// <inheritdoc/>
        public ITopServerConfigStep WithHeadFootParser(byte[] head, byte[] foot)
        {
            _parserFactory = () => new HeadFootParser(head, foot);
            return this;
        }

        /// <inheritdoc/>
        public ITopServerConfigStep WithTimeParser(int intervalMs = 50)
        {
            _parserFactory = () => new TimeParser(intervalMs);
            return this;
        }

        /// <inheritdoc/>
        public ITopServerConfigStep WithFootParser(byte[] foot)
        {
            _parserFactory = () => new FootParser(foot);
            return this;
        }

        /// <inheritdoc/>
        public ITopServerConfigStep WithParser(IParser parser)
        {
            _sharedParser = parser;
            return this;
        }

        /// <inheritdoc/>
        public ITopServerConfigStep WithNoParser()
        {
            _parserFactory = () => new NoParser();
            return this;
        }

        #endregion

        #region ITopServerConfigStep

        /// <inheritdoc/>
        public ITopServerConfigStep SendInterval(int ms) { _sendInterval = ms; return this; }

        /// <inheritdoc/>
        public ITopServerConfigStep OnReceived(Action<Guid, byte[]> handler) { _onReceived = handler; return this; }

        /// <inheritdoc/>
        public ITopServerConfigStep OnReceived(Func<Guid, byte[], Task> handler) { _onReceivedAsync = handler; return this; }

        /// <inheritdoc/>
        public ITopServerConfigStep OnClientConnected(Action<Guid> handler) { _onClientConnected = handler; return this; }

        /// <inheritdoc/>
        public ITopServerConfigStep OnClientConnected(Func<Guid, Task> handler) { _onClientConnectedAsync = handler; return this; }

        /// <inheritdoc/>
        public ITopServerConfigStep OnClientDisconnected(Action<Guid> handler) { _onClientDisconnected = handler; return this; }

        /// <inheritdoc/>
        public ITopServerConfigStep OnClientDisconnected(Func<Guid, Task> handler) { _onClientDisconnectedAsync = handler; return this; }

        /// <inheritdoc/>
        public ITopServerConfigStep OnSent(Action<byte[], Guid> handler) { _onSent = handler; return this; }

        /// <inheritdoc/>
        public ITopPort_Server Build()
        {
            if (_physicalPortServer == null)
                throw new InvalidOperationException("必须选择服务端物理口类型，请调用 UseTcpServer");

            if (_parserFactory == null && _sharedParser == null)
                throw new InvalidOperationException("必须选择分包器，请调用 WithHeadLengthParser/WithHeadFootParser/WithTimeParser/WithFootParser/WithParser/WithNoParser 或 WithParserFactory");

            IParser CreateParser() => _parserFactory != null ? _parserFactory() : _sharedParser!;

            var topPort = new TopPort_Server(_physicalPortServer, () => Task.FromResult(CreateParser()));

            if (_onReceived != null)
                topPort.OnReceiveParsedData += (clientId, data) => { _onReceived(clientId, data); return Task.CompletedTask; };

            if (_onReceivedAsync != null)
                topPort.OnReceiveParsedData += async (clientId, data) => await _onReceivedAsync(clientId, data);

            if (_onClientConnected != null)
                topPort.OnClientConnect += clientId => { _onClientConnected(clientId); return Task.CompletedTask; };

            if (_onClientConnectedAsync != null)
                topPort.OnClientConnect += async clientId => await _onClientConnectedAsync(clientId);

            if (_onClientDisconnected != null)
                topPort.OnClientDisconnect += clientId => { _onClientDisconnected(clientId); return Task.CompletedTask; };

            if (_onClientDisconnectedAsync != null)
                topPort.OnClientDisconnect += async clientId => await _onClientDisconnectedAsync(clientId);

            if (_onSent != null)
                topPort.OnSentData += (data, clientId) => { _onSent(data, clientId); return Task.CompletedTask; };

            return topPort;
        }

        #endregion
    }

    #endregion

    #region 顶层通讯口（UDP 多对多）

    /// <summary>
    /// 顶层通讯口 Builder - UDP 多对多模式（M2M，无连接状态管理）
    /// </summary>
    public class TopM2MBuilder : ITopM2MPhysicalPortStep, ITopM2MParserStep, ITopM2MConfigStep
    {
        private IPhysicalPort_M2M? _physicalPortM2M;
        private Func<IParser>? _parserFactory;
        private IParser? _sharedParser;
        private int _sendInterval;
        private Action<Guid, byte[]>? _onReceived;
        private Func<Guid, byte[], Task>? _onReceivedAsync;
        private Action<Guid>? _onClientConnected;
        private Func<Guid, Task>? _onClientConnectedAsync;
        private Action<byte[], Guid>? _onSent;

        #region ITopM2MPhysicalPortStep

        /// <inheritdoc/>
        public ITopM2MParserStep UseUdp(string host, int port)
        {
            _physicalPortM2M = new Udp(host, port);
            return this;
        }

        #endregion

        #region ITopM2MParserStep

        /// <inheritdoc/>
        public ITopM2MConfigStep WithParserFactory(Func<IParser> parserFactory)
        {
            _parserFactory = parserFactory;
            return this;
        }

        /// <inheritdoc/>
        public ITopM2MConfigStep WithHeadLengthParser(byte[] head, Func<byte[], int> lengthGetter)
        {
            _parserFactory = () => new HeadLengthParser(head, data =>
            {
                try
                {
                    var len = lengthGetter(data);
                    return Task.FromResult(new GetDataLengthRsp { Length = len, StateCode = Parser.StateCode.Success });
                }
                catch
                {
                    return Task.FromResult(new GetDataLengthRsp { Length = 0, StateCode = Parser.StateCode.LengthNotEnough });
                }
            });
            return this;
        }

        /// <inheritdoc/>
        public ITopM2MConfigStep WithHeadFootParser(byte[] head, byte[] foot)
        {
            _parserFactory = () => new HeadFootParser(head, foot);
            return this;
        }

        /// <inheritdoc/>
        public ITopM2MConfigStep WithTimeParser(int intervalMs = 50)
        {
            _parserFactory = () => new TimeParser(intervalMs);
            return this;
        }

        /// <inheritdoc/>
        public ITopM2MConfigStep WithFootParser(byte[] foot)
        {
            _parserFactory = () => new FootParser(foot);
            return this;
        }

        /// <inheritdoc/>
        public ITopM2MConfigStep WithParser(IParser parser)
        {
            _sharedParser = parser;
            return this;
        }

        /// <inheritdoc/>
        public ITopM2MConfigStep WithNoParser()
        {
            _parserFactory = () => new NoParser();
            return this;
        }

        #endregion

        #region ITopM2MConfigStep

        /// <inheritdoc/>
        public ITopM2MConfigStep SendInterval(int ms) { _sendInterval = ms; return this; }

        /// <inheritdoc/>
        public ITopM2MConfigStep OnReceived(Action<Guid, byte[]> handler) { _onReceived = handler; return this; }

        /// <inheritdoc/>
        public ITopM2MConfigStep OnReceived(Func<Guid, byte[], Task> handler) { _onReceivedAsync = handler; return this; }

        /// <inheritdoc/>
        public ITopM2MConfigStep OnClientConnected(Action<Guid> handler) { _onClientConnected = handler; return this; }

        /// <inheritdoc/>
        public ITopM2MConfigStep OnClientConnected(Func<Guid, Task> handler) { _onClientConnectedAsync = handler; return this; }

        /// <inheritdoc/>
        public ITopM2MConfigStep OnSent(Action<byte[], Guid> handler) { _onSent = handler; return this; }

        /// <inheritdoc/>
        public ITopPort_M2M Build()
        {
            if (_physicalPortM2M == null)
                throw new InvalidOperationException("必须选择多对多物理口类型，请调用 UseUdp");

            if (_parserFactory == null && _sharedParser == null)
                throw new InvalidOperationException("必须选择分包器，请调用 WithHeadLengthParser/WithHeadFootParser/WithTimeParser/WithFootParser/WithParser/WithNoParser 或 WithParserFactory");

            IParser CreateParser() => _parserFactory != null ? _parserFactory() : _sharedParser!;

            var topPort = new TopPort_M2M(_physicalPortM2M, () => Task.FromResult(CreateParser()));

            if (_onReceived != null)
                topPort.OnReceiveParsedData += (clientId, data) => { _onReceived(clientId, data); return Task.CompletedTask; };

            if (_onReceivedAsync != null)
                topPort.OnReceiveParsedData += async (clientId, data) => await _onReceivedAsync(clientId, data);

            if (_onClientConnected != null)
                topPort.OnClientConnect += clientId => { _onClientConnected(clientId); return Task.CompletedTask; };

            if (_onClientConnectedAsync != null)
                topPort.OnClientConnect += async clientId => await _onClientConnectedAsync(clientId);

            if (_onSent != null)
                topPort.OnSentData += (data, clientId) => { _onSent(data, clientId); return Task.CompletedTask; };

            return topPort;
        }

        #endregion
    }

    #endregion
}
