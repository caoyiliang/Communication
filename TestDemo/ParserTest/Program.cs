// See https://aka.ms/new-console-template for more information
using Parser;
using Parser.Interfaces;
using Parser.Parsers;

Console.WriteLine("Hello, World!");

//IParser parser = new TimeParser();
//parser.OnReceiveParsedData += Parser_OnReceiveParsedData;

//async Task Parser_OnReceiveParsedData(byte[] data)
//{
//    await Task.Delay(5000);
//};

//_ = Task.Run(async () =>
//{
//    while (true)
//    {
//        await parser.ReceiveOriginalDataAsync([0x11, 0xff, 0xfe], 3);
//        await Console.Out.WriteLineAsync($"{DateTime.Now} 11111");
//        await Task.Delay(1000);
//    }
//});

//var parser = new HeadLengthParser([0xD4, 0xF3, 0xCC, 0xEC], async data =>
//{
//    if (data.Length < 2) return new GetDataLengthRsp() { StateCode = Parser.StateCode.LengthNotEnough };
//    return await Task.FromResult(new GetDataLengthRsp() { Length = Utils.StringByteUtils.ToInt16(data, 4, true), StateCode = Parser.StateCode.Success });
//}, haveHeadOfData: true);
//parser.OnReceiveParsedData += async data =>
//{
//    await Console.Out.WriteLineAsync($"{DateTime.Now} {Utils.StringByteUtils.BytesToString(data)}");
//};
//await parser.ReceiveOriginalDataAsync([0xD4, 0xF3, 0xCC, 0xEC], 4);
//byte[] data = [0xD4, 0xF3, 0xCC, 0xEC, 0x00, 0x11, 0x00, 0x03, 0x03, 0x01, 0xD0, 0x00, 0x01];
//await parser.ReceiveOriginalDataAsync(data, data.Length);
//byte[] data1 = [0xD4, 0xF3, 0xCC, 0xEC, 0x00, 0x11, 0x00, 0x03, 0x03, 0x01, 0xD0, 0x00, 0x01, 0x00, 0x02, 0x01, 0x00, 0x6F, 0x8A, 0xBB, 0xAA];
//await parser.ReceiveOriginalDataAsync(data1, data1.Length);
//await parser.ReceiveOriginalDataAsync(data1, data1.Length);
//await parser.ReceiveOriginalDataAsync([0xD4, 0xF3, 0xCC, 0xEC], 4);
//await parser.ReceiveOriginalDataAsync(data1, data1.Length);

var parser = new HeadLengthParser([0xD4, 0xF3, 0xCC, 0xEC], async data =>
{
    if (data.Length < 2) return new GetDataLengthRsp() { StateCode = Parser.StateCode.LengthNotEnough };
    return await Task.FromResult(new GetDataLengthRsp() { Length = Utils.StringByteUtils.ToInt16(data, 4, true) - 4, StateCode = Parser.StateCode.Success });
}, haveHeadOfData: false);
parser.OnReceiveParsedData += async data =>
{
    await Console.Out.WriteLineAsync($"{DateTime.Now} {Utils.StringByteUtils.BytesToString(data)}");
};
await parser.ReceiveOriginalDataAsync([0xD4, 0xF3, 0xCC, 0xEC], 4);
byte[] data = [0xD4, 0xF3, 0xCC, 0xEC, 0x00, 0x25, 0x67, 0x54, 0x47, 0x95];
await parser.ReceiveOriginalDataAsync(data, data.Length);
byte[] data1 = [0xD4, 0xF3, 0xCC, 0xEC, 0x00, 0x15, 0x00, 0x03, 0x03, 0x01, 0xD0, 0x00, 0x01, 0x00, 0x02, 0x01, 0x00, 0x6F, 0x8A, 0xBB, 0xAA];
await parser.ReceiveOriginalDataAsync(data1, data1.Length);
Console.ReadKey();