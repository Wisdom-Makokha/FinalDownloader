using FinalDownloader.Models;
using FinalDownloader.Models.MediaMetadata;
using FinalDownloader.Models.Settings;
using FinalDownloader.Services.Configuration;
using FinalDownloader.Services.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Parser
{
    internal class ParserService : IParserService
    {
        private readonly ArgumentSettings _argumentSettings;
        
        public ParserService(IConfigurationService configurationService)
        {
            _argumentSettings = configurationService.ArgumentSettings;
        }

        #region Metadata Parsing
        public YtdlpRawMetadata ParseYtdlpRawMetadata(string output)
        {
            if (string.IsNullOrEmpty(output))
            {
                throw new ArgumentException("Output cannot be null or empty.", nameof(output));
            }
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                var metadata = JsonSerializer.Deserialize<YtdlpRawMetadata>(output, options);
                if (metadata == null)
                {
                    throw new JsonException("Deserialization resulted in null.");
                }

                return metadata;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse Ytdlp raw metadata from output.", ex);
            }
            catch (Exception ex) when (ex is not JsonException)
            {
                throw new InvalidOperationException("An unexpected error occurred while parsing Ytdlp raw metadata.", ex);
            }
        }

        public MediaMetadataBase ParseMetadata(string output, Category category, string mediaFormat)
        {
            if (string.IsNullOrEmpty(mediaFormat))
            {
                switch (category.Type)
                {
                    case MediaType.Video:
                        mediaFormat = _argumentSettings.PreferredVideoFormat;
                        break;
                    case MediaType.Audio:
                        mediaFormat = _argumentSettings.PreferredAudioFormat;
                        break;
                    default:
                        mediaFormat = _argumentSettings.PreferredVideoFormat;
                        break;
                }
            }

            try
            {
                var metadata = ParseYtdlpRawMetadata(output);
                if (metadata == null)
                {
                    throw new JsonException("Deserialization resulted in null.");
                }

                var result = new MediaMetadataBase
                {
                    Id = metadata.Id,
                    Title = metadata.Title,
                    Url = metadata.Webpage_Url,
                    Uploader = metadata.Uploader,
                    UploadDate = metadata.Upload_Date,
                    PlaylistIndex = metadata.Playlist_Index,
                    Duration = metadata.Duration ?? 0,
                    ReleaseYear = metadata.Release_Year,
                    Artist = metadata.Artist,
                    Artists = metadata.Artists != null ? string.Join(", ", metadata.Artists) : null,
                    Genre = metadata.Genres != null ? string.Join(", ", metadata.Genres) : null,
                    CategoryId = category.Id,
                    MediaFormat = mediaFormat
                };

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred while parsing metadata.", ex);
            }
        }

        public MediaContainerBase ParseContainerMetadata(string output, List<string> entries, Category category, string mediaFormat)
        {
            if (string.IsNullOrEmpty(mediaFormat))
            {
                switch (category.Type)
                {
                    case MediaType.Video:
                        mediaFormat = _argumentSettings.PreferredVideoFormat;
                        break;
                    case MediaType.Audio:
                        mediaFormat = _argumentSettings.PreferredAudioFormat;
                        break;
                    default:
                        mediaFormat = _argumentSettings.PreferredVideoFormat;
                        break;
                }
            }

            try
            {
                var items = entries.Select(entry =>
                {
                    try
                    {
                        return ParseMetadata(entry, category, mediaFormat);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("An error occurred while parsing entry metadata.", ex);
                    }
                }).ToHashSet();

                var metadata = ParseYtdlpRawMetadata(output);
                if (metadata == null)
                {
                    throw new JsonException("Deserialization resulted in null.");
                }

                var container = new MediaContainerBase
                {
                    Id = metadata.Id,
                    Title = metadata.Title,
                    Url = metadata.Webpage_Url,
                    Album = metadata.Album,
                    Artist = metadata.Artist,
                    Artists = metadata.Artists != null ? string.Join(", ", metadata.Artists) : null,
                    Year = metadata.Release_Year,
                    Entries = items,
                };

                return container;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse container metadata from output.", ex);
            }
            catch (Exception ex) when (ex is not JsonException)
            {
                throw new InvalidOperationException("An unexpected error occurred while parsing container metadata.", ex);
            }
        }
        #endregion

        #region Progress Parsing
        private readonly Regex _progressRegex = new Regex
                ("^\\[PROGRESS\\]\\s+(?<downloaded_bytes>[\\d.]+(?:\\s*[KMGT]?B)?)\\s*\\|\\s*(?<total_bytes>[\\d.]+(?:\\s*[KMGT]?B)?)",
                RegexOptions.Compiled);

        public DownloadProgress? ParseProgressString(string progressString)
        {
            if (string.IsNullOrEmpty(progressString))
                return null;

            Match match = _progressRegex.Match(progressString);


            if (match.Success)
            {
                var downloadedBytesResult = long.TryParse(match.Groups["downloaded_bytes"].Value, out long downloadedBytes);
                var totalBytesResult = long.TryParse(match.Groups["total_bytes"].Value, out long totalBytes);

                if (!downloadedBytesResult || !totalBytesResult)
                {
                    return null;
                }

                return new DownloadProgress
                {
                    TotalBytes = totalBytesResult ? totalBytes : 0,
                    DownloadedBytes = downloadedBytesResult ? downloadedBytes : 0,
                    Status = "Downloading"
                };
            }

            return null;
        }
        #endregion
    }
}
