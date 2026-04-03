using CommBuilder.Interfaces;
using Parser;
using Parser.Interfaces;
using Parser.Parsers;
using TopPortLib;
using TopPortLib.Interfaces;

namespace CommBuilder.Builders
{
    /// <inheritdoc/>
    public class EagleBuilder : IEaglePhysicalPortStep, IEagleParserStep, IEagleConfigureStep
    {
        private readonly object _instance;
        private string? _host;
        private int _port;
        private Func<IParser>? _parserFactory;
        private int _timeout = 5000;
        private Func<Guid, Task>? _onClientConnectedAsync;
        private Func<Guid, Task>? _onClientDisconnectedAsync;

        /// <inheritdoc/>
        public EagleBuilder(object instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        /// <inheritdoc/>
        public IEagleParserStep UseTcpServer(string host, int port)
        {
            _host = host;
            _port = port;
            return this;
        }

        /// <inheritdoc/>
        public IEagleConfigureStep WithParserFactory(Func<IParser> parserFactory)
        {
            _parserFactory = parserFactory;
            return this;
        }

        /// <inheritdoc/>
        public IEagleConfigureStep WithHeadLengthParser(byte[] head, Func<byte[], int> lengthGetter)
        {
            _parserFactory = () => new HeadLengthParser(head, data =>
            {
                var len = lengthGetter(data);
                return Task.FromResult(new GetDataLengthRsp { Length = len, StateCode = Parser.StateCode.Success });
            });
            return this;
        }

        /// <inheritdoc/>
        public IEagleConfigureStep WithHeadFootParser(byte[] head, byte[] foot)
        {
            _parserFactory = () => new HeadFootParser(head, foot);
            return this;
        }

        /// <inheritdoc/>
        public IEagleConfigureStep WithTimeParser(int intervalMs = 50)
        {
            _parserFactory = () => new TimeParser(intervalMs);
            return this;
        }

        /// <inheritdoc/>
        public IEagleConfigureStep Timeout(int ms)
        {
            _timeout = ms;
            return this;
        }

        /// <inheritdoc/>
        public IEagleConfigureStep OnClientConnected(Func<Guid, Task> handler)
        {
            _onClientConnectedAsync = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IEagleConfigureStep OnClientDisconnected(Func<Guid, Task> handler)
        {
            _onClientDisconnectedAsync = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public ICondorPort Build()
        {
            if (string.IsNullOrEmpty(_host))
                throw new InvalidOperationException("必须选择监听地址，请调用 UseTcpServer");

            if (_parserFactory == null)
                throw new InvalidOperationException("必须设置分包器工厂，请调用 WithParserFactory/WithHeadLengthParser/WithHeadFootParser/WithTimeParser");

            var tcpServer = new Communication.Bus.TcpServer(_host!, _port);
            var topPortServer = new TopPort_Server(tcpServer, () => Task.FromResult(_parserFactory()));
            var condorPort = new CondorPort(_instance, topPortServer, _timeout);

            if (_onClientConnectedAsync != null)
                condorPort.OnClientConnect += async clientId => await _onClientConnectedAsync(clientId);

            if (_onClientDisconnectedAsync != null)
                condorPort.OnClientDisconnect += async clientId => await _onClientDisconnectedAsync(clientId);

            return condorPort;
        }
    }
}
