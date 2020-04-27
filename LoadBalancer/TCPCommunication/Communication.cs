using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TCPCommunication
{
    public class Communication : IDisposable
    {
        protected TcpClient Client { get; set; }
        protected MemoryStream MemoryStream { get; set; }
        protected NetworkStream NetworkStream { get; set; }

        private bool disposedValue = false;
        public Communication(TcpClient client = null)
        {
            if (client != null)
            {
                Client = client;
                Client.SendTimeout = 5000;
                Client.ReceiveTimeout = 1200;
                NetworkStream = Client.GetStream();
            }
            MemoryStream = new MemoryStream();
        }
        public byte[] Receive(int BufferSize)
        {
            var buffer = new byte[BufferSize];
            int bytesRead;
            if (NetworkStream.CanRead)
            {
                do
                {
                    bytesRead = NetworkStream.Read(buffer, 0, buffer.Length);
                    MemoryStream.Write(buffer, 0, bytesRead);
                } while (NetworkStream.DataAvailable);

            }
            return MemoryStream.GetBuffer();
        }

        public void Send(byte[] response)
        {
            NetworkStream.Write(response, 0, response.Length);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        NetworkStream.Dispose();
                        Client.Dispose();
                        MemoryStream.Dispose();
                    }
                    catch
                    {}
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
