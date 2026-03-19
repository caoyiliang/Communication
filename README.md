# 概述



# Communication c# 通讯库

专为.NET环境设计的高性能、灵活且高度可扩展的通讯库。支持多种通讯口协议、分包方式和自动重连机制，能有效降低通讯代码开发成本，帮助开发者将更多精力投入业务逻辑实现。

---

## 🚀 为什么选择 Communication 通讯库

- **多通讯协议集成**：支持串口 (SerialPort)、Tcp Client/Server、UDP、蓝牙、命名管道等多种通讯方式。
- **分包算法灵活可定制**：内置多种主流分包方式，支持头尾、头长、定时分包等，并可通过实现接口自定义协议分包。
- **同步/异步全支持**：异步API设计，不卡UI，适用于复杂人机交互与后台服务场景。
- **自动重连与通讯队列**：强健的网络自动重连机制，内置通讯队列管理，显著提升稳定性。
- **跨平台广泛兼容**：支持 .NET Framework 4.8, .NET Core, .NET 8, .NET 9，易于在各类项目中集成。
- **低代码、易集成**：简明配置，极低上手门槛，开箱即用，即插即扩展。
- **扩展友好**：遵循接口设计，易于扩展通讯协议与处理流程。

---

## ⚡️ 30秒快速体验

### 使用 Fluent Builder API（推荐）

```csharp
using CommBuilder;

// ==================== 顶层通讯口（无队列） ====================

// 1. 顶层点对点 - 串口
var port = Comm.Top()
    .UseSerial("COM3", 9600)
    .WithHeadFootParser([0xAA], [0x55])
    .OnReceived(data => Console.WriteLine(BitConverter.ToString(data)))
    .Build();

await port.OpenAsync();
await port.SendAsync([0x01, 0x02, 0x03]);

// 2. 顶层点对点 - TCP 客户端
var tcp = Comm.Top()
    .UseTcp("192.168.1.100", 9000)
    .WithHeadLengthParser([0xAA], data => data[2])
    .AutoReconnect()
    .Build();

await tcp.OpenAsync();

// 3. 顶层服务端 - TCP Server
var server = Comm.TopServer()
    .UseTcpServer("0.0.0.0", 9000)
    .WithHeadFootParser([0xAA], [0x55])
    .OnClientConnected(id => Console.WriteLine($"客户端连接: {id}"))
    .OnReceived((id, data) => Handle(data, id))
    .Build();

await server.OpenAsync();

// 4. 顶层服务端 - UDP 多对多
var udp = Comm.TopServer()
    .UseUdp("0.0.0.0", 9000)
    .WithTimeParser(50)
    .Build();

// ==================== 队列版本 ====================

// 乌鸦场景 - RS485 主从通讯（请求-响应队列）
var crow = Comm.Crow()
    .UseSerial("COM3", 9600)
    .WithHeadFootParser([0xAA], [0x55])
    .Timeout(5000)
    .SendInterval(20)
    .Build();

await crow.OpenAsync();
var response = await crow.RequestAsync<ReadCmd, ReadRsp>(new ReadCmd(1, 0x03));

// 鸽子场景 - TCP 全双工（支持主动推送）
var pigeon = Comm.Pigeon(this)
    .UseTcp("192.168.1.100", 9000)
    .WithHeadLengthParser([0xAA], data => data[2])
    .Timeout(3000)
    .Build();

await pigeon.StartAsync();

// 老鹰场景 - TCP Server（队列版本）
var eagle = Comm.Eagle(this)
    .UseTcpServer("0.0.0.0", 9000)
    .WithHeadFootParser([0xAA], [0x55])
    .Build();

await eagle.StartAsync();

// 麻雀场景 - UDP 多对多（队列版本）
var sparrow = Comm.Sparrow(this)
    .UseUdp("0.0.0.0", 9000)
    .WithTimeParser(50)
    .Build();

await sparrow.StartAsync();
```

### 传统方式

以TCP服务器接入为例：

```csharp
using Communication.Bus;
using Communication.Interfaces;

// 1. 创建TcpServer，配置端口
IBusPort tcpServer = new TcpServer("127.0.0.1", 9000);

// 2. 注册事件与回调
tcpServer.OnReceived += (sender, args) => {
    // 处理收到的数据包
};

// 3. 启动服务
tcpServer.StartAsync();
```

如需接入自定义协议，直接实现 `IParser` 即可无缝接入分包处理。

