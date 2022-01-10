using LogInterface;

namespace Parser.Parsers
{
    public class HeadLengthParser : BaseParser
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<HeadLengthParser>();
        private int _startIndex = -1;
        private int _length = -1;
        /// <summary>
        /// 帧头
        /// </summary>
        private byte[] _head;
        private GetDataLengthEventHandler OnGetDataLength;
        /// <summary>
        /// 长度为除了帧头之外的所有数据的长度
        /// </summary>
        /// <param name="head"></param>
        /// <param name="getDataLength"></param>
        public HeadLengthParser(byte[] head, GetDataLengthEventHandler getDataLength)
        {
            if (head == null || head.Length == 0) throw new Exception("必须传入帧头");
            this._head = head;
            this.OnGetDataLength = getDataLength ?? throw new Exception("必须要getDataLength");
        }

        /// <summary>
        ///  长度为包括表示长度的字节在内的所有数据的长度
        /// </summary>
        /// <param name="getDataLength"></param>
        public HeadLengthParser(GetDataLengthEventHandler getDataLength)
        {
            this._head = new byte[0];
            this.OnGetDataLength = getDataLength ?? throw new Exception("必须要getDataLength");
        }

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
                    if (rsp.ErrorCode != Parser.ErrorCode.Success)
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

        protected override int FindStartIndex()
        {
            if (_startIndex == -1)
            {
                var rsp = FindIndex(_bytes.StartIndex, _head);
                //为了避免存储太多的垃圾数据，要清除垃圾
                if (rsp.Code == ErrorCode.NotFound)
                {
                    _bytes.RemoveHeader(_bytes.Count - _head.Length);
                }
                else if (rsp.Code == ErrorCode.Success)
                {
                    _startIndex = rsp.Index;
                }
            }
            return _startIndex;
        }

        protected override int FindEndIndex()
        {
            return _startIndex + _head.Length + _length;
        }

        protected override void ResetSatrtIndex(int bytesOldIndex)
        {
            if (_startIndex != -1)
                _startIndex -= bytesOldIndex;
        }

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
