using Communication.Bus;
using Communication.Interfaces;

namespace TestTcpServer
{
    public partial class Form1 : Form
    {
        IPhysicalPort_Server? tcpServer;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            tcpServer = new TcpServer("127.0.0.1", 7779);
            await tcpServer.StartAsync();
            tcpServer.OnReceiveOriginalDataFromClient += TcpServer_ReceiveOriginalDataFromTcpClient;
            tcpServer.OnClientConnect += TcpServer_ClientConnect;
            tcpServer.OnClientDisconnect += TcpServer_ClientDisconnect;
        }

        private async Task TcpServer_ClientDisconnect(int clientId)
        {
            var info = await ((TcpServer)tcpServer!).GetClientInfo(clientId);
            await Task.CompletedTask;
        }

        private async Task TcpServer_ClientConnect(int clientId)
        {
            var info = await ((TcpServer)tcpServer!).GetClientInfo(clientId);

            if (info.HasValue)
            {

                var clients = await ((TcpServer)tcpServer!).GetClientsByIp(info.Value.IPAddress);

                await ((TcpServer)tcpServer).GetClientId(info.Value.IPAddress, info.Value.Port);
            }
        }

        private async Task TcpServer_ReceiveOriginalDataFromTcpClient(byte[] data, int size, int clientId)
        {
            var tmp = new byte[size];
            Array.Copy(data, 0, tmp, 0, size);
            await tcpServer!.SendDataAsync(clientId, tmp);
            if (data[0] == 0x89)
            {
                await tcpServer.DisconnectClientAsync(clientId);
            }
        }
    }
}
