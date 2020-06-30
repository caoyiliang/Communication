using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.Exceptions
{
    public class NotConnectedException : Exception
    {
        public NotConnectedException() : base() { }

        public NotConnectedException(string message) : base(message) { }

        public NotConnectedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
