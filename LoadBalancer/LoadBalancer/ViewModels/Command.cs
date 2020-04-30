using BalanceStrategy;
using LoadBalancer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using TCPCommunication;

namespace LoadBalancer.ViewModels
{
    class Command : ICommand
    {
        #pragma warning disable 67
        public event EventHandler CanExecuteChanged;
        #pragma warning restore 67
        private readonly Action _execute = null;
        private readonly Action<IStrategy> _executeWithParam = null;
        private readonly Action<PersistanceModel> _executeWithPersistanceParam = null;
        private readonly Action<Server> _executeWithServerParam = null;

        public Command(Action execute)
        {
            _execute = execute;
        }
        public Command(Action<IStrategy> execute)
        {
            _executeWithParam = execute;
        }

        public Command(Action<PersistanceModel> execute)
        {
            _executeWithPersistanceParam = execute;
        }
        public Command(Action<Server> execute)
        {
            _executeWithServerParam = execute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
                _execute.Invoke();
            else if (_executeWithParam != null && parameter != null)
                _executeWithParam.Invoke(parameter as IStrategy);
            else if(_executeWithPersistanceParam != null && parameter != null)
                _executeWithPersistanceParam.Invoke(parameter as PersistanceModel);
            else if (_executeWithServerParam != null && parameter != null)
                _executeWithServerParam.Invoke(parameter as Server);
        }
    }
}
