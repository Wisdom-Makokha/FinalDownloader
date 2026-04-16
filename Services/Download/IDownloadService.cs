using FinalDownloader.Models.MediaMetadata;
using FinalDownloader.Models.Settings;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Download
{
    internal interface IDownloadService
    {
        bool ProcessingComplete { get; }
        ConcurrentDictionary<string, List<string>> DownloadErrors { get; }
        Task ProcessDownloadQueue(ArgumentSettings settings, CancellationToken cancellationToken = default);
        Task ProcessDownloadQueueWithProgress(ArgumentSettings settings, CancellationToken cancellationToken = default);
        void EnqueueDownload(MediaMetadataBase metadata);
        void EnqueueDownloads(IEnumerable<MediaMetadataBase> metadataList);
    }
}
