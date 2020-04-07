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
        public List<SessionModel> Sessions { get; set; }
        public IStrategy activeBalanceModel { get; set; }
        public PersistanceModel activePersistanceModel { get; set; }
        private int BufferSize = 2048;
        private Dispatcher Dispatcher;
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
            Sessions = new List<SessionModel>();
            CreatePersistances();
            CreateLoadBalanceAlgorithms();
            CreateInitialServers();
        }
        #endregion

        #region Tcp Listener
        /// <summary>
        /// Toggles the loadbalancer on and off
        /// </summary>
        public void ToggleLoadBalancer()
        {
            if (!LoadBalancerIsRunning)
                StartLoadbalancer();
            else
                StopLoadBalancer();
        }
        /// <summary>
        /// Starts the loadbalancer, starts a Timer to check for the health of the servers and begins listening to requests
        /// </summary>
        private void StartLoadbalancer()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, Port);
                LoadBalancerIsRunning = true;
                tcpListener.Start();
                SetTimer();
                Task.Run(() => ListenToRequests());
            }
            catch
            {
                AddLog(LogType.Error, "Couldn't start Load Balancer on port: " + Port);
            }
        }
        /// <summary>
        /// Stops listening for requests and sets the TCP listener to idle.
        /// The timer for health monitoring will stop running
        /// </summary>
        private void StopLoadBalancer()
        {
            StopTimer();
            tcpListener.Stop();
            LoadBalancerIsRunning = false;
            AddLog(LogType.LoadBalancer, "Loadbalancer is idle");
        }

        /// <summary>
        /// Listens to request from a client
        /// </summary>
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

        /// <summary>
        /// Reads the request of a client.
        /// </summary>
        /// <param name="Client">Client with a request</param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Client">The client that from whom the request is</param>
        /// <param name="request">The request object of the client</param>
        private void HandleResponse(Client Client, HttpRequest request)
        {
            Server server = GetServer(request);
            if (server != null)
            {
                byte[] response = SendRequestToServer(request, server);
                if (response.Length > 250)
                    Client.Send(response);
                else
                    Client.Send(HttpResponse.Get503Error().ToByteArray());
            }
            else
                Client.Send(HttpResponse.Get503Error().ToByteArray());
        }

        /// <summary>
        /// Sends the request to the given client
        /// </summary>
        /// <param name="request">Request from the client</param>
        /// <param name="server">Selected Server</param>
        /// <returns>A byte array object. If the loadbalancer is cookie persistance it will return the modified data with a net Set-Cookie object</returns>
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

        /// <summary>
        /// Handles the persistance mode of the loadbalancer. 
        /// The response will be modified if the cookie persistance is activated
        /// If session persistance is activate the sessionID and the server path will be stored in de Sessions list. This is been monitored for validation in the running health monitor timer.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="response"></param>
        private void HandlePersistance(Server server, HttpResponse response)
        {   if (activePersistanceModel.Type != "NONE")
            {
                if (activePersistanceModel.Type == "COOKIE_BASED")
                    response.SetServerCookie(server.Host + ":" + server.Port);
                else if (activePersistanceModel.Type == "SESSION_BASED")
                {
                    string cookie = response.GetSessionCookie();
                    CheckForSession(cookie, server);
                }
            }
        }

        /// <summary>
        /// Checks if there is a sessionID in the cookie from the server response.
        /// If so this session together with the server will be stored in the Sessions list.
        /// </summary>
        /// <param name="cookie">Cookie from the server response</param>
        /// <param name="server">The server from where the response is</param>
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
                    Sessions.Add(new SessionModel(id, serverString, expires));
                }
            }
        }

        /// <summary>
        /// Validates is the sessions in the Sessions list is still valid
        /// If its not valid it will be removed from the Sessions list
        /// </summary>
        private void CheckSessions()
        {
            List<SessionModel> expiredSessions = Sessions.Where(session => session.IsExpired()).ToList();
            foreach (SessionModel expiredSession in expiredSessions)
            {
                Sessions.Remove(expiredSession);
            }
        }
        #endregion

        #region Control Server
        /// <summary>
        /// Creates the initial servers of the loadbalancer
        /// </summary>
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

        /// <summary>
        /// Adds the server if its not already existing.
        /// </summary>
        /// <param name="host">hostname of the server</param>
        /// <param name="port">port of the server</param>
        public void AddServer(string host, int port)
        {
            if (DoesServerExist(host, port))
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

        /// <summary>
        /// Checks if the given server is not already in the pool of servers
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private bool DoesServerExist(string host, int port)
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

        /// <summary>
        ///  Disposes and removes the given server
        /// </summary>
        /// <param name="server">Server model to remove from the pool of servers</param>
        public void RemoveServer(Server server)
        {
            server.Dispose();
            Servers.Remove(server);
        }

        /// <summary>
        /// Finds and gives the Server Model from the given string
        /// </summary>
        /// <param name="serverCookie"></param>
        /// <returns>Server model that belongs to the string, if the server cant be find it will give back the next balanced server</returns>
        private Server GetServerFromString(string serverCookie)
        {
            string host = serverCookie.Split(":")[0];
            int port = Int32.Parse(serverCookie.Split(":")[1]);
            List<Server> filteredServers = Servers.Where(server => server.Host == host && server.Port == port).ToList();
            return filteredServers.Count == 0 ? activeBalanceModel.GetBalancedServer(Servers) : filteredServers[0];
        }

        /// <summary>
        /// Gets a server from the Sessions list with the given sessionID
        /// </summary>
        /// <param name="sessionID">Session ID as string from a request model</param>
        /// <returns></returns>
        private Server GetServerFromSession(string sessionID)
        {
            List<SessionModel> connectedServerSessionList = Sessions.Where(session => session.SessionID == sessionID).ToList();
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
        /// <summary>
        /// Looks for a server 
        /// It first checks the persistance server if not available it will give a server from the active balance model.
        /// </summary>
        /// <param name="request">Request object of a client</param>
        /// <returns></returns>
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

        #region HealthMonitor
        /// <summary>
        /// Makes a call to the server and validates if its alive or not
        /// It will set its property to the right value.
        /// </summary>
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

        #region Log Settings
        /// <summary>
        /// Adds a log object to the list of logs
        /// </summary>
        /// <param name="type">Type of the log</param>
        /// <param name="content">Body content of the log</param>
        public void AddLog(string type, string content)
        {
            Dispatcher.Invoke(() =>
            {
                Logs.Add(new LogModel(type, content));
            });
        }

        /// <summary>
        /// Removes all logs from the list model
        /// </summary>
        public void ClearLogs()
        {
            Logs.Clear();
        }
        #endregion

        #region Algorithms
        /// <summary>
        /// Creates the Load balance algorithms
        /// </summary>
        private void CreateLoadBalanceAlgorithms()
        {
            Algorithms = BalanceStrategyService.GetLoadStrategies();
            setActiveBalanceMethod(Algorithms[0]);
        }
        /// <summary>
        /// Sets the active balance algorithm
        /// </summary>
        /// <param name="balanceModel"></param>
        public void setActiveBalanceMethod(IStrategy balanceModel)
        {
            activeBalanceModel = balanceModel;
            AddLog(LogType.LoadBalancer, "Active Loadbalance method is " + balanceModel.Name);
        }
        /// <summary>
        /// Creates the inital persistance settings
        /// </summary>
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
        /// <summary>
        /// Sets the active persistance setting
        /// </summary>
        /// <param name="persistanceModel"></param>
        public void setActivePersistanceMethod(PersistanceModel persistanceModel)
        {
            activePersistanceModel = persistanceModel;
            AddLog(LogType.LoadBalancer, "Active Loadbalance method is " + persistanceModel.Name);
        }

        #endregion

        #region Timer
        private void SetTimer()
        {
            HealthMonitorTimer = new System.Timers.Timer(800);
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
