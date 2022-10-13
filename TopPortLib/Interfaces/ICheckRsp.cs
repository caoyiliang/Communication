namespace TopPortLib.Interfaces
{
    /// <summary>
    /// 检测命令是否是该返回
    /// </summary>
    public interface ICheckRsp
    {
        /// <summary>
        /// 检测命令是否是该返回
        /// </summary>
        /// <param name="bytes">板子主动上传命令</param>
        /// <returns>是否是改返回</returns>
        bool Check(byte[] bytes);
    }
}
