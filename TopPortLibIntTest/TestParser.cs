/********************************************************************
 * *
 * * 使本项目源码或本项目生成的DLL前请仔细阅读以下协议内容，如果你同意以下协议才能使用本项目所有的功能，
 * * 否则如果你违反了以下协议，有可能陷入法律纠纷和赔偿，作者保留追究法律责任的权利。
 * *
 * * 1、你可以在开发的软件产品中使用和修改本项目的源码和DLL，但是请保留所有相关的版权信息。
 * * 2、不能将本项目源码与作者的其他项目整合作为一个单独的软件售卖给他人使用。
 * * 3、不能传播本项目的源码和DLL，包括上传到网上、拷贝给他人等方式。
 * * 4、以上协议暂时定制，由于还不完善，作者保留以后修改协议的权利。
 * *
 * * Copyright ©2013-? yzlm Corporation All rights reserved.
 * * 作者： 曹一梁 QQ：347739303
 * * 请保留以上版权信息，否则作者将保留追究法律责任。
 * *
 * * 创建时间：2021-06-28
 * * 说明：TestParser.cs
 * *
********************************************************************/

using Communication.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser;
using Parser.Parsers;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TopPortLib;
using TopPortLib.Interfaces;

namespace TopPortLibIntTest
{
    [TestClass]
    public class TestParser
    {
        //7B 7B 11 BB 00 00 00 00 00 00 00 00 00 00 00 00 1D 05 80 41 00 00 80 BF 00 00 80 BF 00 00 80 BF 00 00 80 BF 00 07 E2 00 0A 00 7B 7B 78 45 56 12 45
        private byte[] _data = new byte[] { 0x7B, 0x7B, 0x11, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1D, 0x05, 0x80, 0x41, 0x00, 0x00, 0x80, 0xBF, 0x00, 0x00, 0x80, 0xBF, 0x00, 0x00, 0x80, 0xBF, 0x00, 0x00, 0x80, 0xBF, 0x00, 0x07, 0xE2, 0x00, 0x0A, 0x00, 0x7B, 0x7B, 0x78, 0x45, 0x56, 0x12, 0x45 };
        private TaskCompletionSource<byte[]> _tcs;
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
            ITopPort port = new TopPort(physicalPort, new HeadLengthParser(new byte[] { 0x7B }, new GetDataLengthEventHandler(async data => new GetDataLengthRsp() { ErrorCode = Parser.ErrorCode.Success, Length = 48 })));
            await port.OpenAsync();
            _tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            port.OnReceiveParsedData += async data =>
             {
                 _tcs.TrySetResult(data);
                 _tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
             };
            CancellationTokenSource cts = new CancellationTokenSource();
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
