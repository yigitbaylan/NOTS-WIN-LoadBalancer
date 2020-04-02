using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer.Models
{
    class ClientModel : CommunicationModel
    {

        public ClientModel(TcpClient client) : base(client)
        {
            Client.SendTimeout = 500;
            Client.ReceiveTimeout = 1000;
        }

        public void CheckForCookies(byte[] request)
        {

        }
    }
}
