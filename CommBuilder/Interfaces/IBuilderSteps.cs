using Parser.Interfaces;
using System.IO.Ports;

namespace CommBuilder.Interfaces
{
    #region 乌鸦场景 (RS485 主从)

    /// <summary>
    /// 乌鸦场景 - 物理口选择步骤
    /// </summary>
    public interface ICrowPhysicalPortStep
    {
        /// <summary>
        /// 使用串口
        /// </summary>
        /// <param name="portName">端口名，如 COM3</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        ICrowParserStep UseSerial(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One);

        /// <summary>
        /// 使用TCP客户端
        /// </summary>
        /// <param name="host">主机地址</param>
        /// <param name="port">端口</param>
        ICrowParserStep UseTcp(string host, int port);

        /// <summary>
        /// 使用命名管道
        /// </summary>
        /// <param name="pipeName">管道名</param>
        ICrowParserStep UseNamedPipe(string pipeName);

        /// <summary>
        /// 从连接字符串创建物理口
        /// </summary>
        /// <param name="connectionString">连接字符串，如 serial://COM3:9600 或 tcp://192.168.1.100:9000</param>
        ICrowParserStep FromConnectionString(string connectionString);
    }

    /// <summary>
    /// 乌鸦场景 - 分包器选择步骤
    /// </summary>
    public interface ICrowParserStep
    {
        /// <summary>
        /// 使用头长度分包器
        /// </summary>
        /// <param name="head">帧头字节</param>
        /// <param name="lengthGetter">从数据中获取长度的委托，参数为数据，返回长度</param>
        ICrowConfigureStep WithHeadLengthParser(byte[] head, Func<byte[], int> lengthGetter);

        /// <summary>
        /// 使用头尾分包器
        /// </summary>
        /// <param name="head">帧头字节</param>
        /// <param name="foot">帧尾字节</param>
        ICrowConfigureStep WithHeadFootParser(byte[] head, byte[] foot);

        /// <summary>
        /// 使用定时分包器
        /// </summary>
        /// <param name="intervalMs">时间间隔(毫秒)，默认50ms</param>
        ICrowConfigureStep WithTimeParser(int intervalMs = 50);

        /// <summary>
        /// 使用自定义分包器
        /// </summary>
        /// <param name="parser">分包器实例</param>
        ICrowConfigureStep WithParser(IParser parser);

        /// <summary>
        /// 不使用分包器（原始字节流）
        /// </summary>
        ICrowConfigureStep WithNoParser();
    }

    /// <summary>
    /// 乌鸦场景 - 配置步骤
    /// </summary>
    public interface ICrowConfigureStep
    {
        /// <summary>
        /// 设置请求超时时间
        /// </summary>
        /// <param name="ms">超时毫秒数</param>
        ICrowConfigureStep Timeout(int ms);

        /// <summary>
        /// 设置发送间隔时间（防止粘包）
        /// </summary>
        /// <param name="ms">间隔毫秒数</param>
        ICrowConfigureStep SendInterval(int ms);

        /// <summary>
        /// 设置接收到数据的回调
        /// </summary>
        /// <param name="handler">处理函数</param>
        ICrowConfigureStep OnReceived(Func<byte[], Task> handler);

        /// <summary>
        /// 设置接收到数据的回调
        /// </summary>
        /// <param name="handler">处理函数</param>
        ICrowConfigureStep OnReceived(Action<byte[]> handler);

        /// <summary>
        /// 设置连接成功回调
        /// </summary>
        /// <param name="handler">处理函数</param>
        ICrowConfigureStep OnConnected(Func<Task> handler);

        /// <summary>
        /// 设置断开连接回调
        /// </summary>
        /// <param name="handler">处理函数</param>
        ICrowConfigureStep OnDisconnected(Func<Task> handler);

        /// <summary>
        /// 构建乌鸦通讯口
        /// </summary>
        /// <returns>乌鸦通讯口实例</returns>
        TopPortLib.Interfaces.ICrowPort Build();
    }

    #endregion

    #region 鸽子场景 (TCP 全双工)

    /// <summary>
    /// 鸽子场景 - 物理口选择步骤
    /// </summary>
    public interface IPigeonPhysicalPortStep
    {
        /// <summary>
        /// 使用串口
        /// </summary>
        IPigeonParserStep UseSerial(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One);

        /// <summary>
        /// 使用TCP客户端
        /// </summary>
        IPigeonParserStep UseTcp(string host, int port);

        /// <summary>
        /// 使用命名管道
        /// </summary>
        IPigeonParserStep UseNamedPipe(string pipeName);

        /// <summary>
        /// 从连接字符串创建
        /// </summary>
        IPigeonParserStep FromConnectionString(string connectionString);
    }

    /// <summary>
    /// 鸽子场景 - 分包器选择步骤
    /// </summary>
    public interface IPigeonParserStep
    {
        /// <summary>
        /// 使用头长度分包器
        /// </summary>
        IPigeonConfigureStep WithHeadLengthParser(byte[] head, Func<byte[], int> lengthGetter);

