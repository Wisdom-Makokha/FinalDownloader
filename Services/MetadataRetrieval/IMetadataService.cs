using FinalDownloader.Models.MediaMetadata;
using FinalDownloader.Services.Download;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.MetadataRetrieval
{
    internal interface IMetadataService
    {
        Task<MediaContainerBase?> GetContainerMetadataAsync(DownloadData downloadData, CancellationToken cancellationToken = default);
        Task<MediaMetadataBase?> GetMetadataAsync(DownloadData downloadData, CancellationToken cancellationToken = default);
        MediaMetadataBase UpdateMetadataWithDownloadInfo(MediaMetadataBase metadata, string downloadPath);
    }
}
