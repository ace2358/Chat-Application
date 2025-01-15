using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TCPChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TCPClient _tcpClient;
        public MainWindow()
        {
            InitializeComponent();
            _tcpClient = new TCPClient();
            MessageBox.ItemsSource = _tcpClient.Messages;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageHere.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                await _tcpClient.SendMessageAsync(message);
                MessageHere.Clear();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string server = "172.20.10.8";
            int port = 5000;

            string name = "YourName";
            await _tcpClient.ConnectAsync(server, port, name);
        }
    }
}