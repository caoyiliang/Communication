// See https://aka.ms/new-console-template for more information
using Communication.Bus.PhysicalPort;
using PigeonPortProtocolDemo;

Console.WriteLine("Hello, World!");
IPigeonPortProtocol pigeonPortProtocolDemo = new PigeonPortProtocol(new TcpClient("127.0.0.1", 2756));
pigeonPortProtocolDemo.OnReadValue += PigeonPortProtocolDemo_OnReadValue;

static async Task PigeonPortProtocolDemo_OnReadValue((List<decimal> recData, int result) objects)
{
    await Task.CompletedTask;
}

await pigeonPortProtocolDemo.OpenAsync();

Console.ReadKey();
