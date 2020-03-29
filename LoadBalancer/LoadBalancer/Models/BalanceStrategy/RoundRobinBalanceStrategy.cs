using System;
using System.Collections.Generic;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    class RoundRobinBalanceStrategy : Strategy
    {
        private int Position = 0;
        public override ServerModel GetBalancedServer(List<ServerModel> servers)
        {
            if (Position >= servers.Count)
                Position = 0;

            ServerModel server = servers[Position];
            Position++;
            return server;
        }
    }
}
