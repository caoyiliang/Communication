using System.Runtime.Serialization;

namespace TopPortLib.Exceptions
{
    /// <summary>
    /// 请求转字节数组失败
    /// </summary>
    [Serializable]
    public class RequestParameterToBytesFailedException : Exception
    {
        /// <summary>请求转字节数组失败</summary>
        public RequestParameterToBytesFailedException() : base() { }
        /// <summary>请求转字节数组失败</summary>
        public RequestParameterToBytesFailedException(string message) : base(message) { }
        /// <summary>请求转字节数组失败</summary>
        public RequestParameterToBytesFailedException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>请求转字节数组失败</summary>
        protected RequestParameterToBytesFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
