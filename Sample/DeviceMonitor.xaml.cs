using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SecurityConsole
{
    /// <summary>
    /// Interaction logic for DeviceMonitor.xaml
    /// </summary>
    public partial class DeviceMonitor : Window
    {
        private IoTHubCommunicator _communicator;
 
        public DeviceMonitor()
        {
            InitializeComponent();
            _communicator = new IoTHubCommunicator();
            _communicator.MessageReceivedEvent += _communicator_MessageReceivedEvent;
            _communicator.ReceiveDataFromAzure();

        }

        private void PanicButton_Click(object sender, RoutedEventArgs e)
        {
            var popup = new MainWindow();
            popup.Show();
        }
        private void _communicator_MessageReceivedEvent(object sender, string e)
        {
            //update UI
            listBox.Items.Add(e);
            //start listening again
            _communicator.ReceiveDataFromAzure();
        }
        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            await _communicator.SendDataToAzure(textBox.Text);
        }


    }
}
