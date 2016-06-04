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
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool m_Loaded = false;
        private AplusVideoC01.wpf_Monitor m_obj = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            if (but == but_play)
            {
                Console.WriteLine("play click");
                m_obj.Device_RealPlay(Int32.Parse(tb_ch.Text), 0, 0);
            }
            else if (but == but_stop)
            {
                Console.WriteLine("stop click");
            }
        }
        private void Playback_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;            
            Console.WriteLine("playback click");
            m_obj.Device_PlayBackByTime(Int32.Parse(tb_ch.Text), 0, "2015/11/05 16:00");
        }
        private void Snapshot_Click(object sender, RoutedEventArgs e)
        {
            m_obj.Device_CapturePicture(@"D:\Print Only\", "snap", 1);
        }       
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_Loaded == false)
            {
                try
                {
                    System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
                    m_obj = new AplusVideoC01.wpf_Monitor();
                    host.Child = m_obj;
                    host.SetValue(Grid.RowProperty, 1);
                    grid_main.Children.Add(host);
                    m_obj.Device_Login(tb_host.Text, tb_port.Text, "", "");
                    m_obj.CapturePictureComplete += new AplusVideoC01.wpf_Monitor.CapturePictureCompleteHandler(m_obj_CapturePictureComplete);
                }
                catch (Exception ee)
                {
                }
            }
        }

        void m_obj_CapturePictureComplete(string SavePath)
        {
            MessageBox.Show("Snapshot File saved...");
        }
    }
}
