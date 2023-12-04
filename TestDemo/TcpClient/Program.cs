// See https://aka.ms/new-console-template for more information
using Communication.Bus.PhysicalPort;
using Parser.Parsers;
using TopPortLib;
using TopPortLib.Interfaces;

Console.WriteLine("Hello, World!");

ITopPort topPort = new TopPort(new TcpClient("192.168.6.140", 1111), new TimeParser());
await topPort.OpenAsync();

//收到数据
topPort.OnReceiveParsedData += async (data) => { await Task.CompletedTask; };

//连接服务端
topPort.OnConnect += async () =>
{
    Console.WriteLine($"连接服务端成功..............");
    await Task.CompletedTask;
};

//断开连接
topPort.OnDisconnect += async () =>
{
    Console.WriteLine("断开连接..............");
    await Task.CompletedTask;
};

Console.ReadLine();
