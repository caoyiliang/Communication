/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：SerialPort.cs
********************************************************************/

using Communication.Interfaces;
using System.ComponentModel;
using System.IO.Ports;

namespace Communication.Bus.PhysicalPort
{
    /// <summary>
    /// 物理串口
    /// </summary>
    public class SerialPort : System.IO.Ports.SerialPort, IPhysicalPort, IDisposable
    {
        public SerialPort() : base() { }

        public SerialPort(IContainer container) : base(container) { }

        public SerialPort(string portName) : base(portName) { }

        public SerialPort(string portName, int baudRate) : base(portName, baudRate) { }

        public SerialPort(string portName, int baudRate, Parity parity) : base(portName, baudRate, parity) { }

        public SerialPort(string portName, int baudRate, Parity parity, int dataBits) : base(portName, baudRate, parity, dataBits) { }

        public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) : base(portName, baudRate, parity, dataBits, stopBits) { }

        public async Task OpenAsync()
        {
            base.Open();
            await Task.CompletedTask;
        }

        public async Task CloseAsync()
        {
            base.Close();
            await Task.CompletedTask;
        }

        public async Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken)
        {
            var data = new byte[count];
            int length = base.Read(data, 0, count);
            return new ReadDataResult
            {
                Length = length,
                Data = data
            };
        }

        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            base.Write(data, 0, data.Length);
            await Task.CompletedTask;
        }
    }
}
