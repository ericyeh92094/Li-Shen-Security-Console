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
        private AplusVideoC01.wpf_Monitor m_obj;
        private List<AplusVideoC01.wpf_Monitor> m_objList;
        private List<System.Windows.Forms.Integration.WindowsFormsHost> hostList;
        static string host_ip = "192.168.1.234", host_port = "8888";
        private Boolean [] Alerted;
        public Path myPath { get; set; }

        public DeviceMonitor()
        {
            InitializeComponent();
           
            _communicator = new IoTHubCommunicator();
            _communicator.MessageReceivedEvent += _communicator_MessageReceivedEvent;
            _communicator.ReceiveDataFromAzure();

            m_objList = new List<AplusVideoC01.wpf_Monitor>();
            hostList = new List<System.Windows.Forms.Integration.WindowsFormsHost>();

            Alerted = new Boolean[4];
            Alerted[0] = Alerted[1] = Alerted[2] = Alerted[3] = false;


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
                for (int i = 0; i < 4; i++)
                {
                    System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
                    m_obj = new AplusVideoC01.wpf_Monitor();
                    host.Child = m_obj;
                    host.SetValue(Grid.RowProperty, i);
                    host.SetValue(Grid.ColumnProperty, 1);
                    grid_main.Children.Add(host);
                    m_obj.Device_Login(host_ip, host_port, "", "");
                    m_obj.Device_RealPlay(i, 0, 0);
                    m_objList.Add(m_obj);
                    hostList.Add(host);
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("Video Windows set up error");
            }
        }

        private void InitECG()
        {
            PathGeometry myPathGeometry = new PathGeometry();

            var b = new Binding
            {
                Source = "M 0,50 L 200,50"
            };
            
            BindingOperations.SetBinding(myPath, PathGeometry.FiguresProperty, b);
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitButtonImage();
            InitVideoFeeds();
            //InitECG();

            ((App)Application.Current).httpServer.MessageReceivedEvent += PanicButton1_PingReceivedEvent;

            await _communicator.EnumDevices();
        }

        private void PanicButton_Click(object sender, RoutedEventArgs e)
        {
            /*
            var popup = new MainWindow();
            popup.Show();
            */
            Uri resourceUri = new Uri("Resources/bluebutton.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;

            if (sender == PanicButton1)
            {
                if (Alerted[0]) { PanicButton1.Background = brush; Alerted[0] = false; }
            }
            if (sender == PanicButton2)
            {
                if (Alerted[1]) { PanicButton2.Background = brush; Alerted[1] = false; }
            }
            if (sender == PanicButton3)
            {
                if (Alerted[2]) { PanicButton3.Background = brush; Alerted[2] = false; }
            }
            if (sender == PanicButton4)
            {
                if (Alerted[3]) { PanicButton4.Background = brush; Alerted[3] = false; }
            }
        }
        private void _communicator_MessageReceivedEvent(object sender, string e)
        {
            //update UI
            //listBox.Items.Add(e);
            //start listening again
            _communicator.ReceiveDataFromAzure();
        }
        /*
        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            await _communicator.SendDataToAzure(0, textBox.Text);
        }
        */

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

            char[] delimiterChar = { '='};
            string[] str = e.Split(delimiterChar);
            int id = Int32.Parse(str[1]);

            switch (id)
            {
                case 1:
                    PanicButton1.Background = brush;
                    Alerted[0] = true;
                    break;
                case 2:
                    PanicButton2.Background = brush;
                    Alerted[1] = true;
                    break;
                case 3:
                    PanicButton3.Background = brush;
                    Alerted[2] = true;
                    break;
                case 4:
                    PanicButton4.Background = brush;
                    Alerted[3] = true;
                    break;
            }
        }

    }
}
