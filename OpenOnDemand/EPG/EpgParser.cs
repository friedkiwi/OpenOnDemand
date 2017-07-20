using System;
using System.Xml.Linq;
using System.Text;
using System.Linq;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;


namespace OpenOnDemand.EPG
{
    public class EpgParser
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The xmltv URL.
        /// </summary>
        private string _XmltvUrl = "";

        /// <summary>
        /// The name of the channel, as defined in the XMLTV XML file.
        /// </summary>
        private string _ChannelName = "";

        /// <summary>
        /// The output directory for the EPG files.
        /// </summary>
        private string _OutputDirectory = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenOnDemand.EPG.EpgParser"/> class.
        /// </summary>
        /// <param name="XMLTVUrl">The URL of the XMLTV file.</param>
        /// <param name="ChannelName">The name of the channel, as defined in the XMLTV file.</param>
        /// <param name="OutputDirectory">The path where EPG output files should be placed.</param>
        public EpgParser(string XMLTVUrl, string ChannelName, string OutputDirectory)
        {
            _XmltvUrl = XMLTVUrl;
            _ChannelName = ChannelName;
            _OutputDirectory = OutputDirectory;
        }

        /// <summary>
        /// Parse the available shows from the EPG.
        /// </summary>
        public void ParseShows() {
            log.Info("Parsing shows from EPG...");


            log.Debug("Starting EPG download...");
            XDocument xdoc = XDocument.Load(_XmltvUrl);
            log.Debug("EPG file downloaded");

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            string id = _ChannelName;



            var channels = from channel in xdoc.Descendants("channel")
                           select new
                           {
                                ID = channel.Attribute("id").Value,
                                Name = channel.Element("display-name").Value
                           };

            bool channelFound = false;

            foreach (var channel in channels) {
                log.Debug(string.Format("Found channel '{0}' with id '{1}' in EPG info", channel.Name, channel.ID));

                if (channel.ID == _ChannelName) {
                    id = channel.ID;
                    channelFound = true;
                    log.Debug("Found channel by ID.");
                    break;
                }

                if (channel.Name == _ChannelName) {
                    id = channel.ID;
                    channelFound = true;
                    log.Debug("Found channel by Name.");
                    break;
                }
            }

            if ( ! channelFound) {
                log.Error("Could not find channel in EPG - running EPG-less!!!");
                return;
            }

            var shows = from show in xdoc.Descendants("programme")
                        where show.Attribute("channel").Value == id
                        select new EpgEntry
                        {
                Start = DateTime.ParseExact(show.Attribute("start").Value, "yyyyMMddHHmmss zzz", cultureInfo),
                Stop = DateTime.ParseExact(show.Attribute("stop").Value, "yyyyMMddHHmmss zzz", cultureInfo),
                Title = show.Element("title").Value,
                Subtitle = show.Element("sub-title") != null  ? show.Element("sub-title").Value : "",
                Description = show.Element("desc") != null ? show.Element("desc").Value : "",
                M3U8 = GetM3U8Filename(
                    DateTime.ParseExact(show.Attribute("start").Value, "yyyyMMddHHmmss zzz", cultureInfo),
                    DateTime.ParseExact(show.Attribute("stop").Value, "yyyyMMddHHmmss zzz", cultureInfo),
                    show.Element("title").Value,
                    show.Element("sub-title") != null ? show.Element("sub-title").Value : "",
                    show.Element("desc") != null ? show.Element("desc").Value : ""),
                        };

            foreach (var show in shows) {
                
                log.Debug(string.Format("Writing M3U8 file [{1}] for show '{0}' ", show.Title, show.M3U8));

                File.WriteAllText(Path.Combine(_OutputDirectory, show.M3U8), GetM3U8(show.Start, show.Stop));

            }

            log.Info("Writing new index file...");


            string showListingFile = Path.Combine(_OutputDirectory, "index.json");

            if (File.Exists(showListingFile)) {
                var existingListing = JsonConvert.DeserializeObject<List<EpgEntry>>(File.ReadAllText(Path.Combine(_OutputDirectory, "index.json")));
                var currentListing = JsonConvert.DeserializeObject<List<EpgEntry>>(JsonConvert.SerializeObject(shows));

                IEnumerable<EpgEntry> merged = existingListing.Union( (IEnumerable<EpgEntry>) currentListing);

				File.WriteAllText(showListingFile, JsonConvert.SerializeObject(merged));


            } else {
                File.WriteAllText(showListingFile, JsonConvert.SerializeObject(shows));
            }




            log.Info("Parsed shows from EPG!");
        }

        /// <summary>
        /// Gets the M3U8 content for a show based on a given start and end timestamp.
        /// </summary>
        /// <returns>M3U8 file content</returns>
        /// <param name="Start">The start timestamp for the show.</param>
        /// <param name="Stop">The end timestamp for the show.</param>
        private string GetM3U8(DateTime Start, DateTime Stop) {
            StringBuilder m3u8 = new StringBuilder();

            long startTimestamp = (long)Math.Floor((Start.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            long stopTimestamp = (long)Math.Floor((Stop.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);

            startTimestamp = startTimestamp -= startTimestamp % 10;
            stopTimestamp = stopTimestamp += 10 - (stopTimestamp % 10);

			m3u8.AppendLine("#EXTM3U");
            m3u8.AppendLine("#EXT-X-VERSION:3");
            m3u8.AppendLine("#EXT-X-MEDIA-SEQUENCE:0");
            m3u8.AppendLine("#EXT-X-TARGETDURATION:10");

            for (long ts_seg = startTimestamp; ts_seg <= stopTimestamp; ts_seg +=10) {
                m3u8.AppendLine("#EXTINF:10.0");
                m3u8.AppendLine(string.Format("{0}.ts", ts_seg));
            }

            m3u8.AppendLine("#EXT-X-ENDLIST");

            return m3u8.ToString();
        }

        /// <summary>
        /// Gets a reproducible, but globally unique filename for a show based on the supplied parameters.
        /// </summary>
        /// <returns>A filename.</returns>
        /// <param name="Start">Start timestamp</param>
        /// <param name="Stop">Stop timestamp</param>
        /// <param name="Title">The title of the show.</param>
        /// <param name="Subtitle">The subtitle of the show.</param>
        /// <param name="Description">The description for the show.</param>
        private string GetM3U8Filename(DateTime Start, DateTime Stop, string Title, string Subtitle, string Description) {
            SHA256 sha = SHA256.Create();

            byte[] input = System.Text.Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}{3}{4}", Start.ToString(), Stop.ToString(), Title, Subtitle, Description));

            byte[] hash = sha.ComputeHash(input);

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("x2"));
			}

            return string.Format("{0}.m3u8", sb.ToString().Substring(0,20));
		}
    }
}
