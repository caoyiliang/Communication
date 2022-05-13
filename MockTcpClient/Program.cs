using Communication.Bus.PhysicalPort;
using Communication.Interfaces;
using System.Text;

var cts = new CancellationTokenSource();
IPhysicalPort port = new TcpClient("127.0.0.1", 7779);
await port.OpenAsync();
_ = Task.Run(async () =>
{
    while (!cts.IsCancellationRequested)
    {
        var task = Task.Run(async () =>
        {
            await port.SendDataAsync(Encoding.ASCII.GetBytes("Hello"), cts.Token);
        });
        await Task.Delay(100);
    }
});
Console.ReadKey();