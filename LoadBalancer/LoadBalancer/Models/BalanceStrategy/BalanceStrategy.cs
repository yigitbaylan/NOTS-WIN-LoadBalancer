using System;
using System.Collections.Generic;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    abstract class Strategy
    {
        public abstract ServerModel GetBalancedServer(List<ServerModel> servers);
    }
}
