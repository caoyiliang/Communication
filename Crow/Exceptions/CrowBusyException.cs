using System;
using System.Collections.Generic;
using System.Text;

namespace Crow.Exceptions
{
    public class CrowBusyException : Exception
    {
        public CrowBusyException() : base() { }

        public CrowBusyException(string message) : base(message) { }

        public CrowBusyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
