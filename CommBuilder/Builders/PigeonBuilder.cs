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
    public class PigeonBuilder : IPigeonPhysicalPortStep, IPigeonParserStep, IPigeonConfigureStep
    {
        private readonly object _instance;
        private IPhysicalPort? _physicalPort;
        private IParser? _parser;
        private int _timeout = 5000;
        private int _sendInterval = 20;
        private CheckEventHandler? _checkEvent;

        /// <inheritdoc/>
        public PigeonBuilder(object instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        /// <inheritdoc/>
        public IPigeonParserStep UseSerial(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _physicalPort = new Communication.Bus.PhysicalPort.SerialPort(portName, baudRate, parity, dataBits, stopBits);
            return this;
        }

        /// <inheritdoc/>
        public IPigeonParserStep UseTcp(string host, int port)
        {
            _physicalPort = new Communication.Bus.PhysicalPort.TcpClient(host, port);
            return this;
        }

        /// <inheritdoc/>
        public IPigeonParserStep UseNamedPipe(string pipeName)
        {
            _physicalPort = new Communication.Bus.PhysicalPort.NamedPipeClient(pipeName);
            return this;
        }

        /// <inheritdoc/>
        public IPigeonParserStep FromConnectionString(string connectionString)
        {
            _physicalPort = ConnectionStringParser.Parse(connectionString);
            return this;
        }

        /// <inheritdoc/>
        public IPigeonConfigureStep WithHeadLengthParser(byte[] head, Func<byte[], int> lengthGetter)
        {
            _parser = new HeadLengthParser(head, data =>
            {
                var len = lengthGetter(data);
                return Task.FromResult(new GetDataLengthRsp { Length = len, StateCode = Parser.StateCode.Success });
            });
            return this;
        }

        /// <inheritdoc/>
        public IPigeonConfigureStep WithHeadFootParser(byte[] head, byte[] foot)
        {
            _parser = new HeadFootParser(head, foot);
            return this;
        }

        /// <inheritdoc/>
        public IPigeonConfigureStep WithTimeParser(int intervalMs = 50)
        {
            _parser = new TimeParser(intervalMs);
            return this;
        }

        /// <inheritdoc/>
        public IPigeonConfigureStep WithParser(IParser parser)
        {
            _parser = parser;
            return this;
        }

        /// <inheritdoc/>
        public IPigeonConfigureStep WithNoParser()
        {
            _parser = new NoParser();
            return this;
        }

        /// <inheritdoc/>
        public IPigeonConfigureStep Timeout(int ms)
        {
            _timeout = ms;
            return this;
        }

        /// <inheritdoc/>
        public IPigeonConfigureStep SendInterval(int ms)
        {
            _sendInterval = ms;
            return this;
        }

        /// <inheritdoc/>
        public IPigeonConfigureStep WithCheck(Func<byte[], Task<bool>> checkFunc)
        {
            if (checkFunc == null) throw new ArgumentNullException(nameof(checkFunc));
            _checkEvent = data => checkFunc(data);
            return this;
        }

        /// <inheritdoc/>
        public IPigeonConfigureStep WithCheck(Func<byte[], bool> checkFunc)
        {
            if (checkFunc == null) throw new ArgumentNullException(nameof(checkFunc));
            _checkEvent = data => Task.FromResult(checkFunc(data));
            return this;
        }

        /// <inheritdoc/>
        public IPigeonPort Build()
        {
            if (_physicalPort == null)
                throw new InvalidOperationException("必须选择物理口，请调用 UseSerial/UseTcp/UseNamedPipe/FromConnectionString");

            if (_parser == null)
                throw new InvalidOperationException("必须选择分包器，请调用 WithHeadLengthParser/WithHeadFootParser/WithTimeParser/WithParser/WithNoParser");

            var topPort = new TopPort(_physicalPort, _parser);
            var pigeonPort = new PigeonPort(_instance, topPort, _timeout, _sendInterval)
            {
                CheckEvent = _checkEvent
            };

            return pigeonPort;
        }
    }
}
