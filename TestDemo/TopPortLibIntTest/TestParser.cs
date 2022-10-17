using Communication.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser;
using Parser.Parsers;
using TopPortLib;
using TopPortLib.Interfaces;

namespace TopPortLibIntTest
{
    [TestClass]
    public class TestParser
    {
        //7B 7B 11 BB 00 00 00 00 00 00 00 00 00 00 00 00 1D 05 80 41 00 00 80 BF 00 00 80 BF 00 00 80 BF 00 00 80 BF 00 07 E2 00 0A 00 7B 7B 78 45 56 12 45
        private readonly byte[] _data = new byte[] { 0x7B, 0x7B, 0x11, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1D, 0x05, 0x80, 0x41, 0x00, 0x00, 0x80, 0xBF, 0x00, 0x00, 0x80, 0xBF, 0x00, 0x00, 0x80, 0xBF, 0x00, 0x00, 0x80, 0xBF, 0x00, 0x07, 0xE2, 0x00, 0x0A, 0x00, 0x7B, 0x7B, 0x78, 0x45, 0x56, 0x12, 0x45 };
        private TaskCompletionSource<byte[]>? _tcs;
        [TestMethod]
        public async Task TestTcpAsync()
        {
            await TestAsync(new Communication.Bus.PhysicalPort.TcpClient("127.0.0.1", 2756));
        }
        [TestMethod]
        public async Task TestComAsync()
        {
            await TestAsync(new Communication.Bus.PhysicalPort.SerialPort("COM1", 9600));
        }
        public async Task TestAsync(IPhysicalPort physicalPort)
        {
            ITopPort port = new TopPort(physicalPort, new HeadLengthParser(new byte[] { 0x7B }, new GetDataLengthEventHandler(async data =>
            {
                return await Task.FromResult(new GetDataLengthRsp() { StateCode = Parser.StateCode.Success, Length = 48 });
            })));
            await port.OpenAsync();
            _tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            port.OnReceiveParsedData += async data =>
            {
                _tcs?.TrySetResult(data);
                _tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
                await Task.CompletedTask;
            };
            var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await Task.Delay(3 * 60 * 1000);
                cts.Cancel();
            });

            while (!cts.IsCancellationRequested)
            {
                var data = await _tcs.Task;
                Assert.AreEqual(_data.Length, data.Length);
                for (int i = 0; i < data.Length; i++)
                {
                    Assert.AreEqual(data[i], _data[i]);
                }
            }
        }
    }
}
