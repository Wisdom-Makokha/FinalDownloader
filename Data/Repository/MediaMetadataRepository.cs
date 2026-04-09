using FinalDownloader.Data.Interface;
using FinalDownloader.Models.MediaMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Data.Repository
{
    internal class MediaMetadataRepository : Repository<MediaMetadataBase>, IMediaMetadataBaseRepository
    {
        public MediaMetadataRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
