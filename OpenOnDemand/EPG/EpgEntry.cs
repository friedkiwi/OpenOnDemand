using System;
using System.Runtime.Serialization;

namespace OpenOnDemand.EPG
{
    /// <summary>
    /// This is the EPG Entry data model
    /// </summary>
    public class EpgEntry : ISerializable
    {
		
        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        /// <value>The start.</value>
        public DateTime Start { get; set; }
        /// <summary>
        /// Gets or sets the stop.
        /// </summary>
        /// <value>The stop.</value>
        public DateTime Stop { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the subtitle.
        /// </summary>
        /// <value>The subtitle.</value>
        public string Subtitle { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the m3 u8.
        /// </summary>
        /// <value>The m3 u8.</value>
        public string M3U8 { get; set; }

        public EpgEntry()
        {
        }

        public EpgEntry(SerializationInfo info, StreamingContext context) {
            Start = info.GetDateTime("Start");
            Stop = info.GetDateTime("Stop");
            Title = info.GetString("Title");
            Subtitle = info.GetString("Subtitle");
            Description = info.GetString("Description");
            M3U8 = info.GetString("M3U8");
        }

        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Start", Start);
            info.AddValue("Stop", Stop);
            info.AddValue("Title", Title);
            info.AddValue("Subtitle", Subtitle);
            info.AddValue("Description", Description);
            info.AddValue("M3U8", M3U8);
        }
    }
}
