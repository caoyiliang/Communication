namespace Communication.Exceptions
{
    /// <summary>
    /// 发送失败
    /// </summary>
    public class SendException : Exception
    {
        /// <summary>发送失败</summary>
        public SendException() : base() { }
        /// <summary>发送失败</summary>
        public SendException(string message) : base(message) { }
        /// <summary>发送失败</summary>
        public SendException(string message, Exception innerException) : base(message, innerException) { }
    }
}
