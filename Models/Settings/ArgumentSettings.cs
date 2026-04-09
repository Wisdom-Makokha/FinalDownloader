using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Models.Settings
{
    internal class ArgumentSettings
    {
        public string PreferredResolution { get; set; }
        public string PreferredAudioFormat { get; set; }
        public string PreferredVideoFormat { get; set; }
        public string PreferredSubtitleFormat { get; set; }
        public bool AudioOnly { get; set; }
        public bool Subtitles { get; set; }
        public bool EmbedMetadata { get; set; }
        public bool EmbedSubtitles { get; set; }
        public bool Progress { get; set; }
        public bool Thumbnail { get; set; }
        public bool Simulate { get; set; }
        public bool IgnoreErrors { get; set; }
        public int MaxRetries { get; set; }
    }
}
