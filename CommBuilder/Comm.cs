using CommBuilder.Builders;
using CommBuilder.Interfaces;

namespace CommBuilder
{
    /// <summary>
    /// 通讯库 Fluent Builder 入口
    /// </summary>
    /// <remarks>
    /// <para>使用场景说明：</para>
    /// <list type="bullet">
    ///   <item>
    ///     <term>顶层 (Top)</term>
    ///     <description>最简单的通讯方式，无队列，直接收发数据，适合简单场景</description>
    ///   </item>
    ///   <item>
    ///     <term>顶层服务端 (TopServer)</term>
    ///     <description>TCP 服务端模式，管理多个客户端连接，无队列，直接收发</description>
    ///   </item>
    ///   <item>
    ///     <term>顶层多对多 (TopM2M)</term>
    ///     <description>UDP 多对多模式，无连接状态管理，无队列，直接收发</description>
    ///   </item>
    ///   <item>
    ///     <term>乌鸦 (Crow)</term>
    ///     <description>RS485 主从场景，请求-响应队列模式，适用于 Modbus RTU、串口设备通讯</description>
    ///   </item>
    ///   <item>
    ///     <term>鸽子 (Pigeon)</term>
    ///     <description>TCP 全双工场景，支持主动推送，适用于设备状态监控、实时通讯</description>
    ///   </item>
    ///   <item>
    ///     <term>老鹰 (Eagle)</term>
    ///     <description>TCP Server 版鸽子，服务端多客户端管理</description>
    ///   </item>
    ///   <item>
    ///     <term>麻雀 (Sparrow)</term>
    ///     <description>UDP 多对多场景，适用于广播通讯、设备发现</description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 顶层 - 最简单，无队列
    /// var port = Comm.Top()
    ///     .UseSerial("COM3", 9600)
    ///     .WithHeadFootParser([0xAA], [0x55])
    ///     .OnReceived(data => Console.WriteLine(BitConverter.ToString(data)))
    ///     .Build();
    /// await port.OpenAsync();
    /// await port.SendAsync([0x01, 0x02, 0x03]);
    /// 
    /// // 顶层服务端 - TCP Server，无队列
    /// var server = Comm.TopServer()
    ///     .UseTcpServer("0.0.0.0", 9000)
    ///     .WithHeadFootParser([0xAA], [0x55])
    ///     .OnClientConnected(id => Console.WriteLine($"客户端连接: {id}"))
    ///     .Build();
    /// await server.OpenAsync();
    /// 
    /// // 乌鸦场景 - RS485 主从
    /// var crow = Comm.Crow()
    ///     .UseSerial("COM3", 9600)
    ///     .WithHeadLengthParser([0xAA, 0x55], data => data[2])
    ///     .Timeout(5000)
    ///     .SendInterval(20)
    ///     .Build();
    /// </code>
    /// </example>
    public static class Comm
    {
        /// <summary>
        /// 创建顶层通讯口 Builder（无队列，直接收发）
        /// </summary>
        /// <returns>顶层通讯口物理口选择步骤</returns>
        /// <remarks>
        /// 顶层通讯口是最简单的通讯方式，特点是：
        /// <list type="bullet">
        ///   <item><description>无队列，直接收发</description></item>
        ///   <item><description>适合简单场景或自定义协议处理</description></item>
        ///   <item><description>通过事件接收数据</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var port = Comm.Top()
        ///     .UseSerial("COM3", 9600)
        ///     .WithHeadFootParser([0xAA], [0x55])
        ///     .OnReceived(data => Console.WriteLine(BitConverter.ToString(data)))
        ///     .Build();
        /// 
        /// await port.OpenAsync();
        /// await port.SendAsync([0x01, 0x02, 0x03]);
        /// </code>
        /// </example>
        public static ITopPhysicalPortStep Top() => new TopBuilder();

        /// <summary>
        /// 创建顶层通讯口 Builder（TCP 服务端模式）
        /// </summary>
        /// <returns>顶层通讯口 TCP 服务端物理口选择步骤</returns>
        /// <remarks>
        /// 适用于 TCP 服务端模式，特点是：
        /// <list type="bullet">
        ///   <item><description>管理多个客户端连接</description></item>
        ///   <item><description>每个客户端独立分包器</description></item>
        ///   <item><description>Build() 直接返回强类型 ITopPort_Server</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// ITopPort_Server server = Comm.TopServer()
        ///     .UseTcpServer("0.0.0.0", 9000)
        ///     .WithHeadFootParser([0xAA], [0x55])
        ///     .OnClientConnected(clientId => Console.WriteLine($"客户端连接: {clientId}"))
        ///     .OnReceived((clientId, data) => Console.WriteLine($"收到: {BitConverter.ToString(data)}"))
        ///     .Build();
        /// 
        /// await server.OpenAsync();
        /// await server.SendAsync(clientId, [0x01, 0x02, 0x03]);
        /// </code>
        /// </example>
        public static ITopServerPhysicalPortStep TopServer() => new TopServerBuilder();

