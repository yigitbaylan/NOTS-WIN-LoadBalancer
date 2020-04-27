using BalanceStrategy;
using LoadBalancer.Helpers;
using LoadBalancer.Models;
using LoadBalancer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TCPCommunication;

namespace LoadBalancer.ViewModels
{
    class LoadBalancerViewModel : NotificationBase<LoadBalancerModel>
    {
        public Command ToggleLoadBalancerCommand { get; }
        public Command ClearLogsCommand { get; }
        public Command AddServerCommand { get; }
        public Command RemoveServerCommand { get; }
        public Command ActivatePersistanceCommand { get; }
        public Command ActivateAlgorithmCommand { get; }
        public Command AddAlgorithmCommand { get; }
        public Command RemoveAlgorithmCommand { get; }

        public LoadBalancerViewModel() : base(null) 
        {
            ToggleLoadBalancerCommand = new Command(ToggleLoadBalancer);
            ClearLogsCommand = new Command(ClearLogs);
            AddServerCommand = new Command(AddServer);
            RemoveServerCommand = new Command(RemoveServer);
            ActivatePersistanceCommand = new Command(SetPersistance);
            ActivateAlgorithmCommand = new Command(SetAlgorithm);
            AddAlgorithmCommand = new Command(AddAlogirthm);
            RemoveAlgorithmCommand = new Command(RemoveAlogirthm);
        }

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
        internal void RemoveAlogirthm(IStrategy algorithm)
        {
            This.DeleteAlgorithm(algorithm);
        }

        public void AddAlogirthm() => This.AddAlgorithm();

        public void AddServer()
        {
            AddServerView inputDialog = new AddServerView();
            string host;
            int port;
            if (inputDialog.ShowDialog() == true)
            {
                host = inputDialog.Host;
                port = int.Parse(inputDialog.Port);
                This.AddServer(host, port);
            }
        }
        public void RemoveServer(Server server) => This.RemoveServer(server);
        public ObservableCollection<LogModel> Logs
        {
            get => This.Logs;
        }
        public void ClearLogs() => This.ClearLogs();

    }
}
