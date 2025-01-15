using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// Console-based Server with Logging
class ChatServer
{
    private static TcpListener server;
    private static List<TcpClient> clients = new List<TcpClient>();
    private static readonly object lockObj = new object();

    static void Main()
    {
        server = new TcpListener(IPAddress.Any, 12345);
        server.Start();
        Console.WriteLine("Server started on port 12345...");

        Thread acceptThread = new Thread(AcceptClients);
        acceptThread.Start();
    }

    static void AcceptClients()
    {
        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            lock (lockObj)
            {
                clients.Add(client);
            }

            Console.WriteLine("New client connected!");
            LogMessage("New client connected.");

            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                LogMessage($"Received: {message}");
                BroadcastMessage(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            LogMessage("Error: " + ex.Message);
        }
        finally
        {
            lock (lockObj)
            {
                clients.Remove(client);
            }
            client.Close();
            Console.WriteLine("Client disconnected.");
            LogMessage("Client disconnected.");
        }
    }

    static void BroadcastMessage(string message)
    {
        lock (lockObj)
        {
            foreach (var client in clients)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                    stream.Write(messageBytes, 0, messageBytes.Length);
                }
                catch
                {
                    Console.WriteLine("Failed to send message to a client.");
                }
            }
        }
    }

    static void LogMessage(string message)
    {
        string logEntry = $"[{DateTime.Now}] {message}";
        Console.WriteLine(logEntry);
        // Optionally log to a file:
        System.IO.File.AppendAllText("server_log.txt", logEntry + Environment.NewLine);
    }
}