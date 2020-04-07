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
            string[] files = Directory.GetFiles(@".\", "*.dll"); // Defines the path to look for DLL files
            foreach (var file in files)
            {
                Assembly assembly = Assembly.LoadFrom(file);
                foreach (Type type in assembly.GetTypes()) // Gets the list of types, the parents are also included in this list
                {
                    if (type.GetInterfaces().Contains(typeof(IStrategy))) // Checks if the type is a type of IStrategy, the balance interface
                    {
                        IStrategy balanceStrategy = Activator.CreateInstance(type) as IStrategy; // Creates a instance of the object as an IStrategy object.
                        _loadBalanceStrategies.Add(balanceStrategy); // Adds the strategy to the list of strategies.
                    }
                }
            }
            return _loadBalanceStrategies;
        }
    }
}
