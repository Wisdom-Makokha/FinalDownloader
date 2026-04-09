using FinalDownloader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Download
{
    internal class DownloadData
    {
        public string Url { get; set; }
        public Category Category { get; set; }
        public string Format { get; set; }
        public bool IsPlaylist { get; set; }
        public int? PlaylistStartIndex { get; set; }
        public int? PlaylistEndIndex { get; set; }

        public override string ToString()
        {
            return $"Download Data Details:- \n" +
                $"Url: {Url}\n" +
                $"Category: {Category.Name}\n" +
                $"Format: {Format}\n" +
                $"Is Playlist: {IsPlaylist}\n" +
                $"Playlist Start Index: {PlaylistStartIndex}\n"+
                $"Playlist End Index: {PlaylistEndIndex}";
        }

        public static bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
