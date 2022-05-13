namespace Communication.Exceptions
{
    /// <summary>
    /// 连接失败
    /// </summary>
    public class ConnectFailedException : Exception
    {
        /// <summary>连接失败</summary>
        public ConnectFailedException() : base() { }
        /// <summary>连接失败</summary>
        public ConnectFailedException(string message) : base(message) { }
        /// <summary>连接失败</summary>
        public ConnectFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
