using TopPortLib.Interfaces;

namespace CondorPortProtocolDemo.Response;

internal class ReadValueRsp : IAsyncResponse<(List<decimal> recData, int result)>
{
    public List<decimal> RecData { get; set; } = new();
    public int Result { get; set; }

    public async Task AnalyticalData(byte[] bytes)
    {
        RecData = new List<decimal> { 1, 2, 3 };
        Result = 1;
        await Task.CompletedTask;
    }

    public bool Check(byte[] bytes)
    {
        return bytes[0] == 0x01;
    }

    public (List<decimal> recData, int result) GetResult()
    {
        return (RecData, Result);
    }
}
