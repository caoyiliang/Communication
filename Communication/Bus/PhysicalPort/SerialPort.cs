using Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

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
            await TaskUtils.NullTask;
        }

        public async Task CloseAsync()
        {
            base.Close();
            await TaskUtils.NullTask;
        }

        public async Task<int> ReadDataAsync(byte[] data, int count, CancellationToken cancellationToken)
        {
            return await Task.FromResult(base.Read(data, 0, count));
        }

        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            base.Write(data, 0, data.Length);
            await TaskUtils.NullTask;
        }
    }
}
