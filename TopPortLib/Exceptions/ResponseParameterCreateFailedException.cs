using System.Runtime.Serialization;

namespace TopPortLib.Exceptions
{
    /// <summary>
    /// 接收处理创建失败
    /// </summary>
    [Serializable]
    public class ResponseParameterCreateFailedException : Exception
    {
        /// <summary>接收处理创建失败</summary>
        public ResponseParameterCreateFailedException() : base() { }
        /// <summary>接收处理创建失败</summary>
        public ResponseParameterCreateFailedException(string message) : base(message) { }
        /// <summary>接收处理创建失败</summary>
        public ResponseParameterCreateFailedException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>接收处理创建失败</summary>
        protected ResponseParameterCreateFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
