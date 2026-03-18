using Communication.Interfaces;
using System.IO.Ports;

namespace CommBuilder
{
    /// <summary>
    /// 连接字符串解析器
    /// </summary>
    /// <remarks>
    /// 支持的格式：
    /// <list type="bullet">
    ///   <item><description>串口: serial://COM3:9600 或 serial://COM3:9600:N:8:1 (port:baud:parity:dataBits:stopBits)</description></item>
    ///   <item><description>TCP客户端: tcp://192.168.1.100:9000</description></item>
    ///   <item><description>命名管道: pipe://PipeName</description></item>
    /// </list>
    /// </remarks>
    public static class ConnectionStringParser
    {
        /// <summary>
        /// 解析连接字符串，创建物理口
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>物理口实例</returns>
        /// <exception cref="ArgumentException">连接字符串格式无效</exception>
        /// <exception cref="NotSupportedException">不支持的协议类型</exception>
        public static IPhysicalPort Parse(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("连接字符串不能为空", nameof(connectionString));

            var uri = new Uri(connectionString);
            var scheme = uri.Scheme.ToLowerInvariant();

            return scheme switch
            {
                "serial" => ParseSerial(connectionString),
                "tcp" => ParseTcp(uri),
                "pipe" => ParsePipe(uri),
                _ => throw new NotSupportedException($"不支持的协议类型: {scheme}")
            };
        }

        /// <summary>
        /// 解析串口连接字符串
        /// </summary>
        private static IPhysicalPort ParseSerial(string connectionString)
        {
            var content = connectionString.Substring("serial://".Length);
            var parts = content.Split(':');
            if (parts.Length < 2)
                throw new ArgumentException($"无效的串口连接字符串格式: {connectionString}，期望格式: serial://COM3:9600 或 serial://COM3:9600:N:8:1");

            var portName = parts[0];
            if (!int.TryParse(parts[1], out var baudRate))
                throw new ArgumentException($"无效的波特率: {parts[1]}");

            if (parts.Length == 2)
            {
                return new Communication.Bus.PhysicalPort.SerialPort(portName, baudRate);
            }

            if (parts.Length >= 5)
            {
                var parity = ParseParity(parts[2]);
                if (!int.TryParse(parts[3], out var dataBits))
                    throw new ArgumentException($"无效的数据位: {parts[3]}");
                var stopBits = ParseStopBits(parts[4]);

                return new Communication.Bus.PhysicalPort.SerialPort(portName, baudRate, parity, dataBits, stopBits);
            }

            throw new ArgumentException($"无效的串口连接字符串格式: {connectionString}");
        }

        /// <summary>
        /// 解析TCP连接字符串
        /// </summary>
        private static IPhysicalPort ParseTcp(Uri uri)
        {
            var host = uri.Host;
            var port = uri.Port;

            if (port <= 0 || port > 65535)
                throw new ArgumentException($"无效的端口号: {port}");

            return new Communication.Bus.PhysicalPort.TcpClient(host, port);
        }

        /// <summary>
        /// 解析命名管道连接字符串
        /// </summary>
        private static IPhysicalPort ParsePipe(Uri uri)
        {
            var pipeName = uri.Host;
            if (string.IsNullOrEmpty(pipeName))
                throw new ArgumentException("管道名不能为空");

            return new Communication.Bus.PhysicalPort.NamedPipeClient(pipeName);
        }

        /// <summary>
        /// 解析校验位
        /// </summary>
        private static Parity ParseParity(string value)
        {
            return value.ToUpperInvariant() switch
            {
                "N" or "NONE" => Parity.None,
                "O" or "ODD" => Parity.Odd,
                "E" or "EVEN" => Parity.Even,
                "M" or "MARK" => Parity.Mark,
                "S" or "SPACE" => Parity.Space,
                _ => throw new ArgumentException($"无效的校验位: {value}，有效值: N/O/E/M/S 或 None/Odd/Even/Mark/Space")
            };
        }

        /// <summary>
        /// 解析停止位
        /// </summary>
        private static StopBits ParseStopBits(string value)
        {
            return value switch
            {
                "1" => StopBits.One,
                "1.5" => StopBits.OnePointFive,
                "2" => StopBits.Two,
                _ => throw new ArgumentException($"无效的停止位: {value}，有效值: 1/1.5/2")
            };
        }
    }
}
