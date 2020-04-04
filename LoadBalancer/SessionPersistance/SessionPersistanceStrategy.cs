using PersistanceStrategy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TCPCommunication;

namespace SessionPersistance
{
    public class SessionPersistanceStrategy : Persistance
    {
        public override Server GetServer(string serverString, ObservableCollection<Server> Servers)
        {
            string host = serverString.Split(":")[0];
            int port = Int32.Parse(serverString.Split(":")[1]);
            List<Server> filteredServers = Servers.Where(server => server.Host == host && server.Port == port).ToList();
            return filteredServers.Count == 0 ? null : filteredServers[0];
        }
    }
}
