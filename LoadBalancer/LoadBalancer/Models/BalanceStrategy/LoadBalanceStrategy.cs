using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    class LoadBalanceStrategy : Strategy
    {
        public override ServerModel GetBalancedServer(ObservableCollection<ServerModel> servers)
        {
            int lowestLoad = -1;
            int highestLoad = 0;
            ServerModel serverWithLowestLoad = null;
            ServerModel serverWithHighestLoad = null;
            foreach (ServerModel server in servers)
            {
                if (server.RequestHandledCount >= highestLoad)
                {
                    highestLoad = server.RequestHandledCount;
                    serverWithHighestLoad = server;
                }
                if (server.RequestHandledCount <= lowestLoad || lowestLoad == -1)
                {
                    lowestLoad = server.RequestHandledCount;
                    serverWithLowestLoad = server;
                }
            }
            if (serverWithLowestLoad != null)
                return serverWithLowestLoad;
            
            return serverWithHighestLoad;
        }
    }
}
