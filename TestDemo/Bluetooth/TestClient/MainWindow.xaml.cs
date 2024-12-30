using Communication.Bluetooth;
using InTheHand.Net.Sockets;
using System.Text;
using System.Windows;
using TopPortLib;
using TopPortLib.Interfaces;

namespace TestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BluetoothClassic? _bluetoothClassic;
        ITopPort? topPort;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lb.Items.Clear();
            _ = Task.Run(async () =>
            {
                using var bluetoothClient = new BluetoothClient();
                await foreach (var item in bluetoothClient.DiscoverDevicesAsync())
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        lb.Items.Add($"{item.DeviceName}$${item.DeviceAddress}");
                    });
                }
            });
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (lb.SelectedIndex == -1) return;
            _bluetoothClassic = new BluetoothClassic(lb.SelectedItem.ToString()!.Split("$$")[1]);
            topPort = new TopPort(_bluetoothClassic, new Parser.Parsers.TimeParser());
            topPort.OnReceiveParsedData += TopPort_OnReceiveParsedData;
            await topPort.OpenAsync();
            MessageBox.Show("ok");
        }

        private async Task TopPort_OnReceiveParsedData(byte[] data)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                r.Text += System.Text.Encoding.UTF8.GetString(data);
            });
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await topPort!.SendAsync(Encoding.UTF8.GetBytes(s.Text));
        }
    }
}