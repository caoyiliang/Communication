using Communication.Bus.PhysicalPort;
using Parser.Parsers;
using System.Text;
using TopPortLib;
using TopPortLib.Interfaces;

namespace TestNamedPipeClient
{
    public partial class Form1 : Form
    {
        private ITopPort? topPort;
        public Form1()
        {
            InitializeComponent();
        }

        private async Task TopPort_OnReceiveParsedData(byte[] data)
        {
            var msg = Encoding.ASCII.GetString(data);
            MessageBox.Show(msg);
            await Task.CompletedTask;
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            if (topPort is null) return;
            var msg = $"I am client.";
            await topPort.SendAsync(Encoding.ASCII.GetBytes(msg));
        }

        private async void Button2_Click(object sender, EventArgs e)
        {
            if (topPort is null) return;
            await topPort.CloseAsync();
            topPort.OnReceiveParsedData -= TopPort_OnReceiveParsedData;
            topPort.Dispose();
        }

        private async void Button3_Click(object sender, EventArgs e)
        {
            topPort = new TopPort(new NamedPipeClient("Test"), new NoParser());
            topPort.OnReceiveParsedData += TopPort_OnReceiveParsedData;
            await topPort.OpenAsync();
        }
    }
}
