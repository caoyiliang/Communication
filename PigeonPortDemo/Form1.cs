using System.IO.Ports;

namespace PigeonPortDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.comboBox1.DataSource = SerialPort.GetPortNames();
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            PigeonPortProtocolDemo.IPigeonPortProtocolDemo pigeonPortProtocolDemo = new PigeonPortProtocolDemo.PigeonPortProtocolDemo(new Communication.Bus.PhysicalPort.SerialPort(comboBox1.Text));
            await pigeonPortProtocolDemo.OpenAsync();
        }
    }
}
