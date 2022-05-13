namespace Parser.Interfaces
{
    /// <summary>
    /// 解析器
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// 推送解析结果
        /// </summary>
        event ReceiveParsedDataEventHandler OnReceiveParsedData;

        /// <summary>
        /// 将数据放入解析器
        /// </summary>
        /// <param name="data">待解析数据</param>
        /// <param name="size">待解析数据有效长度</param>
        Task ReceiveOriginalDataAsync(byte[] data, int size);
    }
}
