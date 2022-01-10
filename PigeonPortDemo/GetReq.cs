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
