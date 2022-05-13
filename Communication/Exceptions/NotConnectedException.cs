namespace Communication.Exceptions
{
    /// <summary>
    /// 未连接
    /// </summary>
    public class NotConnectedException : Exception
    {
        /// <summary>未连接</summary>
        public NotConnectedException() : base() { }
        /// <summary>未连接</summary>
        public NotConnectedException(string message) : base(message) { }
        /// <summary>未连接</summary>
        public NotConnectedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
