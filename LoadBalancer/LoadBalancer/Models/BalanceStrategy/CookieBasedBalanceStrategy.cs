using System;
using System.Collections.Generic;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    class CookieBasedBalanceStrategy : Strategy
    {
        public override ServerModel GetBalancedServer(List<ServerModel> servers)
        {
            throw new NotImplementedException();
        }
    }
}
