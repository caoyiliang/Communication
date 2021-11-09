/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：Form1.cs
********************************************************************/

using Communication.Bus;
using Communication.Interfaces;
using System.Text;

namespace TestNamedPipeServer
{
    public partial class Form1 : Form
    {
        private INamedPipeServer server;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            server = new NamedPipeServer("Test");
            server.OnClientConnect += Server_OnClientConnect;
            server.OnClientDisconnect += Server_OnClientDisconnect;
            server.OnReceiveOriginalDataFromTcpClient += Server_OnReceiveOriginalDataFromTcpClient;
            await server.StartAsync();
        }

        private async Task Server_OnReceiveOriginalDataFromTcpClient(byte[] data, int size, int clientId)
        {
            var msg = Encoding.ASCII.GetString(data, 0, size);
            var buf = new byte[size];
            Array.Copy(data, buf, size);
            await server.SendDataAsync(clientId, buf);
        }

        private async Task Server_OnClientDisconnect(int clientId)
        {
        }

        private async Task Server_OnClientConnect(int clientId)
        {
        }
    }
}
