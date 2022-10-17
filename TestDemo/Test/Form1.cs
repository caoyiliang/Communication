using Communication.Bus.PhysicalPort;
using Communication.Interfaces;
using Parser;
using Parser.Parsers;
using System.Text;
using TopPortLib;
using TopPortLib.Interfaces;

namespace Test
{
    public partial class Form1 : Form
    {
        private readonly Dictionary<int, IPhysicalPort> physicalPorts = new();
        private ITopPort parsedPort;
        private readonly byte[] head = new byte[] { 0x7B };
        private readonly byte[] foot = new byte[] { 0x04, 0x06 };
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

        private async void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == -1) return;
            await parsedPort.CloseAsync();
            parsedPort.PhysicalPort = physicalPorts[comboBox2.SelectedIndex];
            await parsedPort.OpenAsync();
            comboBox1.Enabled = true;
        }

        private async void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1) return;
            await parsedPort.CloseAsync();
            if (comboBox1.SelectedIndex == 0)
            {
                parsedPort = new TopPort(physicalPorts[comboBox2.SelectedIndex], new TimeParser(5));
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
                     if (data.Length < 2) return new GetDataLengthRsp() { StateCode = Parser.StateCode.LengthNotEnough };
                     return await Task.FromResult(new GetDataLengthRsp() { Length = data[1], StateCode = Parser.StateCode.Success });
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
