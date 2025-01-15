using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;

class Program
{
    private static TcpListener listener;
    private static Dictionary<TcpClient, string> clients = new Dictionary<TcpClient, string>();
    private static string logFilePath = "server_log.txt";

    static async Task Main(string[] args)
    {
        int port = 5000;

        // Start the TCP Listener on the chosen IP and port
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        LogMessage($"Server started on port {port}");

        Directory.CreateDirectory("Files"); // Ensure the "Files" directory exists

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClient(client));
        }
    }

    static async Task HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        var buffer = new byte[1024];

        // Read the client's name (first message)
        int nameBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
        string clientName = Encoding.UTF8.GetString(buffer, 0, nameBytes).Trim();
        clients[client] = clientName;

        LogMessage($"{clientName} connected.");

        while (true)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                if (message == "file_upload")
                {
                    LogMessage($"{clientName} initiated file upload.");
                    string fileName = $"Files/{DateTime.Now:yyyyMMddHHmmss}.txt";
                    using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        LogMessage("Receiving file...");
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            if (bytesRead < buffer.Length) break;
                        }
                    }
                    LogMessage($"File received and saved as {fileName}");
                    continue;
                }

                LogMessage($"Received from {clientName}: {message}");

                foreach (var c in clients.Keys)
                {
                    if (c != client)
                    {
                        var writer = c.GetStream();
                        await writer.WriteAsync(buffer, 0, bytesRead);
                    }
                }
            }
            catch
            {
                LogMessage($"{clientName} disconnected.");
                clients.Remove(client);
                break;
            }
        }
    }

    static void LogMessage(string message)
    {
        Console.WriteLine(message);
        using (var writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }
    }
}
