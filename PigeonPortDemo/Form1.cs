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

using Parser.Parsers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TopPortLib;
using TopPortLib.Interfaces;

namespace PigeonPortDemo
{
    public partial class Form1 : Form
    {
        private IPigeonPort pigeonPort;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.comboBox1.DataSource = SerialPort.GetPortNames();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            pigeonPort = new PigeonPort(new TopPort(new Communication.Bus.PhysicalPort.SerialPort(comboBox1.Text), new TimeParser(200)), GetRspTypeByRspBytes);
            pigeonPort.OnReceiveResponseData += PigeonPort_OnReceiveResponseData;
            await pigeonPort.StartAsync();
        }

        private async Task PigeonPort_OnReceiveResponseData(Type type, object data)
        {
            if (type == typeof(GetRsp))
            {
                var rsp = data as GetRsp;
                MessageBox.Show("获得对方主动的推送，类型为GetRsp");
            }
            else if (type == typeof(PushMsg))
            {
                var pushMsg = data as PushMsg;
                MessageBox.Show("获得对方主动的推送，类型为PushMsg");
            }
        }

        private Type GetRspTypeByRspBytes(byte[] data)
        {
            if (data[1] == 0) return typeof(GetRsp);
            return typeof(PushMsg);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            GetRsp rsp = null;
            try
            {
                rsp = await pigeonPort.RequestAsync<GetReq, GetRsp>(new GetReq(), 10000);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            if (rsp.Success)
                MessageBox.Show("收到响应:成功");
            else
                MessageBox.Show("收到响应:失败");
        }
    }
}
