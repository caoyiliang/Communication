using CommBuilder.Interfaces;
using Communication.Interfaces;
using Parser;
using Parser.Interfaces;
using Parser.Parsers;
using System.IO.Ports;
using TopPortLib;
using TopPortLib.Interfaces;

namespace CommBuilder.Builders
{
    /// <inheritdoc/>
    public class CrowBuilder : ICrowPhysicalPortStep, ICrowParserStep, ICrowConfigureStep
    {
        private IPhysicalPort? _physicalPort;
        private IParser? _parser;
        private int _timeout = 5000;
        private int _sendInterval = 20;
        private Action<byte[]>? _onReceived;
        private Func<byte[], Task>? _onReceivedAsync;
        private Func<Task>? _onConnectedAsync;
        private Func<Task>? _onDisconnectedAsync;

        /// <inheritdoc/>
        public ICrowParserStep UseSerial(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _physicalPort = new Communication.Bus.PhysicalPort.SerialPort(portName, baudRate, parity, dataBits, stopBits);
            return this;
        }

        /// <inheritdoc/>
        public ICrowParserStep UseTcp(string host, int port)
        {
            _physicalPort = new Communication.Bus.PhysicalPort.TcpClient(host, port);
            return this;
        }

        /// <inheritdoc/>
        public ICrowParserStep UseNamedPipe(string pipeName)
        {
            _physicalPort = new Communication.Bus.PhysicalPort.NamedPipeClient(pipeName);
            return this;
        }

        /// <inheritdoc/>
        public ICrowParserStep FromConnectionString(string connectionString)
        {
            _physicalPort = ConnectionStringParser.Parse(connectionString);
            return this;
        }

        /// <inheritdoc/>
        public ICrowConfigureStep WithHeadLengthParser(byte[] head, Func<byte[], int> lengthGetter)
        {
            _parser = new HeadLengthParser(head, data =>
            {
                var len = lengthGetter(data);
                return Task.FromResult(new GetDataLengthRsp { Length = len, StateCode = Parser.StateCode.Success });
            });
            return this;
        }

        /// <inheritdoc/>
        public ICrowConfigureStep WithHeadFootParser(byte[] head, byte[] foot)
        {
            _parser = new HeadFootParser(head, foot);
            return this;
        }

        /// <inheritdoc/>
        public ICrowConfigureStep WithTimeParser(int intervalMs = 50)
        {
            _parser = new TimeParser(intervalMs);
            return this;
        }

        /// <inheritdoc/>
        public ICrowConfigureStep WithParser(IParser parser)
        {
            _parser = parser;
            return this;
        }

        /// <inheritdoc/>
        public ICrowConfigureStep WithNoParser()
        {
            _parser = new NoParser();
            return this;
        }

        /// <inheritdoc/>
        public ICrowConfigureStep Timeout(int ms)
        {
            _timeout = ms;
            return this;
        }

        /// <inheritdoc/>
        public ICrowConfigureStep SendInterval(int ms)
        {
            _sendInterval = ms;
            return this;
        }

        /// <inheritdoc/>
        public ICrowConfigureStep OnReceived(Func<byte[], Task> handler)
        {
            _onReceivedAsync = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public ICrowConfigureStep OnReceived(Action<byte[]> handler)
        {
            _onReceived = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public ICrowConfigureStep OnConnected(Func<Task> handler)
        {
            _onConnectedAsync = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public ICrowConfigureStep OnDisconnected(Func<Task> handler)
        {
            _onDisconnectedAsync = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public ICrowPort Build()
        {
            if (_physicalPort == null)
                throw new InvalidOperationException("必须选择物理口，请调用 UseSerial/UseTcp/UseNamedPipe/FromConnectionString");

            if (_parser == null)
                throw new InvalidOperationException("必须选择分包器，请调用 WithHeadLengthParser/WithHeadFootParser/WithTimeParser/WithParser/WithNoParser");

            var crowPort = new CrowPort(_physicalPort, _parser, _timeout, _sendInterval);

            if (_onReceived != null)
                crowPort.OnReceivedData += data =>
                {
                    _onReceived(data);
                    return Task.CompletedTask;
                };

            if (_onReceivedAsync != null)
                crowPort.OnReceivedData += async data => await _onReceivedAsync(data);

            if (_onConnectedAsync != null)
                crowPort.OnConnect += async () => await _onConnectedAsync();

            if (_onDisconnectedAsync != null)
                crowPort.OnDisconnect += async () => await _onDisconnectedAsync();

            return crowPort;
        }
    }
}
