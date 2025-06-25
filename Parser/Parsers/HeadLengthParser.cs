using LogInterface;

namespace Parser.Parsers
{
    /// <summary>
    /// 以特定字节数组为数据包头，数特定长度分包
    /// </summary>
    public class HeadLengthParser : BaseParser
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<HeadLengthParser>();
        private int _startIndex = -1;
        private int _length = -1;
        
        private byte[] _head;
        /// <summary>
        /// 帧头
        /// </summary>
        public byte[] Head
        {
            get => _head;
            set
            {
                if (value == null || value.Length == 0)
                    throw new Exception("帧头不能为空或空数组");
                _head = value;
            }
        }
        private readonly GetDataLengthEventHandler OnGetDataLength;

        /// <summary>
        /// 长度为除了帧头之外的所有数据的长度
        /// </summary>
        /// <param name="head">帧头</param>
        /// <param name="getDataLength">获取数据包长度</param>
        /// <param name="useChannel">是否启用内置处理队列</param>
        /// <exception cref="Exception"></exception>
        public HeadLengthParser(byte[] head, GetDataLengthEventHandler getDataLength, bool useChannel = true) : base(useChannel)
        {
            if (head == null || head.Length == 0) throw new Exception("必须传入帧头");
            _head = head;
            OnGetDataLength = getDataLength ?? throw new Exception("必须要getDataLength");
        }

        /// <summary>
        /// 以特定字节数组为数据包头，数特定长度分包
        /// </summary>
        /// <param name="getDataLength">获取数据包长度</param>
        /// <param name="useChannel">是否启用内置处理队列</param>
        /// <exception cref="Exception"></exception>
        public HeadLengthParser(GetDataLengthEventHandler getDataLength, bool useChannel = true) : base(useChannel)
        {
            _head = [];
            OnGetDataLength = getDataLength ?? throw new Exception("必须要getDataLength");
        }

        /// <inheritdoc/>
        protected override async Task<bool> CanFindEndIndexAsync()
        {
            if (_bytes.Count - (_startIndex - _bytes.StartIndex) < _bytes.GetCurrentMessageLength()) return false;
            if (_length == -1)
            {
                byte[] temp = new byte[_bytes.Count - (_startIndex - _bytes.StartIndex)];
                Array.Copy(_bytes.Bytes, _startIndex, temp, 0, temp.Length);
                try
                {
                    var rsp = await OnGetDataLength(temp);
                    if (rsp.StateCode != Parser.StateCode.Success)
                    {
                        return false;
                    }
                    _length = rsp.Length;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Get data length error");
                    return false;
                }
                if (_length <= 0)
                    throw new Exception("数据长度必须大于0");
            }
            if (_head.Length + _length > _bytes.Count - (_startIndex - _bytes.StartIndex))
            {
                _bytes.SetCurrentMessageLength(_head.Length + _length);
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        protected override int FindStartIndex()
        {
            if (_startIndex == -1)
            {
                var rsp = FindIndex(_bytes.StartIndex, _head);
                //为了避免存储太多的垃圾数据，要清除垃圾
                if (rsp.Code == StateCode.NotFound)
                {
                    _bytes.RemoveHeader(_bytes.Count - _head.Length);
                }
                else if (rsp.Code == StateCode.Success)
                {
                    _startIndex = rsp.Index;
                }
            }
            return _startIndex;
        }

        /// <inheritdoc/>
        protected override int FindEndIndex()
        {
            return _startIndex + _head.Length + _length;
        }

        /// <inheritdoc/>
        protected override void ResetStartIndex(int bytesOldIndex)
        {
            if (_startIndex != -1)
                _startIndex -= bytesOldIndex;
        }

        /// <inheritdoc/>
        protected override async Task<bool> ReceiveOneFrameAsync()
        {
            if (await base.ReceiveOneFrameAsync())
            {
                _startIndex = -1;
                _length = -1;
                _bytes.SetCurrentMessageLength(-1);
                return true;
            }
            return false;
        }
    }
}
