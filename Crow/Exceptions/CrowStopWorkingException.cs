namespace Crow.Exceptions
{
    /// <summary>
    /// 乌鸦停止工作异常
    /// </summary>
    public class CrowStopWorkingException : Exception
    {
        public CrowStopWorkingException() : base() { }

        public CrowStopWorkingException(string message) : base(message) { }

        public CrowStopWorkingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
