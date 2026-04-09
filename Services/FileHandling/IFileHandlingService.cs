using FinalDownloader.Models.MediaMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.FileHandling
{
    internal interface IFileHandlingService
    {
        Task MoveDownloadToDestination(MediaMetadataBase metadata);
        Task CleanupTemporaryFilesAsync();
    }
}
