using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace TCPCommunication
{
    public class Client : Communication
    {
        public Client(TcpClient client) : base(client)
        {
            Client.SendTimeout = 500;
            Client.ReceiveTimeout = 1000;
        }

        public void CheckForCookies(byte[] request)
        {

        }
    }
}
