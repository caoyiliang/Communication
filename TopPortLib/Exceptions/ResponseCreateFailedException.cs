using System.Runtime.Serialization;

namespace TopPortLib.Exceptions
{
    /// <summary>
    /// 接收处理创建失败
    /// </summary>
    [Serializable]
    public class ResponseCreateFailedException : Exception
    {
        /// <summary>接收处理创建失败</summary>
        public ResponseCreateFailedException() : base() { }
        /// <summary>接收处理创建失败</summary>
        public ResponseCreateFailedException(string message) : base(message) { }
        /// <summary>接收处理创建失败</summary>
        public ResponseCreateFailedException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>接收处理创建失败</summary>
        protected ResponseCreateFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
