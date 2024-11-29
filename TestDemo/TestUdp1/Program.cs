// See https://aka.ms/new-console-template for more information
using Communication.Bus.PhysicalPort;
using System.Net;
using System.Text;
using TopPortLib;

Console.WriteLine("Hello, World!");
var topPort = new TopPort(new UdpClient("127.0.0.1", 8087, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080)), new Parser.Parsers.FootParser([0x0d, 0x0a]));
topPort.OnReceiveParsedData += TopPort_OnReceiveParsedData;
await topPort.OpenAsync();
Console.ReadLine();


async Task TopPort_OnReceiveParsedData(byte[] data)
{
    Console.WriteLine(Encoding.ASCII.GetString(data));
    await Task.CompletedTask;
}