using TopPortLib.Interfaces;

namespace PigeonPortProtocolDemo.Response;

internal class ReadSignalValueRsp : ICheckRsp
{
    public List<decimal> RecData { get; set; } = new();

    public ReadSignalValueRsp(byte[] rspBytes)
    {
        //string str = Encoding.ASCII.GetString(rspBytes);
        //var result = str.GetAllNum();
        //if (result.Count != 8)
        //{
        //    throw new Exception($"数据长度为{result.Count} {str}");
        //}
        RecData = new List<decimal> { 1, 2, 3 };
    }

    public bool Check(byte[] bytes)
    {
        return bytes[2] == 0x03;
    }
}
