using System.Text;
using TopPortLib.Interfaces;

namespace TopPortLibIntTest.Request
{
    class PlayerReq : IByteStream
    {
        private readonly int _id;
        private readonly string _name;

        public PlayerReq(int id, string name)
        {
            _id = id;
            _name = name;
        }

        public byte[] ToBytes()
        {
            var id = BitConverter.GetBytes(_id);
            var name = Encoding.UTF8.GetBytes(_name);
            var bytes = new byte[id.Length + name.Length];
            Array.Copy(id, 0, bytes, 0, id.Length);
            Array.Copy(name, 0, bytes, id.Length, name.Length);
            return bytes;
        }
    }
}
