using System;
using System.Collections.Generic;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    class SessionBasedBalanceStrategy : Strategy
    {
        public override ServerModel GetBalancedServer(List<ServerModel> servers)
        {
            throw new NotImplementedException();
        }
    }
}
