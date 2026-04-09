using FinalDownloader.Models.MediaMetadata;
using FinalDownloader.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display.ScreenData
{
    internal class ProgressScreenData
    {
        public ArgumentSettings ArgumentSettings { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public List<MediaMetadataBase> DownloadItems { get; set; }
    }
}
