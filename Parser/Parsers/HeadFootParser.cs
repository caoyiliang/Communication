using Parser.Interfaces;

namespace Parser.Parsers
{
    /// <summary>
    /// 以特定字节数组的开头和结尾来分数据包
    /// </summary>
    public class HeadFootParser : BaseParser, IParser
    {
        private int _startIndex = -1;
        /// <summary>
        /// 帧头
        /// </summary>
        private readonly byte[] _head;
        /// <summary>
        /// 帧尾
        /// </summary>
        private readonly byte[] _foot;
        /// <summary>
        /// 以特定字节数组的开头和结尾来分数据包
        /// </summary>
        /// <param name="head">帧头</param>
        /// <param name="foot">帧尾</param>
        /// <exception cref="ArgumentException"></exception>
        public HeadFootParser(byte[] head, byte[] foot)
        {
            if (head == null || head.Length == 0) throw new ArgumentException("必须传入帧头");
            if (foot == null || foot.Length == 0) throw new ArgumentException("必须传入帧尾");
            this._head = head;
            this._foot = foot;
        }

        /// <inheritdoc/>
        protected override async Task<bool> ReceiveOneFrameAsync()
        {
            if (await base.ReceiveOneFrameAsync())
            {
                _startIndex = -1;
                return true;
            }
            return false;
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
            var rsp = FindIndex(_startIndex + _head.Length, _foot);
            return rsp.Code == StateCode.Success ? rsp.Index + _foot.Length : -1;
        }

        /// <inheritdoc/>
        protected override void ResetSatrtIndex(int bytesOldIndex)
        {
            if (_startIndex != -1)
                _startIndex -= bytesOldIndex;
        }
    }
}
