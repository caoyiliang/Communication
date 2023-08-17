using System.Text;
using TopPortLib.Interfaces;

namespace CondorPortProtocolDemo.Request;

internal class ReadValueReq : IByteStream
{
    public ReadValueReq()
    {
    }

    public byte[] ToBytes()
    {
        return Encoding.ASCII.GetBytes(ToString());
    }

    public override string ToString()
    {
        return $"#01\r";
    }
}
