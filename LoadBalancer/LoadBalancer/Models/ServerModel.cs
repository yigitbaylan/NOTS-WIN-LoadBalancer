using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace LoadBalancer.Models
{
    class ServerModel
    {
        public string Host { get; }
        public int Port { get; }
        public string TimeCreated { get; set; }
        private TcpClient tcpClient { get; set; }
        public ServerModel(string host, int port)
        {
            Host = host;
            Port = port;
            TimeCreated = DateTime.Now.ToString();
        }
    }
}
