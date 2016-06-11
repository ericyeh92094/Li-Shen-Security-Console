using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Client;
using Microsoft.ServiceBus.Messaging;
using System.Threading;


namespace SecurityConsole
{
    class IoTHubCommunicator
    {
        private RegistryManager registryManager;
        public event EventHandler<string> MessageReceivedEvent;
        private string _iotHubConnectionString =
            "HostName=lishansecurity.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=PuzeLHuLPxH7f48bJppTnVmK7KDLdpp4mIoe/X4+DYg=";
        private string connectionString = "HostName=lishansecurity.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=PuzeLHuLPxH7f48bJppTnVmK7KDLdpp4mIoe/X4+DYg=";
        private string iotHubD2cEndpoint = "messages/events";
        private EventHubClient eventHubClient;
        private EventHubReceiver[] eventHubReceiver;
        private string[] d2cPartitions;
        private int partnum = 0;

        static List<string> deviceId;
        static List<string> deviceKeys;
        static List<DeviceClient> deviceClients;
        static string iotHubUri = "lishansecurity.azure-devices.net";
        static string devicekey = "W2rTkHcLmSKqcK79WtGlrvpS4HykaIAW+yxOGb1qpMA=";

        public IoTHubCommunicator()
        {
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;
            eventHubReceiver = new EventHubReceiver[5];

            int i = 0;
            foreach (string partition in d2cPartitions)
            {
                eventHubReceiver[i] = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
                i++;
            }
            partnum = i;

            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        }

        public async Task EnumDevices()
        {
            IEnumerable<Device> devices;
            int i = 0;

            try
            {
                devices = await registryManager.GetDevicesAsync(10);

                deviceId = new List<string>();
                deviceKeys = new List<string>();
                deviceClients = new List<DeviceClient>();

                foreach (Device dev in devices)
                {
                    deviceId.Add(dev.Id);
                    string key = dev.Authentication.SymmetricKey.PrimaryKey;
                    deviceKeys.Add(key);

                    //deviceClients.Add(DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(dev.Id, key)));
                    deviceClients.Add(DeviceClient.CreateFromConnectionString(_iotHubConnectionString, dev.Id));
                }
            }
            catch (DeviceNotFoundException)
            { }        
        }

        public async Task SendDataToAzure(int numId, string message)
        {
            DeviceClient deviceClient = deviceClients[numId]; // DeviceClient.CreateFromConnectionString(_iotHubConnectionString);
            var msg = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(message));
            await deviceClient.SendEventAsync(msg);
        }

        public async Task ReceiveDataFromAzure()
        {
            /*
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(_iotHubConnectionString);
            Message receivedMessage;
            string messageData;
            while (true)
            {
                receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage != null)
                {
                    messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    this.OnMessageReceivedEvent(messageData);
                    await deviceClient.CompleteAsync(receivedMessage);
                }
            }
            */
            while (true)
            {
                for (int i = 0; i < partnum; i++)
                {
                    EventData eventData = await eventHubReceiver[i].ReceiveAsync();
                    if (eventData == null) continue;
                    string data = Encoding.UTF8.GetString(eventData.GetBytes());
                    this.OnMessageReceivedEvent(data);
                }
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
