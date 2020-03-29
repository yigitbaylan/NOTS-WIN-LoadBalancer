using System;
using System.Collections.Generic;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    interface IBalanceStrategy
    {
        public ServerModel GetBalancedServer(List<ServerModel> servers);
    }
}
