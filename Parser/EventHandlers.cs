namespace Parser
{
    /// <summary>
    /// 推送解析结果
    /// </summary>
    /// <param name="data">数据包</param>
    public delegate Task ReceiveParsedDataEventHandler(byte[] data);

    /// <summary>
    /// 获取命令长度
    /// </summary>
    /// <param name="data">当前收到的字节</param>
    /// <returns cref="GetDataLengthRsp">数据长度解析回复</returns>
    public delegate Task<GetDataLengthRsp> GetDataLengthEventHandler(byte[] data);

    /// <summary>
    /// 数据长度解析回复
    /// </summary>
    public class GetDataLengthRsp
    {
        /// <summary>
        /// 除了帧头之外的字节数
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public StateCode StateCode { get; set; }
    }

    /// <summary>
    /// 状态码
    /// </summary>
    public enum StateCode
    {
        /// <summary>
        /// 成功解析长度
        /// </summary>
        Success,
        /// <summary>
        /// 长度不够
        /// </summary>
        LengthNotEnough
    }
}
