using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Management;

namespace SecurityConsole
{
    class SerialComm
    {
        bool _continue;
        SerialPort _serialPort;
        Thread readThread = null;

        public event EventHandler<string> MessageReceivedEvent;

        public SerialComm()
        {
            SetupSerial();
        }

        public static string EnumPorts()
        {
            string namePort = "";
            //Below is code pasted from WMICodeCreator
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\WMI",
                    "SELECT * FROM MSSerial_PortName");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    //If the serial port's instance name contains USB 
                    //it must be a USB to serial device
                    if (queryObj["InstanceName"].ToString().Contains("USB"))
                    {
                        //Console.WriteLine(queryObj["PortName"] + "is a USB to SERIAL adapter / converter");
                        namePort = (string)queryObj["PortName"];

                        return namePort;

                    }
                }
            }
            catch (ManagementException e)
            {
                //MessageBox.Show("An error occurred while querying for WMI data: " + e.Message);
                namePort = "";
            }

            return namePort;
        }
        public void SetupSerial()
        {
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

            readThread = new Thread(SerialRead);

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            string PortName = SerialPort.GetPortNames()[0]; // get the first free com port
            PortName = EnumPorts();
            if (PortName == "") // no port exist, abort the program
                System.Environment.Exit(0);

            _serialPort.PortName = PortName;
            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 5000;

            _serialPort.Open();
            _continue = true;
            readThread.Start();
        }

        public  void CloseSerial()
        {
            readThread.Join();
            _serialPort.Close();
        }

        public  void SerialRead()
        {
            while (_continue)
            {
                try
                {
                    byte[] buffer = new byte[100];
                    int b = _serialPort.ReadByte();

                    if ((b >= '1' && b <= '4') || b == 'O') // started with '1' - '4' or 'O'
                    {
                        buffer[0] = (byte)b;
                        for (int i = 1; i < 20; i++)
                            buffer[i] = (byte)_serialPort.ReadByte();

                        string message = Encoding.ASCII.GetString(buffer, 0, 20);
                        message.TrimEnd(' '); // remove all blanks

                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => MessageReceivedEvent(this, message)));
                    }
                    //this.OnMessageReceivedEvent(message);
                }
                catch (TimeoutException) { }
            }
        }

        protected virtual void OnMessageReceivedEvent(string s)
        {
            EventHandler<string> handler = MessageReceivedEvent;
            if (handler != null)
            {
                handler(this, s);
            }
        }
    }
}
