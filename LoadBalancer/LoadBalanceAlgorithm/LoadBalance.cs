using BalanceStrategy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TCPCommunication;

namespace LoadBalanceAlgorithm
{
    public class LoadBalance : IStrategy
    {

        public string Name { get; set; }

        public LoadBalance()
        {
            Name = "Load";
        }

        public Server GetBalancedServer(ObservableCollection<Server> servers)
        {
            List<Server> serversAlive = servers.Where(server => server.isAlive == true).ToList();
            int lowestRequest = -1;
            Server serverWithLowestLoad = null;
            foreach (Server server in serversAlive)
            {
                if (server.RequestHandledCount < lowestRequest || lowestRequest == -1)
                {
                    lowestRequest = server.RequestHandledCount;
                    serverWithLowestLoad = server;
                }
            }

            return serverWithLowestLoad;
        }
    }
}
