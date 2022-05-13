namespace Communication.Interfaces
{
    /// <summary>
    /// 物理口接口
    /// </summary>
    public interface IPhysicalPort : IDisposable
    {
        /// <summary>
        /// 是否开启
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 打开
        /// </summary>
        Task OpenAsync();

        /// <summary>
        /// 关闭
        /// </summary>
        Task CloseAsync();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">要发送的字节数组</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task SendDataAsync(byte[] data, CancellationToken cancellationToken);

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="count">收到字节个数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns cref="ReadDataResult">接收到的数据</returns>
        Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken);
    }

    /// <summary>
    /// 接收到的数据
    /// </summary>
    public class ReadDataResult
    {
        /// <summary>
        /// 收到字节数组
        /// </summary>
        public byte[] Data { get; set; } = null!;
        /// <summary>
        /// 收到字节个数
        /// </summary>
        public int Length { get; set; }
    }
}
