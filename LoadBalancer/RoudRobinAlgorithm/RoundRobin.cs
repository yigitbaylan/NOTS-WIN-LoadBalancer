using BalanceStrategy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TCPCommunication;

namespace RoudRobinAlgorithm
{
    public class RoundRobin : IStrategy
    {
        private int Position = 0;

        public string Name { get; set; }

        public RoundRobin()
        {
            Name = "Round Robin";
        }

        public Server GetBalancedServer(ObservableCollection<Server> servers)
        {
            List<Server> serversAlive = servers.Where(server => server.isAlive == true).ToList();

            if (Position >= serversAlive.Count)
                Position = 0;

            Server server = serversAlive[Position++];
            return server;
        }
    }
}
