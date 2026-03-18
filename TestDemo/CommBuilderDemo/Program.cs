namespace CommBuilderDemo;

/// <summary>
/// CommBuilder 使用示例
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== CommBuilder 使用示例 ===\n");

        // 示例 1: 乌鸦场景 - RS485 主从
        Console.WriteLine("1. 乌鸦场景 - RS485 主从通讯");
        var crow = CommBuilder.CommBuilder.Crow()
            .UseSerial("COM3", 9600)
            .WithHeadFootParser([0xAA], [0x55])
            .Timeout(5000)
            .SendInterval(20)
            .Build();

        Console.WriteLine("   创建成功: CrowPort");
        Console.WriteLine("   用法: await crow.OpenAsync();");
        Console.WriteLine("        var response = await crow.RequestAsync<ReadCmd, ReadRsp>(cmd);\n");

        // 示例 2: 鸽子场景 - TCP 全双工
        Console.WriteLine("2. 鸽子场景 - TCP 全双工通讯");
        var pigeon = CommBuilder.CommBuilder.Pigeon(new MyHandler())
            .UseTcp("192.168.1.100", 9000)
            .WithHeadLengthParser([0xAA], data => data[2])
            .Timeout(3000)
            .Build();

        Console.WriteLine("   创建成功: PigeonPort");
        Console.WriteLine("   用法: await pigeon.StartAsync();");
        Console.WriteLine("        var response = await pigeon.RequestAsync<QueryCmd, QueryRsp>(cmd);\n");

        // 示例 3: 老鹰场景 - TCP Server
        Console.WriteLine("3. 老鹰场景 - TCP Server");
        var eagle = CommBuilder.CommBuilder.Eagle(new MyHandler())
            .UseTcpServer("0.0.0.0", 9000)
            .WithHeadFootParser([0xAA], [0x55])
            .Timeout(5000)
            .Build();

        Console.WriteLine("   创建成功: CondorPort");
        Console.WriteLine("   用法: await eagle.StartAsync();");
        Console.WriteLine("        var response = await eagle.RequestAsync<ClientCmd, ClientRsp>(clientId, cmd);\n");

        // 示例 4: 麻雀场景 - UDP 多对多
        Console.WriteLine("4. 麻雀场景 - UDP 多对多");
        var sparrow = CommBuilder.CommBuilder.Sparrow(new MyHandler())
            .UseUdp("0.0.0.0", 9000)
            .WithTimeParser(50)
            .Timeout(3000)
            .Build();

        Console.WriteLine("   创建成功: SparrowPort");
        Console.WriteLine("   用法: await sparrow.StartAsync();");
        Console.WriteLine("        var clientId = await sparrow.AddClientAsync(\"192.168.1.101\", 9001);\n");

        // 示例 5: 使用连接字符串
        Console.WriteLine("5. 使用连接字符串");
        var crow2 = CommBuilder.CommBuilder.Crow()
            .FromConnectionString("serial://COM3:9600:N:8:1")
            .WithHeadLengthParser([0xAA, 0x55], data => data[2] + 4)
            .Timeout(5000)
            .Build();

        Console.WriteLine("   创建成功: 使用 serial://COM3:9600:N:8:1\n");

        // 示例 6: 自定义分包器
        Console.WriteLine("6. 使用自定义分包器");
        var customParser = new Parser.Parsers.HeadFootParser([0x02], [0x03]);
        var pigeon2 = CommBuilder.CommBuilder.Pigeon(new MyHandler())
            .UseTcp("192.168.1.100", 9000)
            .WithParser(customParser)
            .Timeout(3000)
            .Build();

        Console.WriteLine("   创建成功: 使用自定义分包器\n");

        Console.WriteLine("=== 对比：改前 vs 改后 ===");
        Console.WriteLine("改前:");
        Console.WriteLine("  var crow = new CrowPort(new TopPort(new SerialPort(\"COM3\", 9600), new HeadFootParser([0xAA], [0x55])), 5000, 20);");
        Console.WriteLine();
        Console.WriteLine("改后:");
        Console.WriteLine("  var crow = CommBuilder.Crow()");
        Console.WriteLine("      .UseSerial(\"COM3\", 9600)");
        Console.WriteLine("      .WithHeadFootParser([0xAA], [0x55])");
        Console.WriteLine("      .Timeout(5000)");
        Console.WriteLine("      .SendInterval(20)");
        Console.WriteLine("      .Build();");
    }
}

/// <summary>
/// 主动推送事件处理器示例
/// </summary>
public class MyHandler
{
    // 这里可以定义主动推送的事件处理方法
}
