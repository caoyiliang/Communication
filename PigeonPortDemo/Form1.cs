using Parser.Parsers;
using System.IO.Ports;
using TopPortLib;
using TopPortLib.Interfaces;

namespace PigeonPortDemo
{
    public partial class Form1 : Form
    {
        private IPigeonPort? pigeonPort;
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
            pigeonPort = new PigeonPort(new TopPort(new Communication.Bus.PhysicalPort.SerialPort(comboBox1.Text), new TimeParser(200)), GetRspTypeByRspBytes);
            pigeonPort.OnReceiveActivelyPushData += PigeonPort_OnReceiveActivelyPushData; ;
            await pigeonPort.StartAsync();
        }

        private async Task PigeonPort_OnReceiveActivelyPushData(Type type, object data)
        {
            if (type == typeof(PushMsg))
            {
                var pushMsg = data as PushMsg;
                MessageBox.Show("获得对方主动的推送，类型为PushMsg");
            }
            await Task.CompletedTask;
        }

        private Type GetRspTypeByRspBytes(byte[] data)
        {
            if (data[1] == 0) return typeof(GetRsp);
            return typeof(PushMsg);
        }

        private async void Button2_Click(object sender, EventArgs e)
        {
            if (pigeonPort is null) return;
            GetRsp? rsp;
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
