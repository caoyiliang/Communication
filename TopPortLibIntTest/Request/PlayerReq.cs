using System;
using System.Collections.Generic;
using System.Text;
using TopPortLib.Interfaces;

namespace TopPortLibIntTest.Request
{
    class PlayerReq : IByteStream
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public byte[] ToBytes()
        {
            var id = BitConverter.GetBytes(ID);
            var name= Encoding.UTF8.GetBytes(Name);
            var bytes = new byte[id.Length + name.Length];
            Array.Copy(id,0,bytes,0,id.Length);
            Array.Copy(name,0,bytes,id.Length,name.Length);
            return bytes;
        }
    }
}
