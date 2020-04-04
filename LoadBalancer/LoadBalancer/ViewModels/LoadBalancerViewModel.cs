using BalanceStrategy;
using LoadBalancer.Helpers;
using LoadBalancer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TCPCommunication;

namespace LoadBalancer.ViewModels
{
    class LoadBalancerViewModel : NotificationBase<LoadBalancerModel>
    {
        public LoadBalancerViewModel() : base(null) { }

        public int Port
        {
            get { return This.Port; }
            set { SetProperty(This.Port, value, () => This.Port = value); }
        }

        public ObservableCollection<Server> Servers
        {
            get  { return This.Servers; }
            set  { SetProperty(This.Servers, value, () => This.Servers = value); }
        }

        internal void ToggleLoadBalancer()
        {
            This.ToggleLoadBalancer();
        }
        public ObservableCollection<PersistanceModel> Persistances
        {
            get => This.Persistances;
        }

        internal void SetPersistance(PersistanceModel persistance)
        {
            This.setActivePersistanceMethod(persistance);
        }

        public ObservableCollection<IStrategy> Algorithms
        {
            get => This.Algorithms;
        }

        internal void SetAlgorithm(IStrategy algorithm)
        {
            This.setActiveBalanceMethod(algorithm);
        }

        public void AddServer(string host, int port) => This.AddServer(host, port);
        public void RemoveServer(Server server) => This.RemoveServer(server);
        public ObservableCollection<LogModel> Logs
        {
            get => This.Logs;
        }
        public void ClearLogs() => This.ClearLogs();
    }
}
