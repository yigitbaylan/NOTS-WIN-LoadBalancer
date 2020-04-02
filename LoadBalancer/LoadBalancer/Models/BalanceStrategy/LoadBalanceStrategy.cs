using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    class LoadBalanceStrategy : Strategy
    {
        public override ServerModel GetBalancedServer(ObservableCollection<ServerModel> servers)
        {
            List<ServerModel> serversAlive = servers.Where(server => server.isAlive == true).ToList();
            int lowestRequest = -1;
            ServerModel serverWithLowestLoad = null;
            foreach (ServerModel server in serversAlive)
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
