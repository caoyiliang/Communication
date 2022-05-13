namespace Crow.Exceptions
{
    /// <summary>
    /// 队列繁忙
    /// </summary>
    public class CrowBusyException : Exception
    {
        /// <summary>队列繁忙</summary>
        public CrowBusyException() : base() { }
        /// <summary>队列繁忙</summary>
        public CrowBusyException(string message) : base(message) { }
        /// <summary>队列繁忙</summary>
        public CrowBusyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
