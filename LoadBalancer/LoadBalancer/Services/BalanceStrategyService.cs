using BalanceStrategy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LoadBalancer.Services
{
    class BalanceStrategyService
    {
        private ObservableCollection<IStrategy> _loadBalanceStrategies = new ObservableCollection<IStrategy>();

        public ObservableCollection<IStrategy> GetLoadStrategies()
        {
            string[] files = Directory.GetFiles(@".\", "*.dll");
            foreach (var file in files)
            {
                Assembly assembly = Assembly.LoadFrom(file);
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetInterfaces().Contains(typeof(IStrategy)))
                    {
                        IStrategy balanceStrategy = Activator.CreateInstance(type) as IStrategy;
                        _loadBalanceStrategies.Add(balanceStrategy);
                    }
                }
            }
            return _loadBalanceStrategies;
        }
    }
}
