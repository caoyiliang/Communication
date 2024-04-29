using System.Text;
using TopPortLib.Interfaces;

namespace PigeonPortProtocolDemo.Request;

internal class ReadValueReq(string addr) : IAsyncRequest
{
    public byte[]? Check()
    {
        return null;
    }

    public byte[] ToBytes()
    {
        return Encoding.ASCII.GetBytes(ToString());
    }

    public override string ToString()
    {
        return $"#{addr}\r";
    }
}
