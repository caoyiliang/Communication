/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：CrowBusyException.cs
********************************************************************/

namespace Crow.Exceptions
{
    public class CrowBusyException : Exception
    {
        public CrowBusyException() : base() { }

        public CrowBusyException(string message) : base(message) { }

        public CrowBusyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
