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
        /// <summary>物理串口</summary>
        public SerialPort() : base() { }

        /// <summary>物理串口</summary>
        public SerialPort(IContainer container) : base(container) { }

        /// <summary>物理串口</summary>
        public SerialPort(string portName) : base(portName) { }

        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate) : base(portName, baudRate) { }

        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity) : base(portName, baudRate, parity) { }

        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits) : base(portName, baudRate, parity, dataBits) { }

        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) : base(portName, baudRate, parity, dataBits, stopBits) { }

        /// <inheritdoc/>
        public Task OpenAsync()
        {
            Open();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task CloseAsync()
        {
            Close();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken)
        {
            int length = 0;
            var data = new byte[count];
            while (length == 0)
            {
                if (!IsOpen)
                {
                    throw new InvalidOperationException("Serial port is not open.");
                }
                if (BytesToRead == 0)
                {
                    await Task.Delay(10, cancellationToken);
                    continue;
                }
                try
                {
                    length = await BaseStream.ReadAsync(data, 0, count, cancellationToken);
                }
                catch { }
            }

            return new ReadDataResult
            {
                Length = length,
                Data = data
            };
        }

        /// <inheritdoc/>
        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await BaseStream.WriteAsync(data, 0, data.Length, cancellationToken);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return PortName;
        }
    }
}
