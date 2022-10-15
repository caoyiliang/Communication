using TopPortLib.Interfaces;

namespace PigeonPortProtocolDemo.Response;

internal class ReadValueRsp : IPigeonResponse
{
    public List<decimal> RecData { get; set; } = new();

    public async Task AnalyticalData(byte[] bytes)
    {
        RecData = new List<decimal> { 1, 2, 3 };
        await Task.CompletedTask;
    }

    public bool Check(byte[] bytes)
    {
        return bytes[2] == 0x03;
    }
}
