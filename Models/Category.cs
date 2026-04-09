using FinalDownloader.Models.MediaMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Models
{
    internal class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public MediaType Type { get; set; }
        public string Description { get; set; }
        public string Resolution { get; set; }
        public bool Subtitles { get; set; }
        public bool IsDefault { get; set; }
        public ICollection<MediaMetadataBase> Items { get; set; }
    }
}
