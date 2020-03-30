using LoadBalancer.DataTypes;
using LoadBalancer.Models.BalanceStrategy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace LoadBalancer.Models
{
    class LoadBalancerModel
    {
        private TcpListener tcpListener;
        public int Port { get; set; }
        public string StartStopBtn { get; set; }
        public bool LoadBalancerIsRunning { get; set; }
        public ObservableCollection<LogModel> Logs { get; }
        public ObservableCollection<ServerModel> Servers { get; set; }
        public ObservableCollection<BalanceModel> Algorithms { get; }
        public BalanceModel activeBalanceModel { get; set; }
        private int BufferSize = 1024;
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
            CreateInitialServers();
            AddLog(LogType.Debug, "Debugging, it Works!");  
        }

        public void ToggleLoadBalancer()
        {
            if (LoadBalancerIsRunning)
            {
                tcpListener.Stop();
                LoadBalancerIsRunning = false;
                StartStopBtn = "Start";
                AddLog(LogType.LoadBalancer, "Loadbalancer is been shutdown");
            }
            else
            {
                try
                {
                    tcpListener = new TcpListener(IPAddress.Any, Port);
                    tcpListener.Start();
                    Task.Run(() => ListenToRequests());
                    StartStopBtn = "Stop";
                    LoadBalancerIsRunning = true;
                    AddLog(LogType.LoadBalancer, "Started listening on port: " + Port);
                }
                catch (Exception)
                {
                    AddLog(LogType.Error, "Couldn't start the Loadbalancer on port " + Port + ".");
                }
            }
        }

        private async void ListenToRequests()
        {
            try
            {
                while(LoadBalancerIsRunning)
                {
                    ClientModel clientModel = new ClientModel(await tcpListener.AcceptTcpClientAsync());
                    Task.Run(() => HandleRequest(clientModel));
                }
            }
            catch
            {
                AddLog(LogType.Error, "Failed to listen to requests");
            }
        }

        private async Task HandleRequest(ClientModel ClientModel)
        {
            try
            {
                using (ClientModel)
                {
                    byte[] requestBuffer = await ClientModel.GetRequestAsByteArray(BufferSize);

                    ServerModel server = activeBalanceModel.Strategy.GetBalancedServer(Servers);

                    await server.SendRequest(requestBuffer, BufferSize);
                    byte[] response = await server.GetResponseAsByteArray(BufferSize);
                    Dispatcher.Invoke(() =>
                    {
                        server.incrementRequest();
                        CollectionViewSource.GetDefaultView(Servers).Refresh();
                    });
                    await ClientModel.SendResponse(response, BufferSize);
                }
            }
            catch (Exception)
            {
                AddLog(LogType.Client, "Lost connection with client");
            }
        }

        #region Control Server
        private void CreateInitialServers()
        {
            Dictionary<int,string> servers = new Dictionary<int, string>()
            {
                { 8081, "127.0.0.1" },
                { 8082, "127.0.0.1" },
                { 8083, "127.0.0.1" },
                { 8084, "127.0.0.1" }
            };

            foreach (KeyValuePair<int, string> server in servers)
            {
                AddServer(server.Value, server.Key);
            }
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
                catch (Exception e)
                {
                    AddLog(LogType.Server, "Unable to add server\r\n" + e.Message);
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
            server.Dispose();
            Servers.Remove(server);
        }
        #endregion

        #region Log Settings
        public void AddLog(string type, string content)
        {
            Dispatcher.Invoke(() =>
            {
                Logs.Add(new LogModel(type, content));
            });
        }

        public void ClearLogs()
        {
            Logs.Clear();
        }
        #endregion

        #region Loadbalancer settings
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
            setActiveBalanceMethod(Algorithms[2]);
        }

        public void setActiveBalanceMethod(BalanceModel balanceModel)
        {
            activeBalanceModel = balanceModel;
            AddLog(LogType.LoadBalancer, "Active Loadbalance method is " + balanceModel.Name);
        }
        #endregion
    }
}
