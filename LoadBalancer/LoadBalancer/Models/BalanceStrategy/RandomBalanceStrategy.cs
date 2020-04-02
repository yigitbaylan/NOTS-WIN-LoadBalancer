using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    class RandomBalanceStrategy : Strategy
    {
        public override ServerModel GetBalancedServer(ObservableCollection<ServerModel> servers)
        {
            Random rnd = new Random();
            var serversAlive = servers.Where(server => server.isAlive == true);
            return serversAlive.Skip(rnd.Next(serversAlive.Count())).FirstOrDefault(); ;
        }
    }
}
