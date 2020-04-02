﻿using LoadBalancer.Models;
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

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            loadBalancerViewModal.ClearLogs();
        }

        private void AddServer_Click(object sender, RoutedEventArgs e)
        {
            AddServerView inputDialog = new AddServerView();
            string host;
            int port;
            if (inputDialog.ShowDialog() == true)
            {
                host = inputDialog.Host;
                port = int.Parse(inputDialog.Port);
                loadBalancerViewModal.AddServer(host, port);
            }
        }

        private void RemoveServer_Click(object sender, RoutedEventArgs e)
        {
            // TODO Confirmation Dialog
            if (AlgorithmsList.SelectedItems.Count != 0)
            {
                ServerModel server = (ServerModel)ServerList.SelectedItems[0];
                loadBalancerViewModal.RemoveServer(server);
            }
        }

        private void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            loadBalancerViewModal.ToggleLoadBalancer();
        }
        private void ActivateAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            if(AlgorithmsList.SelectedItems.Count != 0)
            {
                BalanceModel algorithm = (BalanceModel)AlgorithmsList.SelectedItems[0];
                loadBalancerViewModal.SetAlgorithm(algorithm);
            }
        }
    }
}
