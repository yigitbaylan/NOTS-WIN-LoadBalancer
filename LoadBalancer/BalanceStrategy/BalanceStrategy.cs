using System;
using System.Collections.ObjectModel;
using TCPCommunication;

namespace BalanceStrategy
{
    public interface IStrategy
    {
        public string Name { get; set; }
        public Server GetBalancedServer(ObservableCollection<Server> servers);
    }
}
