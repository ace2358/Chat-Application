using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPChat;

public class TCPClient
{
    private TcpClient _client;
    private NetworkStream _stream;
    public ObservableCollection<string> Messages { get; private set; }

    public TCPClient()
    {
        Messages = new ObservableCollection<string>();
    }

    public async Task ConnectAsync(string server, int port, string name)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(server, port);
        _stream = _client.GetStream();

        // Send the name to the server
        var nameBuffer = Encoding.UTF8.GetBytes(name);
        await _stream.WriteAsync(nameBuffer, 0, nameBuffer.Length);

        // Start listening for messages
        ReceiveMessages();
    }

    public async Task SendMessageAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await _stream.WriteAsync(buffer, 0, buffer.Length);
    }

    public async Task UploadFileAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            var uploadBuffer = Encoding.UTF8.GetBytes("file_upload");
            await _stream.WriteAsync(uploadBuffer, 0, uploadBuffer.Length);

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(_stream);
            }
        }
        else
        {
            Messages.Add("File not found.");
        }
    }

    private async void ReceiveMessages()
    {
        var buffer = new byte[1024];
        while (true)
        {
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            App.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(message);
            });
        }
    }
}
