using LoadBalancer.DataTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using System.Windows.Threading;
using TCPCommunication;
using HTTP;
using BalanceStrategy;
using LoadBalancer.Services;

namespace LoadBalancer.Models
{
    class LoadBalancerModel
    {
        #region Variables
        private TcpListener tcpListener;
        public int Port { get; set; }
        public bool LoadBalancerIsRunning { get; set; }
        public ObservableCollection<LogModel> Logs { get; }
        public ObservableCollection<Server> Servers { get; set; }
        public ObservableCollection<IStrategy> Algorithms { get; set; }
        public ObservableCollection<PersistanceModel> Persistances { get; }
        public List<ServerSessionModel> Sessions { get; set; }
        public IStrategy activeBalanceModel { get; set; }
        public PersistanceModel activePersistanceModel { get; set; }
        private int BufferSize = 1024;
        private Dispatcher Dispatcher;
        private Task activeBalancer;
        private System.Timers.Timer HealthMonitorTimer;
        private BalanceStrategyService BalanceStrategyService { get; set; }
        #endregion

        #region Constructor
        public LoadBalancerModel()
        {
            Port = 8080;
            LoadBalancerIsRunning = false;
            Logs = new ObservableCollection<LogModel>();
            Servers = new ObservableCollection<Server>();
            BalanceStrategyService = new BalanceStrategyService();
            Algorithms = new ObservableCollection<IStrategy>();
            Persistances = new ObservableCollection<PersistanceModel>();
            Dispatcher = Dispatcher.CurrentDispatcher;
            Sessions = new List<ServerSessionModel>();
            CreatePersistances();
            CreateLoadBalanceAlgorithms();
            CreateInitialServers();
        }
        #endregion

        #region Tcp Listener
        public void ToggleLoadBalancer()
        {
            if (!LoadBalancerIsRunning)
                StartLoadbalancer();
            else
                StopLoadBalancer();
        }

