using TopPortLib.Interfaces;

namespace CondorPortProtocolDemo.Response;

internal class ReadValueRsp : IAsyncResponse_Server<(List<decimal> recData, int result)>
{
    public List<decimal> RecData { get; set; } = [];
    public int Result { get; set; }

    public async Task AnalyticalData(string clientInfo, byte[] bytes)
    {
        RecData = [1, 2, 3];
        Result = 1;
        await Task.CompletedTask;
    }

    public (bool Type, byte[]? CheckBytes) Check(string clientInfo, byte[] bytes)
    {
        return (bytes[0] == 0x01, null);
    }

    public (List<decimal> recData, int result) GetResult()
    {
        return (RecData, Result);
    }
}
