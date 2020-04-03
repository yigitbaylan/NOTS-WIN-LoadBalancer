using LoadBalancer.DataTypes;
using LoadBalancer.Models.BalanceStrategy;
using LoadBalancer.Models.HTTP;
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

namespace LoadBalancer.Models
{
    class LoadBalancerModel
    {
        #region Variables
        private TcpListener tcpListener;
        public int Port { get; set; }
        public string StartStopBtn { get; set; }
        public bool LoadBalancerIsRunning { get; set; }
        public ObservableCollection<LogModel> Logs { get; }
        public ObservableCollection<ServerModel> Servers { get; set; }
        public ObservableCollection<BalanceModel> Algorithms { get; }
        public List<ServerSessionModel> Sessions { get; set; }
        public BalanceModel activeBalanceModel { get; set; }
        private int BufferSize = 1024;
        private Dispatcher Dispatcher;
        private System.Timers.Timer HealthMonitorTimer;
        #endregion

        #region Constructor
        public LoadBalancerModel()
        {
            Port = 8080;
            StartStopBtn = "Start";
            LoadBalancerIsRunning = false;
            Logs = new ObservableCollection<LogModel>();
            Servers = new ObservableCollection<ServerModel>();
            Algorithms = new ObservableCollection<BalanceModel>();
            Dispatcher = Dispatcher.CurrentDispatcher;
            Sessions = new List<ServerSessionModel>();
            CreateLoadBalanceTypes();
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
                StartStopBtn = "Stop";
                LoadBalancerIsRunning = true;
                tcpListener.Start();
                SetTimer();
                Task.Run(() => ListenToRequests());
            }
            catch (Exception)
            {
                AddLog(LogType.Error, "Couldn't start the Loadbalancer on port " + Port + ".");
            }
        }

        private void StopLoadBalancer()
        {
            StopTimer();
            tcpListener.Stop();
            LoadBalancerIsRunning = false;
            StartStopBtn = "Start";
            AddLog(LogType.LoadBalancer, "Loadbalancer is idle");
        }

        private void ListenToRequests()
        {
            try
            {
                AddLog(LogType.LoadBalancer, "Started listening on port: " + Port);
                while (LoadBalancerIsRunning)
                {
                    ClientModel client = new ClientModel(tcpListener.AcceptTcpClient());
                    HandleRequest(client);
                }
            }
            catch
            {
                AddLog(LogType.Error, "Failed to listen to requests");
            }
        }

        private void HandleRequest(ClientModel Client)
        {
            try {
                using (Client) {
                    try
                    {
                        byte[] requestBuffer = Client.Receive(BufferSize);
                        HttpRequestModel request = HttpRequestModel.Parse(requestBuffer);
                        HandleResponse(Client, request);
                    }
                    catch (Exception) {
                        Client.Send(HttpResponseModel.Get503Error().ToByteArray());
                    }
                }
            }
            catch (Exception e) {
                AddLog(LogType.Client, "Lost connection with client\r\n" + e.Message);
            }
        }

        private void HandleResponse(ClientModel Client, HttpRequestModel request)
        {
            ServerModel server = GetServer(request);
            if (server != null)
            {
                byte[] response = SendRequestToServer(request, server);
                if (response.Length > 250)
                    Client.Send(response);
                else
                    Client.Send(HttpResponseModel.Get503Error().ToByteArray());
            }
            else
                Client.Send(HttpResponseModel.Get503Error().ToByteArray());
        }

        private byte[] SendRequestToServer(HttpRequestModel request, ServerModel server)
        {
            server.Connect();
            server.Send(request.ToByteArray());
            byte[] responseBuffer = server.Receive(BufferSize);
            HttpResponseModel response = HttpResponseModel.Parse(responseBuffer);
            string cookie = activeBalanceModel.Name == "COOKIE_BASED" ? response.SetServerCookie(server.Host + ":" + server.Port) : response.GetSessionCookie();
            CheckForSession(cookie, server);
            server.Disconnect();
            Dispatcher.Invoke(() =>
            {
                server.incrementRequest();
                CollectionViewSource.GetDefaultView(Servers).Refresh();
            });
            return response.ToByteArray();
        }

        private void CheckForSession(string cookie, ServerModel server)
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

        private ServerModel GetServerFromCookie(string serverCookie)
        {
            string host = serverCookie.Split(":")[0];
            int port = Int32.Parse(serverCookie.Split(":")[1]);
            List<ServerModel> filteredServers = Servers.Where(server => server.Host == host && server.Port == port).ToList();
            return filteredServers.Count == 0 ? Algorithms[0].Strategy.GetBalancedServer(Servers) : filteredServers[0];
        }
        private ServerModel GetServer(HttpRequestModel request)
        {
            if (activeBalanceModel.Name == "COOKIE_BASED")
            {
                string cookie = request.GetCookie();
                return cookie != "NO_COOKIE" ? GetServerFromCookie(cookie) : Algorithms[0].Strategy.GetBalancedServer(Servers);
            }
            else if (activeBalanceModel.Name == "SESSION_BASED")
            {
                string sessionID = request.connectedServerList();
                return sessionID != "NO_SESSION" ? GetServerFromSession(sessionID) : Algorithms[0].Strategy.GetBalancedServer(Servers);
            }
            else
                return activeBalanceModel.Strategy.GetBalancedServer(Servers);
        }

        private ServerModel GetServerFromSession(string sessionID)
        {
            List<ServerSessionModel> connectedServerSessionList = Sessions.Where(session => session.SessionID == sessionID).ToList();
            if (connectedServerSessionList.Count == 0)
                return Algorithms[0].Strategy.GetBalancedServer(Servers);
            else
            {
                string serverString = connectedServerSessionList[0].Server;
                string host = serverString.Split(":")[0];
                int port = Int32.Parse(serverString.Split(":")[1]);
                List<ServerModel> filteredServers = Servers.Where(server => server.Host == host && server.Port == port).ToList();
                return filteredServers.Count == 0 ? Algorithms[0].Strategy.GetBalancedServer(Servers) : filteredServers[0];
            }
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
        private void CreateLoadBalanceTypes()
        {
            string[] balances = new string[] { "Random", "Load", "Round Robin", "COOKIE_BASED", "SESSION_BASED" };
            List<Strategy> strategies = new List<Strategy>();
            strategies.Add(new RandomBalanceStrategy());
            strategies.Add(new LoadBalanceStrategy());
            strategies.Add(new RoundRobinBalanceStrategy());
            strategies.Add(null);
            strategies.Add(null);
            for (int i = 0; i < balances.Length; i++)
            {
                Algorithms.Add(new BalanceModel(balances[i], strategies[i]));
            }
            setActiveBalanceMethod(Algorithms[0]);
        }

        public void setActiveBalanceMethod(BalanceModel balanceModel)
        {
            activeBalanceModel = balanceModel;
            AddLog(LogType.LoadBalancer, "Active Loadbalance method is " + balanceModel.Name);
        }
        #endregion

        #region Timer
        private void SetTimer()
        {
            HealthMonitorTimer = new System.Timers.Timer(500);
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
            CheckServersHealth();
            CheckSessions();
        }
        #endregion
    }
}
