// See https://aka.ms/new-console-template for more information
using Communication.Bus.PhysicalPort;
using Parser.Parsers;
using TopPortLib.Interfaces;

Console.WriteLine("Hello, World!");
ITopPort topPort = new TopPortLib.TopPort(new SerialPort(), new HeadFootParser([0x23], [0x0d]));
topPort.OnReceiveParsedData += TopPort_OnReceiveParsedData;

async Task TopPort_OnReceiveParsedData(byte[] data)
{
    
}

await topPort.OpenAsync();

var a = Console.ReadLine();

topPort.Parser = new FootParser([0x0d]);


Console.ReadLine();