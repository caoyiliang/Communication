using TopPortLib.Interfaces;

namespace CommBuilderDemo;

/// <summary>
/// CommBuilder 使用示例
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== CommBuilder 使用示例 ===\n");

        // ==================== 顶层通讯口（无队列） ====================

        // 示例 1: 顶层点对点 - 串口
        Console.WriteLine("【顶层】1. 串口通讯（无队列）");
        var port = CommBuilder.CommBuilder.Top()
            .UseSerial("COM3", 9600)
            .WithHeadFootParser([0xAA], [0x55])
            .OnReceived(data => Console.WriteLine($"收到: {BitConverter.ToString(data)}"))
            .OnConnected(() => Console.WriteLine("已连接"))
            .OnDisconnected(() => Console.WriteLine("已断开"))
            .Build();
        Console.WriteLine("   创建成功: ITopPort");
        Console.WriteLine("   用法: await port.OpenAsync(); await port.SendAsync(data);\n");

        // 示例 2: 顶层点对点 - TCP
        Console.WriteLine("【顶层】2. TCP 客户端通讯（无队列）");
        var tcp = CommBuilder.CommBuilder.Top()
            .UseTcp("192.168.1.100", 9000)
            .WithHeadLengthParser([0xAA], data => data[2])
            .AutoReconnect()
            .OnReceived(data => Handle(data))
            .Build();
        Console.WriteLine("   创建成功: ITopPort");
        Console.WriteLine("   用法: await tcp.OpenAsync(); await tcp.SendAsync(data);\n");

        // 示例 3: 顶层服务端 - TCP Server
        Console.WriteLine("【顶层】3. TCP Server 通讯（无队列）");
        ITopPort_Server? server = null;
        server = CommBuilder.CommBuilder.TopServer()
            .UseTcpServer("0.0.0.0", 9000)
            .WithHeadFootParser([0xAA], [0x55])
            .OnClientConnected(clientId => Console.WriteLine($"客户端连接: {clientId}"))
            .OnClientDisconnected(clientId => Console.WriteLine($"客户端断开: {clientId}"))
            .OnReceived(async (clientId, data) =>
            {
                Console.WriteLine($"收到来自 {clientId}: {BitConverter.ToString(data)}");
                // 处理数据后回复
                if (server != null)
                    await server.SendAsync(clientId, [0x01, 0x02]);
            })
            .Build() as ITopPort_Server;
        Console.WriteLine("   创建成功: ITopPort_Server");
        Console.WriteLine("   用法: await server.OpenAsync(); await server.SendAsync(clientId, data);\n");

        // 示例 4: 顶层服务端 - UDP 多对多
        Console.WriteLine("【顶层】4. UDP 多对多通讯（无队列）");
        var udp = CommBuilder.CommBuilder.TopServer()
            .UseUdp("0.0.0.0", 9000)
            .WithTimeParser(50)
            .OnReceived((clientId, data) => Handle(data))
            .Build();
        Console.WriteLine("   创建成功: ITopPort_M2M");
        Console.WriteLine("   用法: await udp.OpenAsync(); var id = await udp.AddClientAsync(host, port); await udp.SendAsync(id, data);\n");

        // ==================== 队列版本 ====================

        // 示例 5: 乌鸦场景 - RS485 主从
        Console.WriteLine("【队列】5. 乌鸦场景 - RS485 主从通讯");
        var crow = CommBuilder.CommBuilder.Crow()
            .UseSerial("COM3", 9600)
            .WithHeadFootParser([0xAA], [0x55])
            .Timeout(5000)
            .SendInterval(20)
            .Build();
        Console.WriteLine("   创建成功: ICrowPort");
        Console.WriteLine("   用法: await crow.OpenAsync(); var response = await crow.RequestAsync<ReadCmd, ReadRsp>(cmd);\n");

        // 示例 6: 鸽子场景 - TCP 全双工
        Console.WriteLine("【队列】6. 鸽子场景 - TCP 全双工通讯");
        var pigeon = CommBuilder.CommBuilder.Pigeon(new MyHandler())
            .UseTcp("192.168.1.100", 9000)
            .WithHeadLengthParser([0xAA], data => data[2])
            .Timeout(3000)
            .Build();
        Console.WriteLine("   创建成功: IPigeonPort");
        Console.WriteLine("   用法: await pigeon.StartAsync(); var response = await pigeon.RequestAsync<QueryCmd, QueryRsp>(cmd);\n");

        // 示例 7: 老鹰场景 - TCP Server
        Console.WriteLine("【队列】7. 老鹰场景 - TCP Server");
        var eagle = CommBuilder.CommBuilder.Eagle(new MyHandler())
            .UseTcpServer("0.0.0.0", 9000)
            .WithHeadFootParser([0xAA], [0x55])
            .Timeout(5000)
            .Build();
        Console.WriteLine("   创建成功: ICondorPort");
        Console.WriteLine("   用法: await eagle.StartAsync(); var response = await eagle.RequestAsync<ClientCmd, ClientRsp>(clientId, cmd);\n");

        // 示例 8: 麻雀场景 - UDP 多对多
        Console.WriteLine("【队列】8. 麻雀场景 - UDP 多对多");
        var sparrow = CommBuilder.CommBuilder.Sparrow(new MyHandler())
            .UseUdp("0.0.0.0", 9000)
            .WithTimeParser(50)
            .Timeout(3000)
            .Build();
        Console.WriteLine("   创建成功: ISparrowPort");
        Console.WriteLine("   用法: await sparrow.StartAsync(); var id = await sparrow.AddClientAsync(host, port); var rsp = await sparrow.RequestAsync<UdpCmd, UdpRsp>(id, cmd);\n");

        // ==================== 其他示例 ====================

        // 示例 9: 使用连接字符串
        Console.WriteLine("9. 使用连接字符串");
        var crow2 = CommBuilder.CommBuilder.Crow()
            .FromConnectionString("serial://COM3:9600:N:8:1")
            .WithHeadLengthParser([0xAA, 0x55], data => data[2] + 4)
            .Timeout(5000)
            .Build();
        Console.WriteLine("   创建成功: 使用 serial://COM3:9600:N:8:1\n");

        // 示例 10: 自定义分包器
        Console.WriteLine("10. 使用自定义分包器");
        var customParser = new Parser.Parsers.HeadFootParser([0x02], [0x03]);
        var pigeon2 = CommBuilder.CommBuilder.Pigeon(new MyHandler())
            .UseTcp("192.168.1.100", 9000)
            .WithParser(customParser)
            .Timeout(3000)
            .Build();
        Console.WriteLine("   创建成功: 使用自定义分包器\n");

        // 对比展示
        Console.WriteLine("=== 对比：改前 vs 改后 ===");
        Console.WriteLine("改前（繁琐）:");
        Console.WriteLine("  var port = new TopPort(new SerialPort(\"COM3\", 9600), new HeadFootParser([0xAA], [0x55]));");
        Console.WriteLine();
        Console.WriteLine("改后（简洁）:");
        Console.WriteLine("  var port = CommBuilder.Top()");
        Console.WriteLine("      .UseSerial(\"COM3\", 9600)");
        Console.WriteLine("      .WithHeadFootParser([0xAA], [0x55])");
        Console.WriteLine("      .Build();");
    }

    private static void Handle(byte[] data)
    {
        // 处理接收到的数据
        Console.WriteLine($"处理数据: {BitConverter.ToString(data)}");
    }
}

/// <summary>
/// 主动推送事件处理器示例（鸽子/老鹰/麻雀需要）
/// </summary>
public class MyHandler
{
    // 鸽子、老鹰、麻雀场景需要传入一个实例来处理主动推送事件
    // 这里可以定义 OnPushData 等事件处理方法
}
