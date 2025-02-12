using Communication.Bus;
using Parser.Parsers;
using System.Diagnostics;
using TopPortLib;

long totalBytesReceived = 0;

Console.WriteLine("Hello, World!");
var bytes = (byte[])Array.CreateInstance(typeof(byte), 1009);
TopPort_Server server = new(new TcpServer("0.0.0.0", 7778), async () => await Task.FromResult(new FootParser([0x0d])));
//server.OnClientConnect += Server_OnClientConnect;
server.OnReceiveParsedData += Server_OnReceiveParsedData;

async Task Server_OnReceiveParsedData(Guid clientId, byte[] data)
{
    totalBytesReceived += data.Length;
    await Task.CompletedTask;
};

async Task Server_OnClientConnect(Guid clientId)
{
    _ = Task.Run(async () =>
    {
        while (true)
            await server.SendAsync(clientId, [0x11, 0x22, 0x33, .. bytes, 0x0d]);
    });
    await Task.CompletedTask;
}

await server.OpenAsync();

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

Console.ReadLine();