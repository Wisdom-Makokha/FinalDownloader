using FinalDownloader.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Arguments
{
    internal class ArgumentService : IArgumentService
    {
        private string _progressTemplate = "\"[PROGRESS] %(progress._percent_str)s | %(progress._speed_str)s | %(progress._eta_str)s | %(progress.downloaded_bytes)s\"";

        public string GetDownloadArguments(string temporaryDirectory, string url, ArgumentSettings argumentSettings)
        {
            var args = new List<string>
            {
                "--windows-filename",
                $"--output \"{temporaryDirectory}\\%(id)s.%(ext)s\"",
            };

            if (argumentSettings.AudioOnly)
            {
                args.Add("--extract-audio");
                args.Add($"--audio-format {argumentSettings.PreferredAudioFormat}");
                args.Add("--audio-quality 0");
            }
            else
            {
                args.Add($"--merge-output-format {argumentSettings.PreferredVideoFormat}");
                args.Add($"--format \"bestvideo[height<={argumentSettings.PreferredResolution}]+bestaudio/best\"");
            }

            if (!argumentSettings.Progress)
            {
                args.Add("--no-progress");
            }
            else
            {
                args.Add("--progress");
                args.Add($"--progress-template {_progressTemplate}");
            }

            if (argumentSettings.Subtitles)
            {
                args.Add("--write-sub");
                args.Add("--write-auto-sub");
                args.Add("--sub-lang en");
                args.Add($"--convert-subs {argumentSettings.PreferredSubtitleFormat}");
                args.Add($"--sub-format {argumentSettings.PreferredSubtitleFormat}");
            }

            if (argumentSettings.EmbedSubtitles)
            {
                args.Add("--embed-subs");
            }

            if (argumentSettings.EmbedMetadata)
            {
                args.Add("--embed-metadata");
            }

            if (argumentSettings.Thumbnail)
            {
                args.Add("--write-thumbnail");
                args.Add("--convert-thumbnails png");
                args.Add("--embed-thumbnail");
            }

            args.Add($"--retries {argumentSettings.MaxRetries}");

                args.Add("--quiet");
                args.Add("--no-warnings");

            if (argumentSettings.Simulate)
            {
                args.Add("--simulate");
            }

            args.Add($"\"{url}\"");

            return string.Join(" ", args);
        }

        public string GetPlaylistMetadataArguments(string url)
        {
            // if temporary directory is null or empty, dump the json onto the console and it wiil be captured there
            // otherwise yt-dlp will handle writing to a .info.json file on the provided output file path
            var args = new List<string>
                {
                    "--dump-single-json",
                    "--no-warnings",
                    "--ignore-errors",
                    "--quiet",
                    "--no-cache-dir",
                    "--flat-playlist",
                    $"\"{url}\""
                };

            return string.Join(" ", args);
        }

        public string GetPlaylistEntriesMetadataArguments(string url)
        {
            var args = new List<string>
                {
                    "--dump-json",
                    "--no-warnings",
                    "--ignore-errors",
                    "--quiet",
                    "--no-cache-dir",
                    "--flat-playlist",
                    $"\"{url}\""
                };

            return string.Join(" ", args);
        }

        public string GetSingleMetadataArguments(string url)
        {
            // if temporary directory is null or empty, dump the json onto the console and it wiil be captured there
            // otherwise yt-dlp will handle writing to a .info.json file on the provided output file path
            var args = new List<string>
            {
                "--dump-json",
                "--no-warnings",
                "--ignore-errors",
                "--quiet",
                "--no-cache-dir",
                "--no-playlist",
                "--no-download",
                $"\"{url}\""
            };

            return string.Join(" ", args);
        }

        public string GetUpdateArguments()
        {
            return "--update";
        }
    }
}
