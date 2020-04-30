using BalanceStrategy;
using LoadBalancer.Models;
using LoadBalancer.ViewModels;
using LoadBalancer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCPCommunication;

namespace LoadBalancer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoadBalancerView : Window
    {
        private LoadBalancerViewModel loadBalancerViewModal;
        public LoadBalancerView()
        {
            InitializeComponent();
            loadBalancerViewModal = new LoadBalancerViewModel();
            DataContext = loadBalancerViewModal;
        }
    }
}
