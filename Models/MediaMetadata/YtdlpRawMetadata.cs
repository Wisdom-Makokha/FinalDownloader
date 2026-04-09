using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FinalDownloader.Models.MediaMetadata
{
    internal class YtdlpRawMetadata
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("webpage_url")]
        public string Webpage_Url { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("uploader")]
        public string Uploader { get; set; }
        [JsonPropertyName("upload_date")]
        public string Upload_Date { get; set; }
        [JsonPropertyName("playlist_index")]
        public int? Playlist_Index { get; set; }
        [JsonPropertyName("duration")]
        public double? Duration { get; set; } // Duration in seconds
        [JsonPropertyName("playlist_count")]
        public int? Playlist_Count { get; set; }
        [JsonPropertyName("view_count")]
        public long? View_Count { get; set; }
        [JsonPropertyName("like_count")]
        public long? Like_Count { get; set; }
        [JsonPropertyName("dislike_count")]
        public long? Dislike_Count { get; set; }
        [JsonPropertyName("channel")]
        public string? Channel { get; set; }
        [JsonPropertyName("album")]
        public string? Album { get; set; }
        [JsonPropertyName("artist")]
        public string? Artist { get; set; }
        [JsonPropertyName("artists")]
        public List<string>? Artists { get; set; } = new List<string>();
        [JsonPropertyName("track_number")]
        public int? Track_Number { get; set; }
        [JsonPropertyName("release_year")]
        public int? Release_Year { get; set; }
        [JsonPropertyName("genres")]
        public List<string>? Genres { get; set; } = new List<string>();
        //[JsonPropertyName("thumbnail")]
        //public string? Thumbnail { get; set; }
        //[JsonPropertyName("entries")]
        //public List<YtdlpRawMetadata>? Entries { get; set; } = new List<YtdlpRawMetadata>();
    }
}
