/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：Form1.cs
********************************************************************/

using Communication.Bus;
using Communication.Bus.PhysicalPort;
using Communication.Interfaces;
using Parser.Parsers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TopPortLib;
using TopPortLib.Interfaces;

namespace TestNamedPipeClient
{
    public partial class Form1 : Form
    {
        private ITopPort topPort;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
        }
        private async Task TopPort_OnReceiveParsedData(byte[] data)
        {
            var msg = Encoding.ASCII.GetString(data);
            MessageBox.Show(msg);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var msg = $"I am client.";
            await topPort.SendAsync(Encoding.ASCII.GetBytes(msg));
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await topPort.CloseAsync();
            topPort.OnReceiveParsedData -= TopPort_OnReceiveParsedData;
            topPort.Dispose();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            topPort = new TopPort(new NamedPipeClient("Test"), new NoParser());
            topPort.OnReceiveParsedData += TopPort_OnReceiveParsedData;
            await topPort.OpenAsync();
        }
    }
}