        /// <summary>
        /// 创建顶层通讯口 Builder（UDP 多对多模式）
        /// </summary>
        /// <returns>顶层通讯口 UDP 多对多物理口选择步骤</returns>
        /// <remarks>
        /// 适用于 UDP 多对多（M2M）模式，特点是：
        /// <list type="bullet">
        ///   <item><description>无连接状态管理，按对端地址区分</description></item>
        ///   <item><description>每个对端独立分包器</description></item>
        ///   <item><description>Build() 直接返回强类型 ITopPort_M2M</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// ITopPort_M2M udp = Comm.TopM2M()
        ///     .UseUdp("0.0.0.0", 9000)
        ///     .WithTimeParser(50)
        ///     .OnReceived((clientId, data) => Console.WriteLine($"收到: {BitConverter.ToString(data)}"))
        ///     .Build();
        /// 
        /// await udp.OpenAsync();
        /// </code>
        /// </example>
        public static ITopM2MPhysicalPortStep TopM2M() => new TopM2MBuilder();

        /// <summary>
        /// 创建乌鸦场景 Builder
        /// </summary>
        /// <returns>乌鸦场景物理口选择步骤</returns>
        /// <remarks>
        /// 乌鸦场景适用于 RS485 主从通讯，特点是：
        /// <list type="bullet">
        ///   <item><description>请求-响应队列模式，自动排队发送</description></item>
        ///   <item><description>防止数据冲突，适合主从协议</description></item>
        ///   <item><description>支持超时重试</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var crow = Comm.Crow()
        ///     .UseSerial("COM3", 9600)
        ///     .WithHeadFootParser([0xAA], [0x55])
        ///     .Timeout(5000)
        ///     .Build();
        /// 
        /// await crow.OpenAsync();
        /// var response = await crow.RequestAsync&lt;ReadCmd, ReadRsp&gt;(new ReadCmd(1, 0x03));
        /// </code>
        /// </example>
        public static ICrowPhysicalPortStep Crow() => new CrowBuilder();

        /// <summary>
        /// 创建鸽子场景 Builder
        /// </summary>
        /// <param name="instance">主动推送事件所在实例（通常传 this）</param>
        /// <returns>鸽子场景物理口选择步骤</returns>
        /// <remarks>
        /// 鸽子场景适用于 TCP 全双工通讯，特点是：
        /// <list type="bullet">
        ///   <item><description>全双工通讯，支持被动接收</description></item>
        ///   <item><description>通过 Response 类型自动识别响应</description></item>
        ///   <item><description>支持主动推送事件</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var pigeon = Comm.Pigeon(this)
        ///     .UseTcp("192.168.1.100", 9000)
        ///     .WithHeadLengthParser([0xAA], data => data[2])
        ///     .Timeout(3000)
        ///     .Build();
        /// 
        /// await pigeon.StartAsync();
        /// var response = await pigeon.RequestAsync&lt;QueryCmd, QueryRsp&gt;(new QueryCmd());
        /// </code>
        /// </example>
        public static IPigeonPhysicalPortStep Pigeon(object instance) => new PigeonBuilder(instance);

        /// <summary>
        /// 创建老鹰场景 Builder
        /// </summary>
        /// <param name="instance">主动推送事件所在实例（通常传 this）</param>
        /// <returns>老鹰场景物理口选择步骤</returns>
        /// <remarks>
        /// 老鹰场景适用于 TCP Server，特点是：
        /// <list type="bullet">
        ///   <item><description>服务端模式，管理多个客户端连接</description></item>
        ///   <item><description>每个客户端独立分包器</description></item>
        ///   <item><description>支持向指定客户端发送请求</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var eagle = Comm.Eagle(this)
        ///     .UseTcpServer("0.0.0.0", 9000)
        ///     .WithHeadFootParser([0xAA], [0x55])
        ///     .OnClientConnected(clientId => Console.WriteLine($"客户端连接: {clientId}"))
        ///     .Build();
        /// 
        /// await eagle.StartAsync();
        /// </code>
        /// </example>
        public static IEaglePhysicalPortStep Eagle(object instance) => new EagleBuilder(instance);

        /// <summary>
        /// 创建麻雀场景 Builder
        /// </summary>
        /// <param name="instance">主动推送事件所在实例（通常传 this）</param>
        /// <returns>麻雀场景物理口选择步骤</returns>
        /// <remarks>
        /// 麻雀场景适用于 UDP 多对多通讯，特点是：
        /// <list type="bullet">
        ///   <item><description>UDP 无连接通讯</description></item>
        ///   <item><description>支持多目标发送</description></item>
        ///   <item><description>自动管理客户端列表</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var sparrow = Comm.Sparrow(this)
        ///     .UseUdp("0.0.0.0", 9000)
        ///     .WithTimeParser(50)
        ///     .Timeout(3000)
        ///     .Build();
        /// 
        /// await sparrow.StartAsync();
        /// var clientId = await sparrow.AddClientAsync("192.168.1.101", 9001);
        /// var response = await sparrow.RequestAsync&lt;UdpCmd, UdpRsp&gt;(clientId, new UdpCmd());
        /// </code>
        /// </example>
        public static ISparrowPhysicalPortStep Sparrow(object instance) => new SparrowBuilder(instance);
    }
}
