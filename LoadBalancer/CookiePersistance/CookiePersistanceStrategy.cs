using PersistanceStrategy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TCPCommunication;

namespace CookiePersistance
{
    public class CookiePersistanceStrategy : Persistance
    {
        public override Server GetServer(string serverCookie, ObservableCollection<Server> Servers)
        {
            string host = serverCookie.Split(":")[0];
            int port = Int32.Parse(serverCookie.Split(":")[1]);
            List<Server> filteredServers = Servers.Where(server => server.Host == host && server.Port == port).ToList();
            return filteredServers.Count == 0 ? null : filteredServers[0];
        }
    }
}
