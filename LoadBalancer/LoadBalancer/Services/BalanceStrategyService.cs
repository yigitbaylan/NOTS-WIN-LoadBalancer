using BalanceStrategy;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace LoadBalancer.Services
{
    class BalanceStrategyService
    {
        private ObservableCollection<IStrategy> _loadBalanceStrategies = new ObservableCollection<IStrategy>();

        public ObservableCollection<IStrategy> GetLoadStrategies()
        {
            string[] files = Directory.GetFiles(@"..\..\..\..\DLL_files", "*.dll"); // Defines the path to look for DLL files
            foreach (var file in files)
            {
                AddFile(file);
            }
            return _loadBalanceStrategies;
        }

        private void AddFile(string file)
        {
            int currentCount = _loadBalanceStrategies.Count();
            Regex regex = new Regex("$(?<=\\.(exe|dll|EXE|DLL))");
            bool isValidFile = regex.IsMatch(file);
            if (isValidFile)
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
                if(currentCount == _loadBalanceStrategies.Count())
                {
                    MessageBox.Show("Unsupported DLL File");
                }
            }
            else
                MessageBox.Show("Please make sure you choose a DLL file");
        }

        public ObservableCollection<IStrategy> AddBalanceDLL()
        {
            var fileContent = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "C:\\Users\\yigit\\Documents\\Persoonlijk\\HAN\\NOTS\\WIN\\NOTS-WIN-LoadBalancer\\LoadBalancer\\DLL_files";
            openFileDialog.Filter = "DLL Bestanden (*.dll)|*.dll|Alle bestanden (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                AddFile(filePath);
            }

            return _loadBalanceStrategies;
        }

        public ObservableCollection<IStrategy> RemoveBalanceStrategy(IStrategy algorithm)
        {
            _loadBalanceStrategies.Remove(algorithm);
            return _loadBalanceStrategies;
        }
    }
}
