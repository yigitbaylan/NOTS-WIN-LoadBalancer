using System;
using System.Collections.Generic;
using System.Text;
using BalanceStrategy;

namespace LoadBalancer.Models
{
    class BalanceModel
    {
        public string Name { get; }
        public IStrategy Strategy { get; }

        public BalanceModel(string name, IStrategy strategy)
        {
            Name = name;
            Strategy = strategy;
        }
    }
}
