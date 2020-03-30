using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    class RoundRobinBalanceStrategy : Strategy
    {
        private int Position = 0;
        public override ServerModel GetBalancedServer(ObservableCollection<ServerModel> servers)
        {
            if (Position >= servers.Count)
                Position = 0;

            ServerModel server = servers[Position++];
            return server;
        }
    }
}
