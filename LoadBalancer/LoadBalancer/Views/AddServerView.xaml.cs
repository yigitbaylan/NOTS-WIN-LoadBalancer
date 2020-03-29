using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LoadBalancer.Views
{
    /// <summary>
    /// Interaction logic for AddServerView.xaml
    /// </summary>
    public partial class AddServerView : Window
    {
        public AddServerView(string host = "http://", string defaultPort = "80")
        {
            InitializeComponent();
			txtHost.Text = host;
			txtPort.Text = defaultPort;
		}

		private void btnDialogOk_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			txtHost.Focus();
		}
		public string Host
		{
			get { return txtHost.Text; }
		}
		public string Port
		{
			get { return txtPort.Text; }
		}
	}
}
