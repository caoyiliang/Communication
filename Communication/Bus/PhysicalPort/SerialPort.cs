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
        private TaskCompletionSource<bool> _dataReceivedTcs = new();

        /// <summary>物理串口</summary>
        public SerialPort() : base()
        {
            DataReceived += OnDataReceived;
        }
        /// <summary>物理串口</summary>
        public SerialPort(IContainer container) : base(container)
        {
            DataReceived += OnDataReceived;
        }
        /// <summary>物理串口</summary>
        public SerialPort(string portName) : base(portName)
        {
            DataReceived += OnDataReceived;
        }
        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate) : base(portName, baudRate)
        {
            DataReceived += OnDataReceived;
        }
        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity) : base(portName, baudRate, parity)
        {
            DataReceived += OnDataReceived;
        }
        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits) : base(portName, baudRate, parity, dataBits)
        {
            DataReceived += OnDataReceived;
        }
        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) : base(portName, baudRate, parity, dataBits, stopBits)
        {
            DataReceived += OnDataReceived;
        }

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            Open();
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            Close();
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // 等待数据到达
                await WaitForDataAsync(cancellationToken);

                int available = BytesToRead;
                if (available > 0)
                {
                    var data = new byte[Math.Min(available, count)];
                    int length = await BaseStream.ReadAsync(data, 0, data.Length, cancellationToken);
                    return new ReadDataResult
                    {
                        Length = length,
                        Data = data
                    };
                }

                // 重置 TaskCompletionSource
                ResetDataReceivedTcs();
            }

            throw new OperationCanceledException("读取操作被取消");
        }

        /// <inheritdoc/>
        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await BaseStream.WriteAsync(data, 0, data.Length, cancellationToken);
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // 当有数据到达时，设置 TaskCompletionSource 为完成状态
            _dataReceivedTcs.TrySetResult(true);
        }

        /// <summary>
        /// 等待数据到达
        /// </summary>
        private async Task WaitForDataAsync(CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(() => _dataReceivedTcs.TrySetCanceled()))
            {
                await _dataReceivedTcs.Task;
            }
        }

        /// <summary>
        /// 重置 TaskCompletionSource
        /// </summary>
        private void ResetDataReceivedTcs()
        {
            if (_dataReceivedTcs.Task.IsCompleted)
            {
                _dataReceivedTcs = new TaskCompletionSource<bool>();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DataReceived -= OnDataReceived;
            }
            base.Dispose(disposing);
        }
    }
}
