using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Media;

namespace PlayAlarm
{
    class Program
    {
        public static MyHttpServer httpServer;
        private static SoundPlayer ShirenPlayer = new SoundPlayer("emergency.wav");

        static void Main(string[] args)
        {
            httpServer = new MyHttpServer(8888);
            httpServer.MessageReceivedEvent += event_handler;

            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
  
        }

        static private void event_handler(object sender, string e)
        {
            if (e == "error=1")
                ShirenPlayer.PlayLooping();
            else
                ShirenPlayer.Stop();
        }
    }
}
