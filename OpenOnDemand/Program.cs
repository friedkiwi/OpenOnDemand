using System;
using System.Threading;

using OpenOnDemand.Video;
using OpenOnDemand.EPG;
using System.Configuration;
using System.IO;

namespace OpenOnDemand
{
    class MainClass
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {

            log.Info("OpenOnDemand Started.");

			Configuration configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            KeyValueConfigurationCollection config = configManager.AppSettings.Settings;

            if (!config["XMLTV"].Value.Contains("://")) {
                log.Error("Invalid config - invalid XMLTV URL");
                return;
            }

			if (!config["StreamURL"].Value.Contains("://"))
			{
				log.Error("Invalid config - invalid Stream URL");
				return;
			}

            if (!Directory.Exists(config["DataDirectory"].Value)) {
				log.Error("Invalid config - Data directory does not exist");
				return;
            }

            Downloader dl = new Downloader(config["StreamURL"].Value, config["DataDirectory"].Value);
            EpgParser epg = new EpgParser(config["XMLTV"].Value, config["EPGChannelName"].Value, config["DataDirectory"].Value);

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
