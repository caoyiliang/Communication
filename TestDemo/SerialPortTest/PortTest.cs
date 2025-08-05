using Communication.Bus.PhysicalPort;
using Parser;
using Parser.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopPortLib;
using TopPortLib.Interfaces;

namespace SerialPortTest
{
    public class PortTest
    {
        private ITopPort parsedPort;

        public PortTest()
        {
            parsedPort = new TopPort(new SerialPort("COM3", 115200), new TimeParser());
            parsedPort.OnReceiveParsedData += ReceiverDataEventAsync;
            parsedPort.OnConnect += ParsedPort_OnConnect;
            parsedPort.OnDisconnect += ParsedPort_OnDisconnect;
        }

        public async Task Open()
        {
            try
            {
                await parsedPort.OpenAsync(false);
            }
            catch (Exception)
            {
                Trace.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}---->打开失败!!!!");
            }
        }

        public async Task Close()
        {
            await parsedPort.CloseAsync();
        }

        private async Task ParsedPort_OnDisconnect()
        {
            Trace.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}---->断开连接");

            await Task.CompletedTask;
        }

        private async Task ParsedPort_OnConnect()
        {
            Trace.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}---->连接设备");

            await Task.CompletedTask;
        }

        private async Task ReceiverDataEventAsync(byte[] data)
        {
            if (data == null || data.Length <= 0)
            {
                return;
            }

            Trace.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}---->接收数据：{StringUtil.BytesToString(data)}");
            await Task.CompletedTask;
        }

        public async Task SendAsync()
        {
            await parsedPort.SendAsync(new byte[] { 0xD4, 0xF3, 0xCC, 0xEC, 0x00, 0x17, 0x00, 0x03, 0x03, 0x01, 0x97, 0x00, 0x02, 0x00, 0x04, 0xE4, 0xE4, 0xE4, 0xE4, 0x3C, 0xD4, 0xBB, 0xAA });
        }
    }
}
