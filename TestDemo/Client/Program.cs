// See https://aka.ms/new-console-template for more information
using Communication.Bus.PhysicalPort;
using Parser.Parsers;
using System.Diagnostics;
//using System.Net.Sockets;
using TopPortLib;

Console.WriteLine("Hello, World!");
long totalBytesReceived = 0;
var isConnected = false;

var _tcpClient = new TopPort(new TcpClient("192.168.18.200", 7778), new NoParser());
_tcpClient.OnConnect += () =>
{
    isConnected = true;
    return Task.CompletedTask;
};
//_tcpClient.OnReceiveParsedData += (data) =>
//{
//    totalBytesReceived += data.Length;
//    return Task.CompletedTask;
//};

try
{
    await _tcpClient.OpenAsync(true);
}
catch
{

}

while(!isConnected)
{
    await Task.Delay(1000);
}

//var _tcpClient = new TcpClient("192.168.18.200", 7778);
//await _tcpClient.OpenAsync();
//_ = Task.Run(async () =>
//{
//    while (true)
//    {
//        byte[] data = new byte[8192];
//        int i = await _tcpClient.GetStream().ReadAsync(data, 0, data.Length);
//    }
//});

Stopwatch stopwatch = Stopwatch.StartNew();
System.Timers.Timer timer = new(1000); // 每秒触发一次
timer.Elapsed += (sender, e) =>
{
    double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
    if (elapsedSeconds > 0)
    {
        double speed = totalBytesReceived / (1024.0 * 1024.0); // 每秒接收到的兆字节数
        Console.WriteLine($"实时网速: {speed:F2} MB/s");
    }
    // 重置计数器和时间
    totalBytesReceived = 0;
    stopwatch.Restart();
};
timer.Start();

var bytes = (byte[])Array.CreateInstance(typeof(byte), 1009);
byte[] scmd = [0x33, 0x44, 0x55, .. bytes, 0x0d];
_ = Task.Run(async () =>
{
    while (true)
    {
        //await _tcpClient.GetStream().WriteAsync(scmd, 0, scmd.Length);
        await _tcpClient.SendAsync(scmd);
    }
});

Console.ReadLine();