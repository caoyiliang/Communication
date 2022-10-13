using System.Text;
using TopPortLib.Interfaces;

namespace PigeonPortProtocolDemo.Request;

internal class ReadSignalValueReq : IByteStream
{
    private readonly string _addr = "01";
    public ReadSignalValueReq(string addr)
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
