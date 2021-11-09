/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：SendException.cs
********************************************************************/

namespace Communication.Exceptions
{
    public class SendException : Exception
    {
        public SendException() : base() { }

        public SendException(string message) : base(message) { }

        public SendException(string message, Exception innerException) : base(message, innerException) { }
    }
}
