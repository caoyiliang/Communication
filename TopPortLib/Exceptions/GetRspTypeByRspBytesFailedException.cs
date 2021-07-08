/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：GetRspTypeByRspBytesFailedException.cs
********************************************************************/

using System;
using System.Runtime.Serialization;

namespace TopPortLib.Exceptions
{
    [Serializable]
    internal class GetRspTypeByRspBytesFailedException : Exception
    {
        public GetRspTypeByRspBytesFailedException()
        {
        }

        public GetRspTypeByRspBytesFailedException(string message) : base(message)
        {
        }

        public GetRspTypeByRspBytesFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GetRspTypeByRspBytesFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}