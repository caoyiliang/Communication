using TopPortLib.Interfaces;

namespace PigeonPortProtocolDemo.Response;

internal class ReadValueRsp : IAsyncResponse<(List<decimal> recData, int result)>
{
    public List<decimal> RecData { get; set; } = [];
    public int Result { get; set; }

    public async Task AnalyticalData(byte[] bytes)
    {
        RecData = [1, 2, 3];
        Result = 1;
        await Task.CompletedTask;
    }

    public (bool Type, byte[]? CheckBytes) Check(byte[] bytes)
    {
        return (bytes[0] == 0x01, null);
    }

    public (List<decimal> recData, int result) GetResult()
    {
        return (RecData, Result);
    }
}