        private void StartLoadbalancer()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, Port);
                LoadBalancerIsRunning = true;
                tcpListener.Start();
                SetTimer();
                activeBalancer = Task.Run(() => ListenToRequests());
            }
            catch
            {
                AddLog(LogType.Error, "Couldn't start Load Balancer on port: " + Port);
            }
        }

        private void StopLoadBalancer()
        {
            StopTimer();
            tcpListener.Stop();
            LoadBalancerIsRunning = false;
            AddLog(LogType.LoadBalancer, "Loadbalancer is idle");
        }

        private void ListenToRequests()
        {
            try
            {
                AddLog(LogType.LoadBalancer, "Started listening on port: " + Port);
                while (LoadBalancerIsRunning)
                {
                    Client client = new Client(tcpListener.AcceptTcpClient());
                    Task.Run(() => HandleRequest(client));
                }
            }
            catch
            {
                AddLog(LogType.Error, "Failed to listen to requests");
            }
        }

        private void HandleRequest(Client Client)
        {
            try {
                using (Client) {
                    try
                    {
                        byte[] requestBuffer = Client.Receive(BufferSize);
                        HttpRequest request = HttpRequest.Parse(requestBuffer);
                        HandleResponse(Client, request);
                    }
                    catch (Exception) {
                        Client.Send(HttpResponse.Get503Error().ToByteArray());
                    }
                }
            }
            catch (Exception e) {
                AddLog(LogType.Client, "Lost connection with client\r\n" + e.Message);
            }
        }

        private void HandleResponse(Client Client, HttpRequest request)
        {
            Server server = GetServer(request);
            if (server != null)
            {
                byte[] response = SendRequestToServer(request, server);
                if (response.Length > 350)
                    Client.Send(response);
                else
                    Client.Send(HttpResponse.Get503Error().ToByteArray());
            }
            else
                Client.Send(HttpResponse.Get503Error().ToByteArray());
        }

        private byte[] SendRequestToServer(HttpRequest request, Server server)
        {
            server.Connect();
            server.Send(request.ToByteArray());
            byte[] responseBuffer = server.Receive(BufferSize);
            HttpResponse response = HttpResponse.Parse(responseBuffer);
            HandlePersistance(server, response);
            server.Disconnect();
            Dispatcher.Invoke(() =>
            {
                server.incrementRequest();
                CollectionViewSource.GetDefaultView(Servers).Refresh();
            });
            return response.ToByteArray();
        }

        private void HandlePersistance(Server server, HttpResponse response)
        {
            string cookie;
            if (activePersistanceModel.Type != "NONE")
            {
                if (activePersistanceModel.Type == "COOKIE_BASED")
                    cookie = response.SetServerCookie(server.Host + ":" + server.Port);
                else if (activePersistanceModel.Type == "SESSION_BASED")
                {
                    cookie = response.GetSessionCookie();
                    CheckForSession(cookie, server);
                }
            }
        }

        private void CheckForSession(string cookie, Server server)
        {
            if (cookie.Contains("connect.sid"))
            { 
                List<string> sessionIdList = cookie.Split(";").Where(cookie => cookie.Contains("connect.sid")).ToList();
                if(sessionIdList.Count != 0)
                {
                    List<string> expiresList = cookie.Split(";").Where(cookie => cookie.Contains("Expires")).ToList();
                    string serverString = server.Host + ":" + server.Port;
                    string id = sessionIdList[0].Split("=")[1];
                    string expires = expiresList.Count != 0 ? expiresList[0].Split("=")[1] : "Wed, 01 Apr 1990 09:22:42 GMT";
                    Sessions.Add(new ServerSessionModel(id, serverString, expires));
                }
            }
        }

        private void CheckSessions()
        {
            List<ServerSessionModel> expiredSessions = Sessions.Where(session => session.IsExpired()).ToList();
            foreach (ServerSessionModel expiredSession in expiredSessions)
            {
                Sessions.Remove(expiredSession);
            }
        }
        #endregion

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
                    Server serverModel = new Server(host, port);
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
            foreach (Server item in Servers)
            {
                if (item.Host == host)
                    hostIsAlreadyInList = true;
                if (item.Port == port)
                    portIsAlreadyInList = true;
            }
            return (hostIsAlreadyInList && portIsAlreadyInList);
        }

        public void RemoveServer(Server server)
        {
            server.Dispose();
            Servers.Remove(server);
        }

        private Server GetServerFromString(string serverCookie)
        {
            string host = serverCookie.Split(":")[0];
            int port = Int32.Parse(serverCookie.Split(":")[1]);
            List<Server> filteredServers = Servers.Where(server => server.Host == host && server.Port == port).ToList();
            return filteredServers.Count == 0 ? activeBalanceModel.GetBalancedServer(Servers) : filteredServers[0];
        }
        private Server GetServerFromSession(string sessionID)
        {
            List<ServerSessionModel> connectedServerSessionList = Sessions.Where(session => session.SessionID == sessionID).ToList();
            if (connectedServerSessionList.Count == 0)
                return Algorithms[0].GetBalancedServer(Servers);
            else
            {
                string serverString = connectedServerSessionList[0].Server;
                string host = serverString.Split(":")[0];
                int port = Int32.Parse(serverString.Split(":")[1]);
                return GetServerFromString(host + ":" + port);
            }
        }

        private Server GetServer(HttpRequest request)
        {
            if (activePersistanceModel.Type == "COOKIE_BASED")
            {
                string cookie = request.GetCookie();
                return cookie != "NO_COOKIE" ? GetServerFromString(cookie) : activeBalanceModel.GetBalancedServer(Servers);
            }
            else if (activePersistanceModel.Type == "SESSION_BASED")
            {
                string sessionID = request.GetServerSessionID();
                return sessionID != "NO_SESSION" ? GetServerFromSession(sessionID) : activeBalanceModel.GetBalancedServer(Servers);
            }
            else
                return activeBalanceModel.GetBalancedServer(Servers);
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

        public void CheckServersHealth()
        {
            foreach (var server in Servers)
            {
                try
                {
                    if (server.IsHealthy())
                    {
                        Dispatcher.Invoke(() =>
                        {
                            server.isAlive = true;
                            CollectionViewSource.GetDefaultView(Servers).Refresh();
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            server.isAlive = false;
                            CollectionViewSource.GetDefaultView(Servers).Refresh();
                        });
                    }
                }
                catch (Exception)
                {
                    Dispatcher.Invoke(() =>
                    {
                        server.isAlive = false;
                        CollectionViewSource.GetDefaultView(Servers).Refresh();
                    });
                    AddLog(LogType.Server, "Server " + server.Host + "on port " + server.Port + " is unavailable");
                }

            } 
        }
        #endregion

        #region Algorithms
        private void CreateLoadBalanceAlgorithms()
        {
            Algorithms = BalanceStrategyService.GetLoadStrategies();
            setActiveBalanceMethod(Algorithms[0]);
        }
        public void setActiveBalanceMethod(IStrategy balanceModel)
        {
            activeBalanceModel = balanceModel;
            AddLog(LogType.LoadBalancer, "Active Loadbalance method is " + balanceModel.Name);
        }

        private void CreatePersistances()
        {
            string[] names = new string[] { "None", "Cookie Based", "Session Based" }; 
            string[] types= new string[] { "NONE", "COOKIE_BASED", "SESSION_BASED" };
            for (int i = 0; i < names.Length; i++)
            {
                Persistances.Add(new PersistanceModel(names[i], types[i]));
            }
            setActivePersistanceMethod(Persistances[0]);
        }
        public void setActivePersistanceMethod(PersistanceModel persistanceModel)
        {
            activePersistanceModel = persistanceModel;
            AddLog(LogType.LoadBalancer, "Active Loadbalance method is " + persistanceModel.Name);
        }

        #endregion

        #region Timer
        private void SetTimer()
        {
            HealthMonitorTimer = new System.Timers.Timer(2000);
            HealthMonitorTimer.Elapsed += OnTimedEvent;
            HealthMonitorTimer.AutoReset = true;
            HealthMonitorTimer.Enabled = true;
        }

        public void ResetTimer(int milliseconds)
        {
            StopTimer();
            HealthMonitorTimer = new System.Timers.Timer(milliseconds);
            HealthMonitorTimer.Elapsed += OnTimedEvent;
            HealthMonitorTimer.AutoReset = true;
            HealthMonitorTimer.Enabled = true;
        }

        private void StopTimer()
        {
            // Create a timer with a two second interval.
            HealthMonitorTimer.Enabled = false;
            HealthMonitorTimer.Stop();
            HealthMonitorTimer = null;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Task.Run(() => CheckServersHealth());
            CheckSessions();
        }
        #endregion
    }
}
