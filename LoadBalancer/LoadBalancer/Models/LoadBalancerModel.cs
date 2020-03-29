using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows.Threading;

namespace LoadBalancer.Models
{
    class LoadBalancerModel
    {
        private TcpListener tcpListener;
        public int Port { get; set; }
        public bool LoadBalancerIsRunning { get; set; }
        private Dispatcher Dispatcher;

        public LoadBalancerModel()
        {
            Port = 8080;
            LoadBalancerIsRunning = false;
            Dispatcher = Dispatcher.CurrentDispatcher;
        }


    }
}
