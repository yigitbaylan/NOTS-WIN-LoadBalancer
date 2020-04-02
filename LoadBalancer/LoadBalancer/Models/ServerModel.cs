using LoadBalancer.Models.HTTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer.Models
{
    class ServerModel : CommunicationModel
    {
        public string Host { get; }
        public int Port { get; }
        public string TimeCreated { get; set; }
        public int RequestHandledCount { get; set; }
        
        public bool isAlive { get; set; }

        public ServerModel(string host, int port) : base()
        {
            Host = host;
            Port = port;
            RequestHandledCount = 0;
            isAlive = true;
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
            HttpResponseModel response = HttpResponseModel.Parse(responseBuffer);
            Disconnect();
            return response.FirstLine.Contains("200 OK");
        }

        public void incrementRequest()
        {
            RequestHandledCount++;
        }
    }
}
