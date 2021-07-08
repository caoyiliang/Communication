/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：ConnectFailedException.cs
********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.Exceptions
{
    public class ConnectFailedException : Exception
    {
        public ConnectFailedException() : base() { }

        public ConnectFailedException(string message) : base(message) { }

        public ConnectFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
