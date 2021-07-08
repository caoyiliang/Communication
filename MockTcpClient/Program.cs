/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：Program.cs
********************************************************************/

using Communication.Bus.PhysicalPort;
using Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        private static CancellationTokenSource cts = new CancellationTokenSource();
        static void Main(string[] args)
        {
            Startasync();
            Console.ReadLine();
        }
        private static async Task Startasync()
        {
            var task = Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    IPhysicalPort port = new TcpClient("127.0.0.1", 7779);
                    await port.OpenAsync();
                    await SendDataAsync(port, "hello", cts.Token);
                    await Task.Delay(100);
                }
            });
        }

        private static async Task SendDataAsync(IPhysicalPort port, string msg, CancellationToken cancellationToken)
        {
            var task = Task.Run(async () =>
            {
                await port.SendDataAsync(Encoding.ASCII.GetBytes("Hello"), cts.Token);
                await Task.Delay(100);
            });
        }
    }
}
