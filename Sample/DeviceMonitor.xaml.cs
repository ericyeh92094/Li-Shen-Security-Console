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
using System.Windows.Media.Animation;
using System.Media;
using Twilio;
using System.IO;
using RestSharp;

namespace SecurityConsole
{
    /// <summary>
    /// Interaction logic for DeviceMonitor.xaml
    /// </summary>
    public partial class DeviceMonitor : Window
    {
        //private IoTHubCommunicator _communicator;
        private SerialComm _serial_comm;

        private AplusVideoC01.wpf_Monitor m_obj;
        private List<AplusVideoC01.wpf_Monitor> m_objList;
        private List<System.Windows.Forms.Integration.WindowsFormsHost> hostList;
        static string host_ip = "192.168.1.234", host_port = "8888";
        static string alarmUrl = "http://127.0.0.1:8888";
        private Boolean [] Alerted;

        private SoundPlayer ShirenPlayer = new SoundPlayer("emergency.wav");


        public System.Windows.Shapes.Path myPath { get; set; }

        public DeviceMonitor()
        {
            InitializeComponent();

            /*
            _communicator = new IoTHubCommunicator();
            _communicator.MessageReceivedEvent += _communicator_MessageReceivedEvent;
            */

            //_serial_comm = new SerialComm();
           // _serial_comm.MessageReceivedEvent += _communicator_MessageReceivedEvent;

            m_objList = new List<AplusVideoC01.wpf_Monitor>();
            hostList = new List<System.Windows.Forms.Integration.WindowsFormsHost>();

            Alerted = new Boolean[4];
            Alerted[0] = Alerted[1] = Alerted[2] = Alerted[3] = false;


        }
        private void InitButtonImage()
        {
            Uri resourceUri = new Uri("Resources/green_button.png", UriKind.Relative);
            StreamResourceInfo streamInfo = System.Windows.Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;
            brush.Stretch = Stretch.None;

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
                    host.Width = (int) grid_main.ColumnDefinitions[1].ActualWidth;
                    host.Height = (int)grid_main.RowDefinitions[0].ActualHeight;
                    host.Child = m_obj;
                    host.SetValue(Grid.RowProperty, i);
                    host.SetValue(Grid.ColumnProperty, 1);
                    grid_main.Children.Add(host);
                    m_obj.Device_Login(host_ip, host_port, "", "");

                    int id = i;
                    if (i == 0) id = 1;
                    else if (i == 1) id = 0;

                    m_obj.Device_RealPlay(id, 0, 0);
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

            /*
            //InitECG();

            //((App)System.Windows.Application.Current).httpServer.MessageReceivedEvent += PanicButton1_PingReceivedEvent;

            //await _communicator.EnumDevices();
            //_communicator.ReceiveDataFromAzure();
            //_communicator.ReceiveDataFromDevice();
            */
        }

        private void PanicButton_Click(object sender, RoutedEventArgs e)
        {
            int id_num = -1;

            if (sender == PanicButton1)
            {
                id_num = 0;
            }
            if (sender == PanicButton2)
            {
                id_num = 1;
            }
            if (sender == PanicButton3)
            {
                id_num = 2;
            }
            if (sender == PanicButton4)
            {
                id_num = 3;
            }

            if (Alerted[id_num])
            {
                release_button(id_num);
            }
            else
            {
                alarm_button(id_num);
            }
        }
        private void _communicator_MessageReceivedEvent(object sender, string e)
        {
            //update UI
            //listBox.Items.Add(e);
            //start listening again

            if (e.IndexOf("SOS") > 0)
            {
                char[] delimiterChar = { '>'};
                string[] substr = e.Split(delimiterChar);
                int id_num = Int32.Parse(substr[0]) - 1;

                if (!Alerted[id_num])
                {
                    Console.WriteLine("Alarm Received {0}", e);
                    alarm_button(id_num);
                    Fire_SMS(id_num, true);
                }
            }
            else if (e.StartsWith("Off:"))
            {
                char[] delimiterChar = { ':' };
                string[] substr = e.Split(delimiterChar);
                int id_num = substr[1][0] - '1';

                if (Alerted[id_num])
                {
                    Console.WriteLine("Alarm Released {0}", e);
                    release_button(id_num);
                    Fire_SMS(id_num, false);
                }

            }
           // _communicator.ReceiveDataFromAzure();
        }

        public void Fire_SMS(int id, bool alarmOn)
        { 
            string AccountSid = "ACb8ff77dff70eb33393322f9be8ac4154";
            string AuthToken = "6d3d9d39eaf3d87d1d60fcd4d8f963a1";
            var twilio = new TwilioRestClient(AccountSid, AuthToken);
            string alarmStr = "";

            if (alarmOn) alarmStr = "啟動於 ";
            else alarmStr = "解除於 ";

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fullName = System.IO.Path.Combine(desktopPath, "phonenum.txt");
                using (StreamReader steamReader = new StreamReader(fullName))
                {
                    int counter = 0;
                    string line;

                    while ((line = steamReader.ReadLine()) != null)
                    {
                        var message = twilio.SendMessage( "+16315935729", line,
                            "緊急按鈕" + id.ToString() + alarmStr + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() );
                        Console.WriteLine(line);
                        counter++;
                    }

                }
            } catch (Exception)
            {
                Console.WriteLine("The file could not be read. No SMS will be sent.");
            }

        }

        private void Ellipse_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard s = (Storyboard)this.Resources["SB0"];
            if (sender == e1_1)
                Storyboard.SetTargetName(s, "e1_1");
            else if (sender == e3_1)
                Storyboard.SetTargetName(s, "e3_1");
            else if (sender == e4_1)
                Storyboard.SetTargetName(s, "e4_1");
            s.Begin();
        }

        public void release_button(int id_num)
        {
            Uri resourceUri = new Uri("Resources/green_button.png", UriKind.Relative);
            StreamResourceInfo streamInfo = System.Windows.Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;
            brush.Stretch = Stretch.None;

            if (Alerted[id_num])
            {
                Alerted[id_num] = false;
                //ShirenPlayer.Stop();
                AlarmStop();
            }

            switch (id_num)
            {
                case 0:
                    PanicButton1.Background = brush; break;
                case 1:
                    PanicButton2.Background = brush; break;
                case 2:
                    PanicButton3.Background = brush; break;
                case 3:
                    PanicButton4.Background = brush; break;
            }
        }

        public void alarm_button(int id_num)
        {             
            Uri resourceUri = new Uri("Resources/red_button.png", UriKind.Relative);
            StreamResourceInfo streamInfo = System.Windows.Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;
            brush.Stretch = Stretch.None;

            switch (id_num)
            {
                case 0:
                    PanicButton1.Background = brush;
                    Alerted[0] = true;
                    break;
                case 1:
                    PanicButton2.Background = brush;
                    Alerted[1] = true;
                    break;
                case 2:
                    PanicButton3.Background = brush;
                    Alerted[2] = true;
                    break;
                case 3:
                    PanicButton4.Background = brush;
                    Alerted[3] = true;
                    break;
            }

            //ShirenPlayer.PlayLooping();
            AlarmOut();

        }

        private static void AlarmOut()
        {
            var client = new RestClient(alarmUrl);
            var request = new RestRequest(Method.POST);

            request.AddParameter("error", "1");
            IRestResponse response = client.Execute(request);
        }

        private static void AlarmStop()
        {
            var client = new RestClient(alarmUrl);
            var request = new RestRequest(Method.POST);

            request.AddParameter("error", "0");
            IRestResponse response = client.Execute(request);
        }
    }
}
