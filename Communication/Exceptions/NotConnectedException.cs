/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：NotConnectedException.cs
********************************************************************/

namespace Communication.Exceptions
{
    public class NotConnectedException : Exception
    {
        public NotConnectedException() : base() { }

        public NotConnectedException(string message) : base(message) { }

        public NotConnectedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
