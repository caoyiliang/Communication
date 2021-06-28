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

using Communication.Bus;
using Communication.Bus.PhysicalPort;
using Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestTcpServer
{
    public partial class Form1 : Form
    {
        ITcpServer tcpServer;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            tcpServer = new TcpServer("127.0.0.1", 7779);
            await tcpServer.StartAsync();
            tcpServer.OnReceiveOriginalDataFromTcpClient += TcpServer_ReceiveOriginalDataFromTcpClient;
            tcpServer.OnClientConnect += TcpServer_ClientConnect;
            tcpServer.OnClientDisconnect += TcpServer_ClientDisconnect;
        }

        private async Task TcpServer_ClientDisconnect(int clientId)
        {
        }

        private async Task TcpServer_ClientConnect(string hostName, int port, int clientId)
        {
        }

        private async Task TcpServer_ReceiveOriginalDataFromTcpClient(byte[] data, int size, int clientId)
        {
            var tmp = new byte[size];
            Array.Copy(data,0,tmp,0,size);
            await tcpServer.SendDataAsync(clientId, tmp);
            if (data[0] == 0x89)
            {
                await tcpServer.DisconnectClientAsync(clientId);
            }
        }
    }
}
