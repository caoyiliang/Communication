using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopPortLib.Interfaces;

namespace PigeonPortDemo
{
    class GetReq : IByteStream
    {
        public byte[] ToBytes()
        {
            return new byte[] { 1 };
        }
    }
}
