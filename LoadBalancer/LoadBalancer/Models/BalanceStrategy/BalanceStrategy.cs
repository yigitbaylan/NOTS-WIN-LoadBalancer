using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LoadBalancer.Models.BalanceStrategy
{
    abstract class Strategy
    {
        public abstract ServerModel GetBalancedServer(ObservableCollection<ServerModel> servers);
    }
}
