namespace TopPortLib.Interfaces
{
    /// <summary>
    /// 返回队列
    /// </summary>
    public interface IRspEnumerable
    {
        /// <summary>
        /// 是否是队列最后一个回复
        /// </summary>
        /// <returns>是则表示该返回类型队列结束</returns>
        Task<bool> IsFinish();

        /// <summary>
        /// 是否和请求校验
        /// </summary>
        /// <returns>是否和请求校验</returns>
        bool NeedCheck();
    }
}
