﻿/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：Form1.cs
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
