namespace Crow.Exceptions
{
    /// <summary>
    /// 队列连接层发送异常
    /// </summary>
    public class TilesSendException : Exception
    {
        /// <summary>队列连接层发送异常</summary>
        public TilesSendException() : base() { }
        /// <summary>队列连接层发送异常</summary>
        public TilesSendException(string message) : base(message) { }
        /// <summary>队列连接层发送异常</summary>
        public TilesSendException(string message, Exception innerException) : base(message, innerException) { }
    }
}
