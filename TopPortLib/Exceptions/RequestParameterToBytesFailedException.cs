/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：RequestParameterToBytesFailedException.cs
********************************************************************/

using System.Runtime.Serialization;

namespace TopPortLib.Exceptions
{
    [Serializable]
    public class RequestParameterToBytesFailedException : Exception
    {
        public RequestParameterToBytesFailedException() : base() { }
        public RequestParameterToBytesFailedException(string message) : base(message) { }
        public RequestParameterToBytesFailedException(string message, Exception innerException) : base(message, innerException) { }
        protected RequestParameterToBytesFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
