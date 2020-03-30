using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer.Models
{
    class ClientModel : IDisposable
    {
        public TcpClient Client { get; }
        private MemoryStream MemoryStream { get; }
        public NetworkStream NetworkStream { get; set; }

        private bool disposedValue = false;

        public ClientModel(TcpClient client)
        {
            Client = client;
            Client.SendTimeout = 5000;
            Client.ReceiveTimeout = 1200;
            NetworkStream = Client.GetStream();
            MemoryStream = new MemoryStream();
        }

        public async Task<byte[]> GetRequestAsByteArray(int BufferSize)
        {
            var buffer = new byte[BufferSize];
            int bytesRead;
            if (NetworkStream.CanRead)
            {
                if (NetworkStream.DataAvailable)
                {
                    do
                    {
                        bytesRead = await NetworkStream.ReadAsync(buffer, 0, buffer.Length);
                        await MemoryStream.WriteAsync(buffer, 0, bytesRead);
                    } while (NetworkStream.DataAvailable);
                }
            }
            return MemoryStream.GetBuffer();
        }

        public async Task SendResponse(byte[] response, int BufferSize)
        {
            int count = 0;
            int currentIndex = response.Length;

            while (currentIndex > 0)
            {
                if (currentIndex - BufferSize <= response.Length)
                    BufferSize = currentIndex;
                await NetworkStream.WriteAsync(response, count, BufferSize);
                count += BufferSize;
                currentIndex -= BufferSize;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    NetworkStream.Dispose();
                    Client.Dispose();
                    MemoryStream.Dispose();
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
