using System;
using System.Threading;

using OpenOnDemand.Video;
using OpenOnDemand.EPG;

namespace OpenOnDemand
{
    class MainClass
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
            log.Info("OpenOnDemand Started.");

            Downloader dl = new Downloader("http://localhost:9981/stream/channel/8955f1567c07223b179ac923f991bb8a?profile=openod-stream", "/Users/yvanjanssens/Desktop/ts_out");
            EpgParser epg = new EpgParser("http://localhost:9981/xmltv/channels", "BBC One HD", "/Users/yvanjanssens/Desktop/ts_out");

            Thread downloaderThread = new Thread(new ThreadStart(delegate {
              dl.Start();  
            }));


            Timer epgTimer = new Timer(new TimerCallback(delegate(object state)
            {
                epg.ParseShows();
            }), null, 0, 60000);

            downloaderThread.Start();


            while (downloaderThread.IsAlive) {
                downloaderThread.Join();
            }


            //dl.Start();
        }
    }
}
