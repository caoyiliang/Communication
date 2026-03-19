using CommBuilder;

namespace CommBuilderDemo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== CommBuilder Demo ===\n");

        // 示例 1: 顶层点对点 - 串口
        Console.WriteLine("1. Serial (Top)");
        var port = Comm.Top()
            .UseSerial("COM3", 9600)
            .WithHeadFootParser([0xAA], [0x55])
            .OnReceived(data => Console.WriteLine($"Received: {BitConverter.ToString(data)}"))
            .Build();
        Console.WriteLine("   OK: ITopPort\n");

        // 示例 2: 顶层点对点 - TCP
        Console.WriteLine("2. TCP Client (Top)");
        var tcp = Comm.Top()
            .UseTcp("192.168.1.100", 9000)
            .WithHeadLengthParser([0xAA], data => data[2])
            .AutoReconnect()
            .Build();
        Console.WriteLine("   OK: ITopPort\n");

        // 示例 3: 顶层服务端 - TCP Server
        Console.WriteLine("3. TCP Server (TopServer)");
        var server = Comm.TopServer()
            .UseTcpServer("0.0.0.0", 9000)
            .WithHeadFootParser([0xAA], [0x55])
            .Build();
        Console.WriteLine("   OK: ITopPort_Server\n");

        // 示例 4: 顶层服务端 - UDP
        Console.WriteLine("4. UDP (TopServer)");
        var udp = Comm.TopServer()
            .UseUdp("0.0.0.0", 9000)
            .WithTimeParser(50)
            .Build();
        Console.WriteLine("   OK: ITopPort_M2M\n");

        // 示例 5: 乌鸦 - RS485
        Console.WriteLine("5. Crow (RS485)");
        var crow = Comm.Crow()
            .UseSerial("COM3", 9600)
            .WithHeadFootParser([0xAA], [0x55])
            .Timeout(5000)
            .Build();
        Console.WriteLine("   OK: ICrowPort\n");

        // 示例 6: 鸽子 - TCP
        Console.WriteLine("6. Pigeon (TCP)");
        var pigeon = Comm.Pigeon(new MyHandler())
            .UseTcp("192.168.1.100", 9000)
            .WithHeadLengthParser([0xAA], data => data[2])
            .Timeout(3000)
            .Build();
        Console.WriteLine("   OK: IPigeonPort\n");

        // 示例 7: 老鹰 - TCP Server
        Console.WriteLine("7. Eagle (TCP Server)");
        var eagle = Comm.Eagle(new MyHandler())
            .UseTcpServer("0.0.0.0", 9000)
            .WithHeadFootParser([0xAA], [0x55])
            .Build();
        Console.WriteLine("   OK: ICondorPort\n");

        // 示例 8: 麻雀 - UDP
        Console.WriteLine("8. Sparrow (UDP)");
        var sparrow = Comm.Sparrow(new MyHandler())
            .UseUdp("0.0.0.0", 9000)
            .WithTimeParser(50)
            .Build();
        Console.WriteLine("   OK: ISparrowPort\n");

        // 对比展示
        Console.WriteLine("=== Before vs After ===");
        Console.WriteLine("Before: var port = new TopPort(new SerialPort(...), new HeadFootParser(...));");
        Console.WriteLine("After:  var port = Comm.Top().UseSerial(...).WithHeadFootParser(...).Build();");
    }
}

public class MyHandler
{
    // Pigeon/Eagle/Sparrow need an instance for push events
}