---

## 📦 安装方式

### 1. NuGet 包安装

```shell
# 核心库
NuGet\Install-Package CSoft.TopPortLib -Version 9.12.0

# Fluent Builder API（推荐）
NuGet\Install-Package CSoft.CommBuilder -Version 1.0.0
```

### 2. 本地源码集成

下载 [GitHub项目](https://github.com/caoyiliang/Communication) 源码或将其作为子模块导入。

将 `/Communication`、`/TopPortLib`、`/Crow`、`/Parser`、`/CommBuilder` 等核心目录作为项目引用即可。

### 2. .NET平台兼容矩阵

- **.NET Framework 4.8** ✅
- **.NET Core** ✅
- **.NET 8, 9** ✅

---

## 🛠️ 基础用法

### 多种通讯接口

#### 1. 串口通讯

```csharp
using Communication.Bus.PhysicalPort;

var serialPort = new SerialPort("COM1", 9600);
// 配置与启动
serialPort.OpenAsync();
```

#### 2. TcpClient 通讯

```csharp
using Communication.Bus.PhysicalPort;

var tcpClient = new TcpClient("127.0.0.1", 9000);
tcpClient.OpenAsync();
```

#### 3. 命名管道通讯

```csharp
using Communication.Bus.PhysicalPort;

var pipeClient = new NamedPipeClient("PipeName");
pipeClient.OpenAsync();
```

#### 4. 蓝牙通讯

```csharp
using Communication.Bluetooth;

var bluetooth = new BluetoothClassic("设备MAC地址");
bluetooth.ConnectAsync();
```

### 分包方式定制

实现 `IParser`，支持任意自定义协议：

```csharp
public class MyParser : IParser
{
    public IEnumerable<byte[]> Parse(byte[] buffer)
    {
        // 自定义分包算法...
    }
}
```

---

## 📚 API 参考

### 主要接口与类

#### 物理通讯口

- `IPhysicalPort`：物理口基接口（串口/TCP/蓝牙/命名管道等）
- `SerialPort, TcpClient, NamedPipeClient, BluetoothClassic`：各类物理口实现类
- `OpenAsync()/CloseAsync()`：开启/关闭通讯
- `SendAsync(byte[] data)`：发送数据

#### 逻辑通讯总线

- `IBusPort`：总线抽象层（数据包收发）
- `TcpServer, Udp, NamedPipeServer`：各类服务端实现
- `StartAsync()/StopAsync()`：启动/停止服务
- `OnReceived`：数据包接收事件

#### 分包处理

- `IParser`：数据包分割接口
- `BaseParser, HeadFootParser, HeadLengthParser, TimeParser`：内建协议分割实现
- 可自定义实现Protocol Parser，提升适用性

#### 异常处理

- `ConnectFailedException, NotConnectedException, SendException`（Communication/Exceptions）
- `RequestParameterToBytesFailedException, ResponseCreateFailedException`（TopPortLib/Exceptions）

#### 高阶抽象

- `TopPort, CondorPort, CrowPort, PigeonPort`（TopPortLib）
    - 针对典型工业设备/协议接入的高阶抽象
    - 提供更丰富的命令控制与响应处理接口

---

## 🎯 进阶用法

### 使用 Fluent Builder API

CommBuilder 提供了简洁的 Fluent API，大幅降低入门门槛：

#### 如何选择？

```
┌─────────────────────────────────────────────────────────────────┐
│                        通讯场景选择                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  你需要管理多个客户端吗？                                        │
│                                                                 │
│  ├─ 是 → 需要服务端模式                                         │
│  │        │                                                    │
│  │        ├─ 简单收发（无队列）→ TopServer()                   │
│  │        │                            ├─ TCP Server → ITopPort_Server │
│  │        │                            └─ UDP M2M → ITopPort_M2M     │
│  │        │                                                    │
│  │        └─ 需要请求-响应队列 → 老鹰(Eagle) / 麻雀(Sparrow)    │
│  │                                                                  │
│  └─ 否 → 点对点模式                                              │
│           │                                                      │
│           ├─ 简单收发（无队列）→ Top()                          │
│           │                            ├─ 串口 → ITopPort       │
│           │                            ├─ TCP → ITopPort         │
│           │                            └─ 管道 → ITopPort        │
│           │                                                      │
│           └─ 需要请求-响应队列 → 乌鸦(Crow) / 鸽子(Pigeon)      │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

#### 六种场景

| 场景 | 适用协议 | 特点 | 返回类型 |
|------|----------|------|----------|
| **顶层 (Top)** | 串口/TCP客户端/管道 | 无队列，直接收发，最简单 | `ITopPort` |
| **顶层服务端 (TopServer)** | TCP Server/UDP | 无队列，服务端模式 | `ITopPort_Server` / `ITopPort_M2M` |
| **乌鸦 (Crow)** | RS485/串口 | 主从队列，请求-响应模式 | `ICrowPort` |
| **鸽子 (Pigeon)** | TCP 全双工 | 支持被动推送，实时通讯 | `IPigeonPort` |
| **老鹰 (Eagle)** | TCP Server | 服务端多客户端管理 | `ICondorPort` |
| **麻雀 (Sparrow)** | UDP | 多对多广播通讯 | `ISparrowPort` |

#### 无队列 vs 有队列

| 特性 | 无队列 (Top/TopServer) | 有队列 (Crow/Pigeon/Eagle/Sparrow) |
|------|------------------------|-----------------------------------|
| 发送方式 | `SendAsync(data)` 直接发送 | `RequestAsync<TReq, TRsp>(req)` 请求-响应 |
| 队列管理 | ❌ 无 | ✅ 自动排队，防止数据冲突 |
| 超时重试 | ❌ 无 | ✅ 请求超时自动重试 |
| 响应匹配 | ❌ 手动处理 | ✅ 自动通过 Response 类型匹配 |
| 适用场景 | 简单收发、自定义协议 | 工业设备、Modbus 等主从协议 |

#### 完整示例代码

##### 1. 顶层点对点 - 串口（无队列）

```csharp
using CommBuilder;

// 串口 + 头尾分包
var port = Comm.Top()
    .UseSerial("COM3", 9600)
    .WithHeadFootParser([0xAA], [0x55])
    .OnReceived(data => Console.WriteLine(BitConverter.ToString(data)))
    .OnConnected(() => Console.WriteLine("已连接"))
    .OnDisconnected(() => Console.WriteLine("已断开"))
    .Build();

await port.OpenAsync();

// 发送数据
await port.SendAsync([0x01, 0x02, 0x03]);
```

##### 2. 顶层点对点 - TCP（无队列）

```csharp
// TCP + 头长度分包 + 自动重连
var tcp = Comm.Top()
    .UseTcp("192.168.1.100", 9000)
    .WithHeadLengthParser([0xAA, 0x55], data => data[2])  // 帧头 + 长度字段
    .AutoReconnect()  // 断线自动重连
    .OnReceived(data => Handle(data))
    .Build();

await tcp.OpenAsync();
```

##### 3. 顶层服务端 - TCP Server（无队列）

```csharp
using CommBuilder;

// TCP Server，每个客户端独立分包器
var server = Comm.TopServer()
    .UseTcpServer("0.0.0.0", 9000)
    .WithHeadFootParser([0xAA], [0x55])
    .OnClientConnected(clientId => Console.WriteLine($"客户端连接: {clientId}"))
    .OnClientDisconnected(clientId => Console.WriteLine($"客户端断开: {clientId}"))
    .OnReceived((clientId, data) =>
    {
        Console.WriteLine($"收到来自 {clientId}: {BitConverter.ToString(data)}");
        // 处理数据后回复
        await server.SendAsync(clientId, [0x01, 0x02]);
    })
    .Build();

await server.OpenAsync();
```

##### 4. 顶层服务端 - UDP 多对多（无队列）

```csharp
// UDP 多对多
var udp = Comm.TopServer()
    .UseUdp("0.0.0.0", 9000)
    .WithTimeParser(50)  // 定时分包
    .OnReceived((clientId, data) => Handle(data))
    .Build();

await udp.OpenAsync();

// 添加目标客户端
var clientId = await udp.AddClientAsync("192.168.1.101", 9001);

// 向指定客户端发送
await udp.SendAsync(clientId, [0x01, 0x02, 0x03]);
```

##### 5. 乌鸦 - RS485 主从队列

```csharp
// RS485 主从通讯，自动队列
var crow = Comm.Crow()
    .UseSerial("COM3", 9600)
    .WithHeadLengthParser([0xAA, 0x55], data => data[2])
    .Timeout(5000)     // 请求超时 5 秒
    .SendInterval(20)  // 发送间隔 20ms，防止粘包
    .Build();

await crow.OpenAsync();

// 请求-响应模式，自动排队
var response = await crow.RequestAsync<ReadCmd, ReadRsp>(new ReadCmd(1, 0x03));
```

##### 6. 鸽子 - TCP 全双工

```csharp
// TCP 全双工，支持主动推送
var pigeon = Comm.Pigeon(this)
    .UseTcp("192.168.1.100", 9000)
    .WithHeadFootParser([0xAA], [0x55])
    .Timeout(3000)
    .Build();

await pigeon.StartAsync();

// 请求-响应
var response = await pigeon.RequestAsync<QueryCmd, QueryRsp>(new QueryCmd());

// 主动推送通过事件处理（在 this 类中定义对应事件）
// private void OnPushDataEventHandler(object? sender, PushData e) { }
```

#### 连接字符串支持

```csharp
// 支持连接字符串快速创建
var crow = Comm.Crow()
    .FromConnectionString("serial://COM3:9600:N:8:1")
    .WithHeadFootParser([0xAA], [0x55])
    .Build();

var pigeon = Comm.Pigeon(this)
    .FromConnectionString("tcp://192.168.1.100:9000")
    .WithHeadLengthParser([0xAA], data => data[2])
    .Build();
```

连接字符串格式：
- 串口: `serial://COM3:9600` 或 `serial://COM3:9600:N:8:1`
- TCP: `tcp://192.168.1.100:9000`
- 命名管道: `pipe://PipeName`

### 自定义分包协议接入

只需实现 `IParser` 接口，即可用自定义协议处理底层字节流。例如：

```csharp
public class CustomParser : IParser { ... }
ITopPort port = new TopPort(new TcpClient("127.0.0.1", 9000), new CustomParser());
```

### 上层协议栈封装

`TopPortLib` 提供了对工业协议常用模式的抽象，可灵活叠加堆栈组合业务逻辑，如异步M2M、设备逻辑拆分等。

### 测试示例工程

`/TestDemo` 下包含众多实际协议、分包和通讯测试用例，例如：
- `TestNamedPipeClient`, `TcpServer`, `TestUdp1`, `CondorPortProtocolDemo` 等，
提供从基础通讯到进阶协议解码的全流程样例。

---

## 🔧 配置选项

支持自定义通讯参数，如串口波特率、网络端口、协议分包超时时间等，大多通过构造函数或属性设置即可。

```csharp
var serial = new SerialPort(portName: "COM2", baudRate: 9600, parity: Parity.None);
```

对于高级需求，参见各类接口提供的扩展属性（详见源码与examples）。

---

## 🧩 主要集成与扩展方式

### 框架支持

- 原生支持WPF、WinForms、控制台等任何.NET工程类型。
- 作为通讯层可与业务中间件、工业协议栈、SCADA系统无缝集成。

### 扩展插件机制

- 任意新增通讯协议，仅需实现对应接口 (`IPhysicalPort`, `IParser`)。
- 可叠加应用层协议，伴随队列、重连、缓存、事件驱动等机制。

---

## 📊 性能与质量

- **异步架构，高并发友好**：所有通讯操作异步实现，不卡UI。
- **内存优化**：分包算法避免无效字节流淤积，内建安全防护。
- **稳定性保障**：自愈通讯机制，断线自动重连，错误有效上报。

---

## 🔗 兼容性与升级指南

- **兼容 .NET Framework 4.8, .NET Core, .NET 8/9**，支持绝大多数主流.NET应用环境。
- 新增协议或通讯方式时，仅需实现标准接口，无需修改核心库。
- 原有分包/设备逻辑如需迁移，仅需替换配置，无需重构业务代码。

---

## 🕹️ 学习与支持

### 入门建议

1. 从TestDemo工程实际运行示例开始理解调用方式与数据流转。
2. 仔细阅读各接口源文件与内建Parser算法实现，理解协议扩展机制。
3. 如有业务特殊需求，优先选择继承/实现相关接口适配扩展。

### 社区与维护

- [源码仓库](https://github.com/caoyiliang/Communication)
- 欢迎issue/PR，共同维护完善
- 框架持续维护，适配新协议和.NET新特性

---

## 总结

Communication通讯库是.NET下通用高扩展性的通讯中间件，开箱即用，易于扩展，可显著提升上位机通讯开发效率，减少代码出错概率并提升系统健壮性。适合各类工控、物联网、业务信息系统场景。

**立即集成，专注业务，高效通讯！**

