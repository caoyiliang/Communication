/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：GetReq.cs
********************************************************************/

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
