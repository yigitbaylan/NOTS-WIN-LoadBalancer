using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    class RoundRobinBalanceStrategy : Strategy
    {
        private int Position = 0;
        public override ServerModel GetBalancedServer(ObservableCollection<ServerModel> servers)
        {
            List<ServerModel> serversAlive = servers.Where(server => server.isAlive == true).ToList();

            if (Position >= serversAlive.Count)
                Position = 0;

            ServerModel server = serversAlive[Position++];
            return server;
        }
    }
}
