using Communication.Interfaces;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Net.Sockets;

namespace Communication.Bluetooth
{
    public class BluetoothClassic(string address) : IPhysicalPort
    {
        private bool _disposed = false;
        private readonly BluetoothClient _bluetoothClient = new();
        private readonly BluetoothAddress _bluetoothAddress = new(BluetoothAddress.Parse(address));
        private NetworkStream? _networkStream;
        public bool IsOpen => _bluetoothClient.Connected;

        public async Task CloseAsync()
        {
            _networkStream?.Close();
            _bluetoothClient.Close();
            await Task.CompletedTask;
        }

        public async Task OpenAsync()
        {
            _bluetoothClient.Connect(new BluetoothEndPoint(_bluetoothAddress, BluetoothService.SerialPort));
            _networkStream = _bluetoothClient.GetStream();
            await Task.CompletedTask;
        }

        public async Task<ReadDataResult> ReadDataAsync(int count, CancellationToken cancellationToken)
        {
            var data = new byte[count];
            int length = await _networkStream!.ReadAsync(data, 0, count, cancellationToken);
            return new ReadDataResult
            {
                Length = length,
                Data = data
            };
        }

        public async Task SendDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await _networkStream!.WriteAsync(data, 0, data.Length, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    _networkStream?.Dispose();
                    _bluetoothClient?.Dispose();
                }

                // 释放其他非托管资源

                _disposed = true;
            }
        }

        ~BluetoothClassic()
        {
            Dispose(false);
        }
    }
}
