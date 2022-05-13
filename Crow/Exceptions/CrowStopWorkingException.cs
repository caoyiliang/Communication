namespace Crow.Exceptions
{
    /// <summary>
    /// 队列停止工作异常
    /// </summary>
    public class CrowStopWorkingException : Exception
    {
        /// <summary>队列停止工作异常</summary>
        public CrowStopWorkingException() : base() { }
        /// <summary>队列停止工作异常</summary>
        public CrowStopWorkingException(string message) : base(message) { }
        /// <summary>队列停止工作异常</summary>
        public CrowStopWorkingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
