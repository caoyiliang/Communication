using System.Runtime.Serialization;

namespace TopPortLib.Exceptions
{
    [Serializable]
    public class ResponseParameterCreateFailedException : Exception
    {
        public ResponseParameterCreateFailedException() : base() { }
        public ResponseParameterCreateFailedException(string message) : base(message) { }
        public ResponseParameterCreateFailedException(string message, Exception innerException) : base(message, innerException) { }
        protected ResponseParameterCreateFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
