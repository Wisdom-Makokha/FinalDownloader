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
        public string Uploader { get; set; }
        public string? UploadDate { get; set; }
        public DateTime DownloadDate { get; set; }
        public long FileSize { get; set; }
        public string FileSizeView => FormatFileSize(FileSize);
        public DateOnly? UploadDateView => UploadDate != null ? DateOnly.ParseExact(UploadDate, "yyyyMMdd") : null;
        public int Duration { get; set; }
        public TimeSpan DurationView => TimeSpan.FromSeconds(Duration);
        public string MediaFormat { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateUpdated { get; set; } = DateTime.Now;
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public Guid? MediaContainerBaseId { get; set; }
        public MediaContainerBase? MediaContainerBase { get; set; }

        private string CreateSafeProgressTitle(int? specifiedLength = null)
        {
            // Determine the target length
            int targetLength = specifiedLength ?? ((Console.WindowWidth / 3) - 7);

            string baseText = Title + "... ";
            string safeTitle;

            if (baseText.Length > targetLength)
            {
                int maxTitleLength = targetLength - 4; // Reserve space for "... "
                safeTitle = Title.Substring(0, maxTitleLength) + "... ";
            }
            else
            {
                // Pad with spaces to reach target length
                safeTitle = baseText.PadRight(targetLength);
            }

            safeTitle = safeTitle.Replace('[', '(').Replace(']', ')');

            string prefix = DownloadIndex == null ? string.Empty : $"#{DownloadIndex} ";
            return $"{prefix}{safeTitle}";
        }

        private string FormatFileSize(long fileSize)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = fileSize;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
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
