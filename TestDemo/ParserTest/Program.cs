// See https://aka.ms/new-console-template for more information
using Parser.Interfaces;
using Parser.Parsers;

Console.WriteLine("Hello, World!");

IParser parser = new TimeParser();
parser.OnReceiveParsedData += Parser_OnReceiveParsedData;

async Task Parser_OnReceiveParsedData(byte[] data)
{
    await Task.Delay(5000);
};

_ = Task.Run(async () =>
{
    while (true)
    {
        await parser.ReceiveOriginalDataAsync([0x11, 0xff, 0xfe], 3);
        await Console.Out.WriteLineAsync($"{DateTime.Now} 11111");
        await Task.Delay(1000);
    }
});

Console.ReadKey();