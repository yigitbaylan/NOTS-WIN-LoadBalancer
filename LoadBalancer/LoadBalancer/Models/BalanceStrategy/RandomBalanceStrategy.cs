using System;
using System.Collections.Generic;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    class RandomBalanceStrategy : Strategy
    {
        public override ServerModel GetBalancedServer(List<ServerModel> servers)
        {
            Random rnd = new Random();
            ServerModel server = servers[rnd.Next(0, servers.Count)];
            return server;
        }
    }
}
