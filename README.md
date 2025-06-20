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

以TCP服务器接入为例，轻松几步即可启动一个异步通讯服务器（具体API见后文详解）：

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

### 1. 本地源码集成

提供Nuget包管理发布下载，也可直接下载 [GitHub项目](https://github.com/caoyiliang/Communication) 源码或将其作为子模块导入。

```shell
NuGet\Install-Package CSoft.TopPortLib -Version 9.12.0
```

将 `/Communication`、`/TopPortLib`、`/Crow`、`/Parser` 等核心目录作为项目引用即可。

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

### 自定义分包协议接入

只需实现 `IParser` 接口，即可用自定义协议处理底层字节流。例如：

```csharp
public class CustomParser : IParser { ... }
IBusPort port = new TcpServer("127.0.0.1", 9000, new CustomParser());
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

