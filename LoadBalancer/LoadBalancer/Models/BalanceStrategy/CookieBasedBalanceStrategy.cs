using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    class CookieBasedBalanceStrategy : Strategy
    {
        public override ServerModel GetBalancedServer(ObservableCollection<ServerModel> servers)
        {
            throw new NotImplementedException();
        }
    }
}
