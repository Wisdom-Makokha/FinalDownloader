using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Download
{
    internal class DownloadProgress
    {
        public double Percent { get; set; } = 0.0;
        public bool IsCompleted { get; set; } = false;
        public Color StatusColor => CurrentStatusColor(Status);
        public string Status { get; set; } = string.Empty;
        public string Speed { get; set; } = string.Empty;
        public string EstimatedTimeRemaining { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public long DownloadedBytes { get; set; } = 0;
        //public long TotalBytes { get; set; }
        public string DownloadSize => FormatBytes(DownloadedBytes);
        //public string TotalSize => FormatBytes(TotalBytes);

        private static string FormatBytes(long bytes)
        {
            string[] units = { "B", "KiB", "MiB", "GiB", "TiB" };
            int unitIndex = 0;
            double size = bytes;
            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }
            return $"{size:0.##} {units[unitIndex]}";
        }


        public static string CurrentStatus(bool phase)
        {
            return phase ? "Merging" : "Downloading";
        }

        public static Color CurrentStatusColor (string status)
        {
            switch (status.ToLower())
            {
                case "merging":
                    return Color.Yellow;
                case "downloading":
                    return Color.Cyan;
                case "completed":
                    return Color.Orange1;
                case "failed":
                    return Color.Red;
                default:
                    return Color.Magenta;
            }
        }
    }
}
