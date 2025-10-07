using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Assignment3.Util;
using Assignment3.Shared;

namespace Assignment3.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TcpClient();
            client.Connect(IPAddress.Loopback, 5000);

            Console.WriteLine("Connected to server");

            // Example: send a read request
            var request = new Request
            {
                Method = "read",
                Path = "/api/categories",
                Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                Body = null
            };

            string requestJson = JsonSerializer.Serialize(request) + "\n";

            var stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(requestJson);
            stream.Write(data, 0, data.Length);

            Console.WriteLine("Request sent, waiting for response...");

            string response = client.Read();
            Console.WriteLine("Response:");
            Console.WriteLine(response);
        }
    }
}
