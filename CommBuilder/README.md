# CommBuilder

Fluent Builder API for Communication Library

## 快速开始

```csharp
using CommBuilder;

// 顶层点对点（无队列）
var port = CommBuilder.Top()
    .UseSerial("COM3", 9600)
    .WithHeadFootParser([0xAA], [0x55])
    .Build();

// 顶层服务端（无队列）
var server = CommBuilder.TopServer()
    .UseTcpServer("0.0.0.0", 9000)
    .WithHeadFootParser([0xAA], [0x55])
    .Build();

// 队列版本
var crow = CommBuilder.Crow()...
var pigeon = CommBuilder.Pigeon(this)...
var eagle = CommBuilder.Eagle(this)...
var sparrow = CommBuilder.Sparrow(this)...
```

## API 参考

### CommBuilder.Top() → ITopPort

点对点模式，无队列，直接收发。

```csharp
CommBuilder.Top()
├── UseSerial(portName, baudRate, parity, dataBits, stopBits)
├── UseTcp(host, port)
├── UseNamedPipe(pipeName)
├── FromConnectionString(connectionString)
├── WithHeadLengthParser(head, lengthGetter)
├── WithHeadFootParser(head, foot)
├── WithTimeParser(intervalMs)
├── WithFootParser(foot)
├── WithParser(parser)
├── WithNoParser()
├── SendInterval(ms)
├── AutoReconnect(enable)
├── OnReceived(handler)        // Action<byte[]> 或 Func<byte[], Task>
├── OnConnected(handler)       // Action 或 Func<Task>
├── OnDisconnected(handler)    // Action 或 Func<Task>
├── OnSent(handler)            // Action<byte[]>
└── Build() → ITopPort
```

### CommBuilder.TopServer() → ITopPort_Server / ITopPort_M2M

服务端模式，无队列。

```csharp
CommBuilder.TopServer()
├── UseTcpServer(host, port)
├── UseUdp(host, port)
├── WithParserFactory(parserFactory)
├── WithHeadLengthParser(head, lengthGetter)
├── WithHeadFootParser(head, foot)
├── WithTimeParser(intervalMs)
├── WithFootParser(foot)
├── WithParser(parser)
├── WithNoParser()
├── SendInterval(ms)
├── OnReceived(handler)        // Action<Guid, byte[]> 或 Func<Guid, byte[], Task>
├── OnClientConnected(handler) // Action<Guid> 或 Func<Guid, Task>
├── OnClientDisconnected(handler)
├── OnSent(handler)            // Action<byte[], Guid>
└── Build() → object (ITopPort_Server / ITopPort_M2M)
```

### CommBuilder.Crow() → ICrowPort

乌鸦场景，RS485 主从队列。

### CommBuilder.Pigeon(instance) → IPigeonPort

鸽子场景，TCP 全双工。

### CommBuilder.Eagle(instance) → ICondorPort

老鹰场景，TCP Server 队列。

### CommBuilder.Sparrow(instance) → ISparrowPort

麻雀场景，UDP 多对多队列。

## 连接字符串格式

- 串口: `serial://COM3:9600` 或 `serial://COM3:9600:N:8:1`
- TCP: `tcp://192.168.1.100:9000`
- 命名管道: `pipe://PipeName`
