using System;
using System.Net;
using System.IO;
using System.Text;


namespace OpenOnDemand.Video
{
    public class Downloader
    {
        /// <summary>
        /// The buffer size to use to read server responses.
        /// </summary>
        private const int BUFFSIZE = 512;

        /// <summary>
        /// The logger instance.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The download URL.
        /// </summary>
        private string _DownloadURL = "";

        /// <summary>
        /// The output folder.
        /// </summary>
        private string _OutputFolder = "";

        /// <summary>
        /// The web request used to download the stream.
        /// </summary>
        private HttpWebRequest _WebRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenOnDemand.Video.Downloader"/> class.
        /// </summary>
        /// <param name="DownloadURL">Stream download URL.</param>
        /// <param name="OutputFolder">Output folder for individual chunks.</param>
        public Downloader(string DownloadURL, string OutputFolder) {
            log.Debug("Created Downloader instance.");
            _DownloadURL = DownloadURL;
            _OutputFolder = OutputFolder;
        }

        /// <summary>
        /// Start processing the stream indefinitely.
        /// </summary>
        public void Start() {
            _WebRequest = WebRequest.CreateHttp(_DownloadURL);
            long checkpoint = GetUnixTime();

            log.Info("Starting stream download...");


            HttpWebResponse response = (HttpWebResponse) _WebRequest.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK) {
                log.Error(string.Format("HTTP Error: {0} ({1})", response.StatusCode, _DownloadURL));
                throw new Exception(string.Format("HTTP Error: {0} ({1})", response.StatusCode, _DownloadURL));
            } else {
                log.Info("Stream download started");
            }


            FileStream outputFile = File.OpenWrite(Path.Combine(_OutputFolder, checkpoint - (checkpoint % 10) + ".ts"));

            Stream responseStream = response.GetResponseStream();

            while (true) {
                byte[] buffer = new byte[BUFFSIZE];

                int amountRead = responseStream.Read(buffer, 0, BUFFSIZE);

                outputFile.Write(buffer, 0, amountRead);

                if (GetUnixTime() % 10 == 0 && GetUnixTime() > checkpoint ) {
                    checkpoint = GetUnixTime();
                    outputFile.Flush();
                    outputFile.Close();
                    outputFile = File.OpenWrite(Path.Combine(_OutputFolder, checkpoint + ".ts"));
                    log.Info(string.Format("Swithing to next output file ({0}.ts)", checkpoint));
                    WriteLivestreamFile(checkpoint);
                }

            }

        }

        /// <summary>
        /// Gets the unix time.
        /// </summary>
        /// <returns>The unix time.</returns>
        private long GetUnixTime() {
            return (long) Math.Floor((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds); 
		}


        /// <summary>
        /// Writes the livestream M3U8 playlist file.
        /// </summary>
        private void WriteLivestreamFile(long currentTimestamp)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("#EXTM3U");
            sb.AppendLine("#EXT-X-VERSION:3");
            sb.AppendLine("#EXT-X-TARGETDURATION:10");
            sb.AppendLine(string.Format("#EXT-X-MEDIA-SEQUENCE:{0}", currentTimestamp - 30));

            for (int i = 30; i > 0; i -= 10) {
                sb.AppendLine("#EXTINF:10.0,");
                sb.AppendLine(string.Format("{0}.ts", currentTimestamp - i));
            }

            File.WriteAllText(Path.Combine(_OutputFolder, "live.m3u8"), sb.ToString());
        }
        
    }
}
