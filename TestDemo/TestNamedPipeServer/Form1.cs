using Communication.Bus;
using Communication.Interfaces;
using System.Text;

namespace TestNamedPipeServer
{
    public partial class Form1 : Form
    {
        private IPhysicalPort_Server? server;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            server = new NamedPipeServer("Test");
            server.OnClientConnect += Server_OnClientConnect;
            server.OnClientDisconnect += Server_OnClientDisconnect;
            server.OnReceiveOriginalDataFromClient += Server_OnReceiveOriginalDataFromClient;
            await server.StartAsync();
        }

        private async Task Server_OnReceiveOriginalDataFromClient(byte[] data, int size, Guid clientId)
        {
            _ = Encoding.ASCII.GetString(data, 0, size);
            var buf = new byte[size];
            Array.Copy(data, buf, size);
            await server!.SendDataAsync(clientId, buf);
        }

        private async Task Server_OnClientDisconnect(Guid clientId)
        {
            await Task.CompletedTask;
        }

        private async Task Server_OnClientConnect(Guid clientId)
        {
            await Task.CompletedTask;
        }
    }
}
