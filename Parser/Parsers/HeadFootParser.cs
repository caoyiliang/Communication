/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：HeadFootParser.cs
********************************************************************/

using Parser.Interfaces;

namespace Parser.Parsers
{
    public class HeadFootParser : BaseParser, IParser
    {
        private int _startIndex = -1;
        /// <summary>
        /// 帧头
        /// </summary>
        private byte[] _head;
        /// <summary>
        /// 帧尾
        /// </summary>
        private byte[] _foot;

        public HeadFootParser(byte[] head, byte[] foot)
        {
            if (head == null || head.Length == 0) throw new Exception("必须传入帧头");
            if (foot == null || foot.Length == 0) throw new Exception("必须传入帧尾");
            this._head = head;
            this._foot = foot;
        }

        protected override async Task<bool> ReceiveOneFrameAsync()
        {
            if (await base.ReceiveOneFrameAsync())
            {
                _startIndex = -1;
                return true;
            }
            return false;
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
            var rsp = FindIndex(_startIndex + _head.Length, _foot);
            if (rsp.Code != ErrorCode.Success) return -1;
            return rsp.Index + _foot.Length;
        }

        protected override void ResetSatrtIndex(int bytesOldIndex)
        {
            if (_startIndex != -1)
                _startIndex -= bytesOldIndex;
        }
    }
}
