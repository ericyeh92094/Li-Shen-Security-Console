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
        private bool m_Loaded = false;
        private AplusVideoC01.wpf_Monitor m_obj1 = null, m_obj2 = null, m_obj3 = null, m_obj4 = null;
        static string host_ip = "192.168.1.234", host_port = "8888";

        public DeviceMonitor()
        {
            InitializeComponent();
            _communicator = new IoTHubCommunicator();
            _communicator.MessageReceivedEvent += _communicator_MessageReceivedEvent;
            _communicator.ReceiveDataFromAzure();

        }

        private void InitButtonImage()
        {
            Uri resourceUri = new Uri("Resources/bluebutton.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;
            PanicButton1.Background = brush;
            PanicButton2.Background = brush;
            PanicButton3.Background = brush;
            PanicButton4.Background = brush;
        }

        private void InitVideoFeeds()
        {
            try
            {
                System.Windows.Forms.Integration.WindowsFormsHost host1 = new System.Windows.Forms.Integration.WindowsFormsHost();
                m_obj1 = new AplusVideoC01.wpf_Monitor();
                host1.Child = m_obj1;
                host1.SetValue(Grid.RowProperty, 0);
                host1.SetValue(Grid.ColumnProperty, 1);
                grid_main.Children.Add(host1);
                m_obj1.Device_Login(host_ip, host_port, "", "");
                //m_obj1.CapturePictureComplete += new AplusVideoC01.wpf_Monitor.CapturePictureCompleteHandler(m_obj_CapturePictureComplete);

                System.Windows.Forms.Integration.WindowsFormsHost host2 = new System.Windows.Forms.Integration.WindowsFormsHost();
                m_obj2 = new AplusVideoC01.wpf_Monitor();
                host2.Child = m_obj2;
                host2.SetValue(Grid.RowProperty, 1);
                host2.SetValue(Grid.ColumnProperty, 1);
                grid_main.Children.Add(host2);
                m_obj2.Device_Login(host_ip, host_port, "", "");
                //m_obj2.CapturePictureComplete += new AplusVideoC01.wpf_Monitor.CapturePictureCompleteHandler(m_obj_CapturePictureComplete);

                System.Windows.Forms.Integration.WindowsFormsHost host3 = new System.Windows.Forms.Integration.WindowsFormsHost();
                m_obj3 = new AplusVideoC01.wpf_Monitor();
                host3.Child = m_obj3;
                host3.SetValue(Grid.RowProperty, 2);
                host3.SetValue(Grid.ColumnProperty, 1);
                grid_main.Children.Add(host3);
                m_obj3.Device_Login(host_ip, host_port, "", "");
                //m_obj3.CapturePictureComplete += new AplusVideoC01.wpf_Monitor.CapturePictureCompleteHandler(m_obj_CapturePictureComplete);

                System.Windows.Forms.Integration.WindowsFormsHost host4 = new System.Windows.Forms.Integration.WindowsFormsHost();
                m_obj4 = new AplusVideoC01.wpf_Monitor();
                host4.Child = m_obj4;
                host4.SetValue(Grid.RowProperty, 3);
                host4.SetValue(Grid.ColumnProperty, 1);
                grid_main.Children.Add(host4);
                m_obj4.Device_Login(host_ip, host_port, "", "");
                //m_obj4.CapturePictureComplete += new AplusVideoC01.wpf_Monitor.CapturePictureCompleteHandler(m_obj_CapturePictureComplete);
            }
            catch (Exception ee)
            {
                Console.WriteLine("Video Windows set up error");
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitButtonImage();
            InitVideoFeeds();

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
            Console.WriteLine("Alarm Received {0}", e);
            change_button(e);

        }

        public void change_button(string e)
        { 
            
            Uri resourceUri = new Uri("Resources/redbutton.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;

            PanicButton1.Background = brush;
        }

    }
}
