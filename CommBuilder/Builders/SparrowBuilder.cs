using CommBuilder.Interfaces;
using Parser;
using Parser.Interfaces;
using Parser.Parsers;
using TopPortLib;
using TopPortLib.Interfaces;

namespace CommBuilder.Builders
{
    /// <summary>
    /// 麻雀场景 Builder - UDP 多对多通讯
    /// </summary>
    public class SparrowBuilder : ISparrowPhysicalPortStep, ISparrowParserStep, ISparrowConfigureStep
    {
        private readonly object _instance;
        private string? _host;
        private int _port;
        private Func<IParser>? _parserFactory;
        private int _timeout = 5000;

        /// <summary>
        /// 创建麻雀 Builder
        /// </summary>
        /// <param name="instance">主动推送事件所在实例（通常传 this）</param>
        public SparrowBuilder(object instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        #region ISparrowPhysicalPortStep

        /// <inheritdoc/>
        public ISparrowParserStep UseUdp(string host, int port)
        {
            _host = host;
            _port = port;
            return this;
        }

        #endregion

        #region ISparrowParserStep

        /// <inheritdoc/>
        public ISparrowConfigureStep WithParserFactory(Func<IParser> parserFactory)
        {
            _parserFactory = parserFactory;
            return this;
        }

        /// <inheritdoc/>
        public ISparrowConfigureStep WithHeadLengthParser(byte[] head, Func<byte[], int> lengthGetter)
        {
            _parserFactory = () => new HeadLengthParser(head, data =>
            {
                var len = lengthGetter(data);
                return Task.FromResult(new GetDataLengthRsp { Length = len, StateCode = Parser.StateCode.Success });
            });
            return this;
        }

        /// <inheritdoc/>
        public ISparrowConfigureStep WithHeadFootParser(byte[] head, byte[] foot)
        {
            _parserFactory = () => new HeadFootParser(head, foot);
            return this;
        }

        /// <inheritdoc/>
        public ISparrowConfigureStep WithTimeParser(int intervalMs = 50)
        {
            _parserFactory = () => new TimeParser(intervalMs);
            return this;
        }

        #endregion

        #region ISparrowConfigureStep

        /// <inheritdoc/>
        public ISparrowConfigureStep Timeout(int ms)
        {
            _timeout = ms;
            return this;
        }

        /// <inheritdoc/>
        public ISparrowPort Build()
        {
            if (string.IsNullOrEmpty(_host))
                throw new InvalidOperationException("必须选择监听地址，请调用 UseUdp");

            if (_parserFactory == null)
                throw new InvalidOperationException("必须设置分包器工厂，请调用 WithParserFactory/WithHeadLengthParser/WithHeadFootParser/WithTimeParser");

            var udp = new Communication.Bus.Udp(_host!, _port);
            var topPortM2M = new TopPort_M2M(udp, () => Task.FromResult(_parserFactory()));
            return new SparrowPort(_instance, topPortM2M, _timeout);
        }

        #endregion
    }
}
