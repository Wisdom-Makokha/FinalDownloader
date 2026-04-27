using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Models.Settings
{
    internal class DownloadSettings
    {
        public string TemporaryDirectory => Path.Combine(Path.GetTempPath(), "FinalDownloaderTempFiles");
        public bool DeleteTempFiles { get; set; }
        public int MaxConcurrentDownloads { get; set; }
        public List<string> SupportedVideoFormats { get; set; }
        public List<string> SupportedAudioFormats { get; set; }
        public List<string> SupportedResolutions { get; set; }
        public List<string> SupportedSubtitleFormats { get; set; }
    }
}
