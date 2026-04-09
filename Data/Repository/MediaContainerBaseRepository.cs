using FinalDownloader.Data.Interface;
using FinalDownloader.Models.MediaMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Data.Repository
{
    internal class MediaContainerBaseRepository : Repository<MediaContainerBase>, IMediaContainerBaseRepository
    {
        public MediaContainerBaseRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
