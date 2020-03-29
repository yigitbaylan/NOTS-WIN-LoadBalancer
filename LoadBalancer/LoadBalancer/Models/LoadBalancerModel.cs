using LoadBalancer.DataTypes;
using LoadBalancer.Models.BalanceStrategy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text;
using System.Windows.Threading;

namespace LoadBalancer.Models
{
    class LoadBalancerModel
    {
        private TcpListener tcpListener;
        public int Port { get; set; }
        public string StartStopBtn { get; }
        public bool LoadBalancerIsRunning { get; set; }
        public ObservableCollection<LogModel> Logs { get; }
        public ObservableCollection<ServerModel> Servers { get; }
        public ObservableCollection<BalanceModel> Algorithms { get; }
        private Dispatcher Dispatcher;

        public LoadBalancerModel()
        {
            Port = 8080;
            StartStopBtn = "Start";
            LoadBalancerIsRunning = false;
            Logs = new ObservableCollection<LogModel>();
            Servers = new ObservableCollection<ServerModel>();
            Algorithms = new ObservableCollection<BalanceModel>();
            Dispatcher = Dispatcher.CurrentDispatcher;
            CreateLoadBalanceTypes();
            AddLog(LogType.Debug, "Debugging, it Works!");  
        }

        public void AddServer(string host, int port)
        {
            if (IsServerAlreadyExisting(host, port))
                AddLog(LogType.Server, "Server already exist");
            else
            {
                try
                {
                    ServerModel serverModel = new ServerModel(host, port);
                    Servers.Add(serverModel);
                }
                catch (Exception)
                {
                    AddLog(LogType.Server, "Unable to add server");
                }
            }
        }

        private bool IsServerAlreadyExisting(string host, int port)
        {
            bool hostIsAlreadyInList = false;
            bool portIsAlreadyInList = false;
            foreach (ServerModel item in Servers)
            {
                if (item.Host == host)
                    hostIsAlreadyInList = true;
                if (item.Port == port)
                    portIsAlreadyInList = true;
            }
            return (hostIsAlreadyInList && portIsAlreadyInList);
        }

        public void RemoveServer(ServerModel server)
        {
            Servers.Remove(server);
        }

        public void AddLog(string type, string content)
        {
            Logs.Add(new LogModel(type, content));
        }

        public void ClearLogs()
        {
            Logs.Clear();
        }

        private void CreateLoadBalanceTypes()
        {
            string[] balances = new string[] { "Random", "Load", "Round Robin", "Cookie Based", "Session Based" };
            List<Strategy> strategies = new List<Strategy>();
            strategies.Add(new RandomBalanceStrategy());
            strategies.Add(new LoadBalanceStrategy());
            strategies.Add(new RoundRobinBalanceStrategy());
            strategies.Add(new CookieBasedBalanceStrategy());
            strategies.Add(new SessionBasedBalanceStrategy());
            for (int i = 0; i < balances.Length; i++)
            {
                Algorithms.Add(new BalanceModel(balances[i], strategies[i]));
            }
        }

    }
}
