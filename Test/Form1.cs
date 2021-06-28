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
 * * 说明：Form1.cs
 * *
********************************************************************/

using TopPortLib.Interfaces;
using Communication.Bus;
using Communication.Bus.PhysicalPort;
using Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using Parser.Parsers;
using Parser;
using TopPortLib;

namespace Test
{
    public partial class Form1 : Form
    {
        private Dictionary<int, IPhysicalPort> physicalPorts = new Dictionary<int, IPhysicalPort>();
        private ITopPort parsedPort;
        private byte[] head = new byte[] { 0x7B };
        private byte[] foot = new byte[] { 0x04, 0x06 };
        public Form1()
        {
            InitializeComponent();
            physicalPorts[0] = new SerialPort("COM1", 9600);
            physicalPorts[1] = new TcpClient("127.0.0.1", 2756);
            parsedPort = new TopPort(physicalPorts[1], new FootParser(foot));
            parsedPort.OnReceiveParsedData += ReceiverDataEventAsync;
            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        await parsedPort.SendAsync(new byte[] { 0x01, 0x03, 0x10, 0x02, 0x00, 0x04, 0xE1, 0x09 });
            //        await Task.Delay(3000);
            //    }
            //});
        }

        private async Task ReceiverDataEventAsync(byte[] data)
        {
            try
            {
                await this.InvokeAsync(() =>
                 {
                     richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + (checkBox1.Checked ? StringUtil.BytesToString(data) : Encoding.Default.GetString(data)) + "\n");
                     richTextBox1.ScrollToCaret();
                 });
            }
            catch { }
        }

        private async void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == -1) return;
            await parsedPort.CloseAsync();
            parsedPort.PhysicalPort = physicalPorts[comboBox2.SelectedIndex];
            await parsedPort.OpenAsync();
            comboBox1.Enabled = true;
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1) return;
            await parsedPort.CloseAsync();
            if (comboBox1.SelectedIndex == 0)
            {
                parsedPort = new TopPort(physicalPorts[comboBox2.SelectedIndex], new TimeParser(200));
                parsedPort.OnReceiveParsedData += ReceiverDataEventAsync;
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                parsedPort = new TopPort(physicalPorts[comboBox2.SelectedIndex], new HeadFootParser(head, foot));
                parsedPort.OnReceiveParsedData += ReceiverDataEventAsync;
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                parsedPort = new TopPort(physicalPorts[comboBox2.SelectedIndex], new HeadLengthParser(head, async data =>
                 {
                     if (data.Length < 2) return new GetDataLengthRsp() { ErrorCode = Parser.ErrorCode.LengthNotEnough };
                     return await Task.FromResult(new GetDataLengthRsp() { Length = data[1], ErrorCode = Parser.ErrorCode.Success });
                 }));
                parsedPort.OnReceiveParsedData += ReceiverDataEventAsync;
            }
            else if (comboBox1.SelectedIndex == 3)
            {
                parsedPort = new TopPort(physicalPorts[comboBox2.SelectedIndex], new FootParser(foot));
                parsedPort.OnReceiveParsedData += ReceiverDataEventAsync;
            }
            await parsedPort.OpenAsync();
        }

        private async void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            await parsedPort.SendAsync(new byte[] { 0x01, 0x03, 0x10, 0x02, 0x00, 0x04, 0xE1, 0x09 });
        }
    }
}
