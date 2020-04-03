using System;
using System.Collections.Generic;
using LoadBalancer.Models.BalanceStrategy;
using System.Text;

namespace LoadBalancer.Models
{
    class BalanceModel
    {
        public string Name { get; }
        public T Strategy { get; }

        public BalanceModel(string name, Strategy strategy)
        {
            Name = name;
            Strategy = strategy;
        }
    }
}
