using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FinalDownloader.Models.MediaMetadata
{
    internal class MediaContainerBase
    {
        public Guid UniqueId { get; set; } = new Guid();
        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string? Album { get; set; }
        public string? Artist { get; set; }
        public string? Artists { get; set; }
        public int? Year { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateUpdated { get; set; } = DateTime.Now;
        public MediaContainerType Type { get; set; }
        public ICollection<MediaMetadataBase> Items { get; set; }
        [NotMapped]
        public HashSet<MediaMetadataBase> Entries { get; set; }
    }

    public enum MediaContainerType
    {
        Playlist,
        Artist,
        Album,
    }
}
