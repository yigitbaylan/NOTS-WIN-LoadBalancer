using LoadBalancer.Helpers;
using LoadBalancer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

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
        public string StartStopBtn
        {
            get { return This.StartStopBtn; }
        }
        public ObservableCollection<ServerModel> Servers
        {
            get  { return This.Servers; }
            set  { SetProperty(This.Servers, value, () => This.Servers = value); }
        }

        internal void ToggleLoadBalancer()
        {
            This.ToggleLoadBalancer();
        }

        internal void SetAlgorithm(BalanceModel algorithm)
        {
            This.setActiveBalanceMethod(algorithm);
        }

        public void AddServer(string host, int port) => This.AddServer(host, port);
        public void RemoveServer(ServerModel server) => This.RemoveServer(server);
        public ObservableCollection<LogModel> Logs
        {
            get => This.Logs;
        }
        public void ClearLogs() => This.ClearLogs();

        public ObservableCollection<BalanceModel> Algorithms
        {
            get => This.Algorithms;
        }
    }
}
