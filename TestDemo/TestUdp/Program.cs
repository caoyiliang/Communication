// See https://aka.ms/new-console-template for more information
using Communication.Bus;
using System.Text;
using TopPortLib;

Console.WriteLine("Hello, World!");
var topPort = new TopPort_M2M(new Udp("127.0.0.1", 7778), async () => await Task.FromResult(new Parser.Parsers.FootParser([0x0d, 0x0a])));
await topPort.OpenAsync();

while (true)
{
    await topPort.SendAsync("127.0.0.1", 2756, [.. Encoding.ASCII.GetBytes("Hello,I'm Client"), 0x0d, 0x0a]);
}