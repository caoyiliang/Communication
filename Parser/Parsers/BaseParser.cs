using LogInterface;
using Parser.Interfaces;
using System.Threading.Channels;
using Utils;

namespace Parser.Parsers
{
    /// <summary>
    /// 解析器基类
    /// </summary>
    public abstract class BaseParser : IParser
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger("BaseParser");
        private readonly bool _useChannel = true;
        private readonly Channel<byte[]> _channel = Channel.CreateUnbounded<byte[]>();

        /// <summary>
        /// 解析器中的数据
        /// </summary>
        protected RemainBytes _bytes = new();

        /// <inheritdoc/>
        public event ReceiveParsedDataEventHandler? OnReceiveParsedData;

        /// <summary>
        /// 解析器基类
        /// </summary>
        /// <param name="useChannel">是否启用内置处理队列</param>
        protected BaseParser(bool useChannel = true)
        {
            _useChannel = useChannel;
            if (_useChannel) _ = Task.Run(ParseAndProcessDataAsync);
        }

        private async Task ParseAndProcessDataAsync()
        {
            await foreach (var data in _channel.Reader.ReadAllAsync())
            {
                if (OnReceiveParsedData is not null)
                {
                    await OnReceiveParsedData.Invoke(data);
                }
            }
        }

        /// <inheritdoc/>
        public async Task ReceiveOriginalDataAsync(byte[] data, int size)
        {
            var bytesOldIndex = _bytes.StartIndex;
            _bytes.Append(data, 0, size);
            if (bytesOldIndex != 0 && _bytes.StartIndex == 0)
            {
                ResetStartIndex(bytesOldIndex);
            }
            while (await ReceiveOneFrameAsync()) ;
        }

        /// <summary>
        /// 设置新的起始位置
        /// </summary>
        /// <param name="bytesOldIndex">旧位置</param>
        protected virtual void ResetStartIndex(int bytesOldIndex) { }

        /// <summary>
        /// 能否找新的结束位置
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<FrameEndStatusCode> CanFindEndIndexAsync() => await Task.FromResult(FrameEndStatusCode.Success);

        /// <summary>
        /// 返回解析完成的包
        /// </summary>
        /// <returns>是否有解析成功的包</returns>
        protected async virtual Task<bool> ReceiveOneFrameAsync()
        {
            int startIndex = FindStartIndex();
            if (startIndex < 0)
            {
                return false;
            }
            FrameEndStatusCode endStatusCode = await CanFindEndIndexAsync();
            if (endStatusCode == FrameEndStatusCode.Fail)
            {
                return false;
            }
            if (endStatusCode == FrameEndStatusCode.ResearchHead)
            {
                return await ReceiveOneFrameAsync();
            }
            startIndex = FindStartIndex();
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
                if (_useChannel)
                {
                    await _channel.Writer.WriteAsync(data);
                }
                else
                {
                    if (OnReceiveParsedData is not null)
                    {
                        await OnReceiveParsedData.Invoke(data);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Handle Parsed data error");
            }
            return true;
        }

        /// <summary>
        /// 找起始位置
        /// </summary>
        /// <returns>起始位置</returns>
        protected abstract int FindStartIndex();

        /// <summary>
        /// 找结束位置
        /// </summary>
        /// <returns>结束位置</returns>
        protected abstract int FindEndIndex();

        /// <summary>
        /// 找指定数组所在位置
        /// </summary>
        /// <param name="startIndex">查找起点</param>
        /// <param name="block">指定数组</param>
        /// <returns>指定数组位置</returns>
        protected FindIndexRsp FindIndex(int startIndex, byte[] block)
        {
            if (block.Length == 0) return new FindIndexRsp { Code = StateCode.Success, Index = startIndex };
            if (_bytes.Count - (startIndex - _bytes.StartIndex) < block.Length) return new FindIndexRsp() { Code = StateCode.LengthNotEnough, Index = -1 };
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
                        return new FindIndexRsp() { Index = i, Code = StateCode.Success };
                    }
                }
            }
            return new FindIndexRsp() { Code = StateCode.NotFound, Index = -1 };
        }
    }

    /// <summary>
    /// 查找位置结果
    /// </summary>
    public class FindIndexRsp
    {
        /// <summary>
        /// 找到的位置信息
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 查找结果
        /// </summary>
        public StateCode Code { get; set; }
    }

    /// <summary>
    /// 查找结果
    /// </summary>
    public enum StateCode
    {
        /// <summary>
        /// 找到位置
        /// </summary>
        Success,
        /// <summary>
        /// 长度不足
        /// </summary>
        LengthNotEnough,
        /// <summary>
        /// 没找到
        /// </summary>
        NotFound
    }

    /// <summary>
    /// 数据帧搜寻结束状态码
    /// </summary>
    public enum FrameEndStatusCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success,
        /// <summary>
        /// 重新搜寻数据帧头
        /// </summary>
        ResearchHead,
        /// <summary>
        /// 失败
        /// </summary>
        Fail
    }

}
