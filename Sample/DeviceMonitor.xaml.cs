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
using System.Windows.Resources;

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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).httpServer.MessageReceivedEvent += PanicButton1_PingReceivedEvent;
            await _communicator.EnumDevices();
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
            await _communicator.SendDataToAzure(0, textBox.Text);
        }

        public void PanicButton1_PingReceivedEvent(object sender, string e)
        {
            Console.WriteLine("Alarm Received");
            change_button();

        }

        public void change_button()
        { 
            
            Uri resourceUri = new Uri("Resources/reddrop.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;

            PanicButton1.Background = brush;
        }

    }
}