        /// <summary>
        /// 使用头尾分包器
        /// </summary>
        IPigeonConfigureStep WithHeadFootParser(byte[] head, byte[] foot);

        /// <summary>
        /// 使用定时分包器
        /// </summary>
        IPigeonConfigureStep WithTimeParser(int intervalMs = 50);

        /// <summary>
        /// 使用自定义分包器
        /// </summary>
        IPigeonConfigureStep WithParser(IParser parser);

        /// <summary>
        /// 不使用分包器
        /// </summary>
        IPigeonConfigureStep WithNoParser();
    }

    /// <summary>
    /// 鸽子场景 - 配置步骤
    /// </summary>
    public interface IPigeonConfigureStep
    {
        /// <summary>
        /// 设置请求超时时间
        /// </summary>
        IPigeonConfigureStep Timeout(int ms);

        /// <summary>
        /// 设置发送间隔时间
        /// </summary>
        IPigeonConfigureStep SendInterval(int ms);

        /// <summary>
        /// 设置校验方法
        /// </summary>
        IPigeonConfigureStep WithCheck(Func<byte[], Task<bool>> checkFunc);

        /// <summary>
        /// 设置校验方法
        /// </summary>
        IPigeonConfigureStep WithCheck(Func<byte[], bool> checkFunc);

        /// <summary>
        /// 构建鸽子通讯口
        /// </summary>
        TopPortLib.Interfaces.IPigeonPort Build();
    }

    #endregion

    #region 老鹰场景 (TCP Server)

    /// <summary>
    /// 老鹰场景 - 物理口选择步骤
    /// </summary>
    public interface IEaglePhysicalPortStep
    {
        /// <summary>
        /// 使用TCP服务端
        /// </summary>
        /// <param name="host">监听地址</param>
        /// <param name="port">监听端口</param>
        IEagleParserStep UseTcpServer(string host, int port);
    }

    /// <summary>
    /// 老鹰场景 - 分包器选择步骤
    /// </summary>
    public interface IEagleParserStep
    {
        /// <summary>
        /// 设置分包器工厂（每个客户端独立分包器）
        /// </summary>
        IEagleConfigureStep WithParserFactory(Func<IParser> parserFactory);

        /// <summary>
        /// 使用头长度分包器
        /// </summary>
        IEagleConfigureStep WithHeadLengthParser(byte[] head, Func<byte[], int> lengthGetter);

        /// <summary>
        /// 使用头尾分包器
        /// </summary>
        IEagleConfigureStep WithHeadFootParser(byte[] head, byte[] foot);

        /// <summary>
        /// 使用定时分包器
        /// </summary>
        IEagleConfigureStep WithTimeParser(int intervalMs = 50);
    }

    /// <summary>
    /// 老鹰场景 - 配置步骤
    /// </summary>
    public interface IEagleConfigureStep
    {
        /// <summary>
        /// 设置请求超时时间
        /// </summary>
        IEagleConfigureStep Timeout(int ms);

        /// <summary>
        /// 设置客户端连接回调
        /// </summary>
        IEagleConfigureStep OnClientConnected(Func<Guid, Task> handler);

        /// <summary>
        /// 设置客户端断开回调
        /// </summary>
        IEagleConfigureStep OnClientDisconnected(Func<Guid, Task> handler);

        /// <summary>
        /// 构建老鹰通讯口
        /// </summary>
        TopPortLib.Interfaces.ICondorPort Build();
    }

    #endregion

    #region 麻雀场景 (UDP 多对多)

    /// <summary>
    /// 麻雀场景 - 物理口选择步骤
    /// </summary>
    public interface ISparrowPhysicalPortStep
    {
        /// <summary>
        /// 使用UDP
        /// </summary>
        /// <param name="host">监听地址</param>
        /// <param name="port">监听端口</param>
        ISparrowParserStep UseUdp(string host, int port);
    }

    /// <summary>
    /// 麻雀场景 - 分包器选择步骤
    /// </summary>
    public interface ISparrowParserStep
    {
        /// <summary>
        /// 设置分包器工厂
        /// </summary>
        ISparrowConfigureStep WithParserFactory(Func<IParser> parserFactory);

        /// <summary>
        /// 使用头长度分包器
        /// </summary>
        ISparrowConfigureStep WithHeadLengthParser(byte[] head, Func<byte[], int> lengthGetter);

        /// <summary>
        /// 使用头尾分包器
        /// </summary>
        ISparrowConfigureStep WithHeadFootParser(byte[] head, byte[] foot);

        /// <summary>
        /// 使用定时分包器
        /// </summary>
        ISparrowConfigureStep WithTimeParser(int intervalMs = 50);
    }

    /// <summary>
    /// 麻雀场景 - 配置步骤
    /// </summary>
    public interface ISparrowConfigureStep
    {
        /// <summary>
        /// 设置请求超时时间
        /// </summary>
        ISparrowConfigureStep Timeout(int ms);

        /// <summary>
        /// 构建麻雀通讯口
        /// </summary>
        TopPortLib.Interfaces.ISparrowPort Build();
    }

    #endregion
}
