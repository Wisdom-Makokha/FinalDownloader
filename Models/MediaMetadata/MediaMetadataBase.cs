using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Models.MediaMetadata
{
    internal class MediaMetadataBase
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ProgressTitle => CreateSafeProgressTitle();
        [NotMapped]
        public int? DownloadIndex { get; set; }
        public string Url { get; set; }
        public int? PlaylistIndex { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Artist { get; set; }
        public string? Artists { get; set; }
        public string? Genre { get; set; }
        public string? Genres { get; set; }
        public string Uploader { get; set; }
        public string? UploadDate { get; set; }
        public DateTime DownloadDate { get; set; }
        public DateOnly? UploadDateView => UploadDate != null ? DateOnly.ParseExact(UploadDate, "yyyyMMdd") : null;
        public double Duration { get; set; }
        public TimeSpan DurationView => TimeSpan.FromSeconds(Duration);
        public string MediaFormat { get; set; }
        public int CategoryId { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateUpdated { get; set; } = DateTime.Now;
        public Category Category { get; set; }
        public Guid? MediaContainerBaseId { get; set; }
        public MediaContainerBase? MediaContainerBase { get; set; }

        private string CreateSafeProgressTitle()
        {
            var consoleLengthLimit = (Console.WindowWidth / 3) - 7; // Adjust this limit as needed
            string safeTitle = Title.Length > consoleLengthLimit ? Title.Substring(0, consoleLengthLimit) + ".. " : (Title + ".. ").PadRight(consoleLengthLimit);
            safeTitle = safeTitle.Replace('[', '(').Replace(']', ')');

            return $"{(DownloadIndex == null ? string.Empty : "#" + DownloadIndex + " ")}{safeTitle}";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            { return false; }

            MediaMetadataBase other = (MediaMetadataBase)obj;
            return Id == other.Id && Title == other.Title && Url == other.Url;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Title, Url);
        }

    }
    public enum MediaType
    {
        Video,
        Audio,
        VideoNAudio
    }
}
