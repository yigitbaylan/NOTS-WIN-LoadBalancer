using System;
using System.Collections.ObjectModel;
using TCPCommunication;

namespace PersistanceStrategy
{
    public abstract class Persistance
    {
        public abstract Server GetServer(string value, ObservableCollection<Server> Servers);
    }
}
