using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Parsers
{
    public class FootParser : BaseParser
    {
        /// <summary>
        /// 帧尾巴
        /// </summary>
        private byte[] _foot;

        public FootParser(byte[] foot)
        {
            if (foot == null || foot.Length == 0) throw new Exception("必须传入帧尾");
            this._foot = foot;
        }

        protected override int FindStartIndex()
        {
            return _bytes.StartIndex;
        }

        protected override int FindEndIndex()
        {
            var rsp = FindIndex(_bytes.StartIndex, _foot);
            if (rsp.Code != ErrorCode.Success) return -1;
            return rsp.Index + _foot.Length;
        }
    }
}
