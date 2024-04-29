namespace TopPortLib.Interfaces
{
    /// <summary>
    /// 异步请求接口
    /// </summary>
    public interface IAsyncRequest
    {
        /// <summary>
        /// 转成字节数组
        /// </summary>
        /// <returns>字节数组</returns>
        byte[] ToBytes();

        /// <summary>
        /// 取请求包中的某些字节和返回包中的某些字节进行比对，从而确定返回是否是对应包
        /// </summary>
        /// <returns>请求包中的待校字节</returns>
        byte[]? Check();
    }
}
