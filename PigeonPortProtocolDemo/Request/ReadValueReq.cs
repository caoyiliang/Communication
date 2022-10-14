using System.Text;
using TopPortLib.Interfaces;

namespace PigeonPortProtocolDemo.Request;

internal class ReadValueReq : IByteStream
{
    private readonly string _addr = "01";
    public ReadValueReq(string addr)
    {
        this._addr = addr;
    }

    public byte[] ToBytes()
    {
        return Encoding.ASCII.GetBytes(ToString());
    }

    public override string ToString()
    {
        return $"#{_addr}\r";
    }
}
