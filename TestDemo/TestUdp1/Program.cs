// See https://aka.ms/new-console-template for more information
using Communication.Bus;
using System.Text;
using TopPortLib;

Console.WriteLine("Hello, World!");
var topPort = new TopPort_M2M(new Udp("127.0.0.1", 2756), async () => await Task.FromResult(new Parser.Parsers.FootParser([0x0d, 0x0a])));
topPort.OnReceiveParsedData += TopPort_OnReceiveParsedData;

async Task TopPort_OnReceiveParsedData(Guid clientId, byte[] data)
{
    Console.WriteLine($"{clientId}:{Encoding.ASCII.GetString(data)}");
    await Task.CompletedTask;
}

await topPort.OpenAsync();
Console.ReadLine();