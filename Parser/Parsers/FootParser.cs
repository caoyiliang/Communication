namespace Parser.Parsers
{
    /// <summary>
    /// 帧尾以特定字节数组结尾
    /// </summary>
    public class FootParser : BaseParser
    {
        /// <summary>
        /// 帧尾
        /// </summary>
        private readonly byte[] _foot;

        /// <summary>
        /// 帧尾以特定字节数组结尾
        /// </summary>
        /// <param name="foot">帧尾</param>
        /// <exception cref="ArgumentException"></exception>
        public FootParser(byte[] foot)
        {
            if (foot == null || foot.Length == 0) throw new ArgumentException("必须传入帧尾");
            this._foot = foot;
        }

        /// <inheritdoc/>
        protected override int FindStartIndex()
        {
            return _bytes.StartIndex;
        }

        /// <inheritdoc/>
        protected override int FindEndIndex()
        {
            var rsp = FindIndex(_bytes.StartIndex, _foot);
            if (rsp.Code != StateCode.Success) return -1;
            return rsp.Index + _foot.Length;
        }
    }
}
