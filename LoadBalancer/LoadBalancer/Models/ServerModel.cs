using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer.Models
{
    class ServerModel : IDisposable
    {
        public string Host { get; }
        public int Port { get; }
        public string TimeCreated { get; set; }
        public NetworkStream NetworkStream { get; }
        public int RequestHandledCount { get; set; }
        private TcpClient tcpClient { get; set; }
        public MemoryStream MemoryStream { get; private set; }

        private bool disposedValue = false;

        public ServerModel(string host, int port)
        {
            Host = host;
            Port = port;
            tcpClient = new TcpClient(Host, Port);
            tcpClient.SendTimeout = 500;
            tcpClient.ReceiveTimeout = 1000;
            NetworkStream = tcpClient.GetStream();
            RequestHandledCount = 0;
            TimeCreated = DateTime.Now.ToString();
        }

        public async Task SendRequest(byte[] request, int BufferSize)
        {
            MemoryStream = new MemoryStream();
            await NetworkStream.WriteAsync(request, 0, request.Length);
        }

        public async Task<byte[]> GetResponseAsByteArray(int BufferSize)
        {
            var buffer = new byte[BufferSize];
            int bytesRead;
            {
                bytesRead = await NetworkStream.ReadAsync(buffer, 0, buffer.Length);
                await MemoryStream.WriteAsync(buffer, 0, bytesRead);
            } while (NetworkStream.DataAvailable) ;
            return MemoryStream.GetBuffer();
        }

        public void incrementRequest()
        {
            RequestHandledCount++;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    tcpClient.Dispose();
                    if (NetworkStream != null || MemoryStream != null)
                    {
                        NetworkStream.Dispose();
                        MemoryStream.Dispose();
                    }
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
