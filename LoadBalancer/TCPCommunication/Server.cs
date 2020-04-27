using HTTP;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TCPCommunication
{
    public class Server : Communication
    {
        public string Host { get; }
        public int Port { get; }
        public string TimeCreated { get; set; }
        public int RequestHandledCount { get; set; }

        public bool isAlive { get; set; }

        public Server(string host, int port) : base()
        {
            Host = host;
            Port = port;
            RequestHandledCount = 0;
            isAlive = false;
            TimeCreated = DateTime.Now.ToString();
        }

        public void Connect()
        {
            Client = new TcpClient(Host, Port);
            NetworkStream = Client.GetStream();
            MemoryStream = new MemoryStream();
        }

        public void Disconnect()
        {
            NetworkStream.Close();
            Client.Close();
            MemoryStream.Dispose();
        }

        public bool IsHealthy()
        {
            Connect();
            string requestString = "GET / HTTP/1.1\r\n" +
                                    "Host: " + Host +
                                    "Connection: close" +
                                    "\r\n\r\n";
            byte[] requestBuffer = Encoding.ASCII.GetBytes(requestString);
            Send(requestBuffer);
            byte[] responseBuffer = Receive(1024);
            HttpResponse response = HttpResponse.Parse(responseBuffer);
            Disconnect();
            return response.FirstLine.Contains("200 OK");
        }

        public void incrementRequest()
        {
            RequestHandledCount++;
        }
    }
}
