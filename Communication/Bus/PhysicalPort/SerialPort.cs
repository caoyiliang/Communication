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
        private volatile TaskCompletionSource<bool> _dataReceivedTcs = CreateTaskCompletionSource();

        /// <summary>物理串口</summary>
        public SerialPort() : base()
        {
            Initialize();
        }
        /// <summary>物理串口</summary>
        public SerialPort(IContainer container) : base(container)
        {
            Initialize();
        }
        /// <summary>物理串口</summary>
        public SerialPort(string portName) : base(portName)
        {
            Initialize();
        }
        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate) : base(portName, baudRate)
        {
            Initialize();
        }
        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity) : base(portName, baudRate, parity)
        {
            Initialize();
        }
        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits) : base(portName, baudRate, parity, dataBits)
        {
            Initialize();
        }
        /// <summary>物理串口</summary>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) : base(portName, baudRate, parity, dataBits, stopBits)
        {
            Initialize();
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
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _ = Task.Run(async () =>
                    {
                        while (IsOpen && !cancellationToken.IsCancellationRequested)
                        {
                            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                            var delayTask = Task.Delay(10, cts.Token);
                            await Task.WhenAny(_dataReceivedTcs.Task, delayTask);
                            cts.Cancel();
                            if (_dataReceivedTcs.Task.IsCompleted)
                            {
                                return;
                            }
                        }
                        _dataReceivedTcs.TrySetCanceled();
                    }, cancellationToken);

                    if (BytesToRead == 0) await WaitForDataAsync(cancellationToken);
                    var data = new byte[Math.Min(BytesToRead, count)];
                    int length = await BaseStream.ReadAsync(data, 0, data.Length, cancellationToken);
                    return new ReadDataResult
                    {
                        Length = length,
                        Data = data
                    };
                }

                throw new OperationCanceledException("读取操作被取消");
            }
            finally
            {
                ResetDataReceivedTcs();
            }
        }

        /// <inheritdoc/>
        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await BaseStream.WriteAsync(data, 0, data.Length, cancellationToken);
        }

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
                    _dataReceivedTcs.TrySetResult(true);
                }
            }
            catch { }
        }

        /// <summary>
        /// 等待数据到达
        /// </summary>
        private async Task WaitForDataAsync(CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(() => _dataReceivedTcs.TrySetCanceled()))
            {
                await _dataReceivedTcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 重置 TaskCompletionSource
        /// </summary>
        private void ResetDataReceivedTcs()
        {
            Interlocked.Exchange(ref _dataReceivedTcs, CreateTaskCompletionSource());
        }

        /// <summary>
        /// 创建新的 TaskCompletionSource
        /// </summary>
        private static TaskCompletionSource<bool> CreateTaskCompletionSource()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            DataReceived += OnDataReceived;
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
                _dataReceivedTcs.TrySetCanceled();
            }
            base.Dispose(disposing);
        }
    }
}
