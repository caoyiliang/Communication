using LogInterface;
using Parser.Interfaces;
using Utils;

namespace Parser.Parsers
{
    public abstract class BaseParser : IParser
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger("BaseParser");
        protected RemainBytes _bytes = new RemainBytes();

        public event ReceiveParsedDataEventHandler OnReceiveParsedData;

        public async Task ReceiveOriginalDataAsync(byte[] data, int size)
        {
            var bytesOldIndex = _bytes.StartIndex;
            _bytes.Append(data, 0, size);
            if (bytesOldIndex != 0 && _bytes.StartIndex == 0)
            {
                ResetSatrtIndex(bytesOldIndex);
            }
            while (await ReceiveOneFrameAsync()) ;
        }
        protected virtual void ResetSatrtIndex(int bytesOldIndex) { }

        protected virtual async Task<bool> CanFindEndIndexAsync() => await Task.FromResult(true);

        protected async virtual Task<bool> ReceiveOneFrameAsync()
        {
            int startIndex = FindStartIndex();
            if (startIndex < 0)
            {
                return false;
            }
            if (!await CanFindEndIndexAsync()) return false;
            int endIndex = FindEndIndex();
            if (endIndex < 0)
            {
                return false;
            }
            byte[] data = new byte[endIndex - startIndex];
            Array.Copy(_bytes.Bytes, startIndex, data, 0, data.Length);
            _bytes.RemoveHeader(endIndex - _bytes.StartIndex);
            try
            {
                await this.OnReceiveParsedData?.Invoke(data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Handle Parsed data error");
            }
            return true;
        }

        protected abstract int FindStartIndex();

        protected abstract int FindEndIndex();

        protected FindIndexRsp FindIndex(int startIndex, byte[] block)
        {
            if (block.Length == 0) return new FindIndexRsp { Code = ErrorCode.Success, Index = startIndex };
            if (_bytes.Count - (startIndex - _bytes.StartIndex) < block.Length) return new FindIndexRsp() { Code = ErrorCode.LengthNotEnough, Index = -1 };
            for (int i = startIndex; i < _bytes.StartIndex + _bytes.Count; i++)
            {
                if (_bytes.Bytes[i] == block[0])
                {
                    bool find = true;
                    for (int j = 1; j < block.Length; j++)
                    {
                        if (i + j < _bytes.StartIndex + _bytes.Count)
                        {
                            if (_bytes.Bytes[i + j] != block[j]) { find = false; break; }
                        }
                        else
                        {
                            find = false; break;
                        }
                    }
                    if (find)
                    {
                        return new FindIndexRsp() { Index = i, Code = ErrorCode.Success };
                    }
                }
            }
            return new FindIndexRsp() { Code = ErrorCode.NotFound, Index = -1 };
        }
    }

    public class FindIndexRsp
    {
        public int Index { get; set; }

        public ErrorCode Code { get; set; }
    }

    public enum ErrorCode
    {
        Success,
        LengthNotEnough,
        NotFound
    }
}
