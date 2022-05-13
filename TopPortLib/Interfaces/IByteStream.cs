namespace TopPortLib.Interfaces
{
    /// <summary>
    /// 字节数组流
    /// </summary>
    public interface IByteStream
    {
        /// <summary>
        /// 转成字节数组
        /// </summary>
        /// <returns>字节数组</returns>
        byte[] ToBytes();
    }
}
