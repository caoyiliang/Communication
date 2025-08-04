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
        private readonly AutoResetEvent _dataAvailableEvent = new(false);

        /// <summary>物理串口</summary>
        public SerialPort() : base() => Initialize();

        /// <summary>物理串口</summary>
        public SerialPort(IContainer container) : base(container) => Initialize();

        /// <summary>物理串口</summary>
        public SerialPort(string portName) : base(portName) => Initialize();

        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate) : base(portName, baudRate) => Initialize();

        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity) : base(portName, baudRate, parity) => Initialize();

        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits) : base(portName, baudRate, parity, dataBits) => Initialize();

        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) : base(portName, baudRate, parity, dataBits, stopBits) => Initialize();

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize() => DataReceived += OnDataReceived;

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (!IsOpen)
                {
                    return;
                }

                if (BytesToRead > 0)
                {
                    _dataAvailableEvent.Set();
                }
            }
            catch { }
        }

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
            while (BytesToRead == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!IsOpen)
                {
                    throw new InvalidOperationException("Serial port is not open.");
                }
                _dataAvailableEvent.WaitOne(10);
            }

            var data = new byte[Math.Min(BytesToRead, count)];
            int length = await BaseStream.ReadAsync(data, 0, data.Length, cancellationToken);
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
    }
}
