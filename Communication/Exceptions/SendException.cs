/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：SendException.cs
********************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Communication.Exceptions
{
    public class SendException : Exception
    {
        public SendException() : base() { }

        public SendException(string message) : base(message) { }

        public SendException(string message, Exception innerException) : base(message, innerException) { }
    }
}
