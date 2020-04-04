using BalanceStrategy;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TCPCommunication;

namespace RandomBalanceAlgorithm
{
    public class RandomBalance : IStrategy
    {
        public string Name { get; set; }

        public RandomBalance()
        {
            Name = "Random";
        }

        public Server GetBalancedServer(ObservableCollection<Server> servers)
        {
            Random rnd = new Random();
            var serversAlive = servers.Where(server => server.isAlive == true);
            return serversAlive.Skip(rnd.Next(serversAlive.Count())).FirstOrDefault(); ;
        }
    }
}
