using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Progress
{
    internal class DownloadProgress
    {
        public bool IsCompleted { get; set; } = false;
        public Color StatusColor => CurrentStatusColor(Status);
        public string Status { get; set; } = string.Empty;
        public long DownloadedBytes { get; set; } = 0;
        public long TotalBytes { get; set; }

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
