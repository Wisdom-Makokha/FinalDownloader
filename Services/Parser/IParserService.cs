using FinalDownloader.Models;
using FinalDownloader.Models.MediaMetadata;
using FinalDownloader.Services.Download;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Parser
{
    internal interface IParserService
    {
        DownloadProgress? ParseProgressString(string progressString);
        MediaMetadataBase ParseMetadata(string output, Category category, string mediaFormat);
        MediaContainerBase ParseContainerMetadata(string output, List<string> entries, Category category, string mediaFormat);
    }
}
