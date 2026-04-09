using FinalDownloader.Models.MediaMetadata;
using FinalDownloader.Models.Settings;
using FinalDownloader.Services.Arguments;
using FinalDownloader.Services.Configuration;
using FinalDownloader.Services.Download;
using FinalDownloader.Services.Parser;
using FinalDownloader.Services.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.MetadataRetrieval
{
    internal class MetadataService : IMetadataService
    {
        private readonly IParserService _parserService;
        private readonly IArgumentService _argumentService;
        private readonly IProcessService _processService;
        private readonly DownloadSettings _downloadSettings;

        public MetadataService(
            IParserService parserService, 
            IArgumentService argumentService, 
            IProcessService processService,
            IConfigurationService configurationService)
        {
            _parserService = parserService;
            _argumentService = argumentService;
            _processService = processService;
            _downloadSettings = configurationService.DownloadSettings;

        }

        public async Task<MediaContainerBase?> GetContainerMetadataAsync(DownloadData downloadData, CancellationToken cancellationToken = default)
        {
            var playlistMetadataFP = Path.Combine(_downloadSettings.TemporaryDirectory, "playlist_metadata.json");
            var playlistErrorFP = Path.Combine(_downloadSettings.TemporaryDirectory, "playlist_error.txt");
            var entriesFP = Path.Combine(_downloadSettings.TemporaryDirectory, "playlist_entries_metadata.json");
            var entriesErrorFP = Path.Combine(_downloadSettings.TemporaryDirectory, "playlist_entries_error.txt");

            if (!Directory.Exists(_downloadSettings.TemporaryDirectory))
                Directory.CreateDirectory(_downloadSettings.TemporaryDirectory);

            var playlistProcess = new ProcessData
            {
                ToolName = "yt-dlp.exe",
                Arguments = _argumentService.GetPlaylistMetadataArguments(downloadData.Url),
                RedirectOutputFilePath = playlistMetadataFP,
                RedirectErrorFilePath = playlistErrorFP,
                CreateNewWindow = false
            };


            var entriesProcess = new ProcessData
            {
                ToolName = "yt-dlp.exe",
                Arguments = _argumentService.GetPlaylistEntriesMetadataArguments(downloadData.Url),
                RedirectOutputFilePath = entriesFP,
                RedirectErrorFilePath = entriesErrorFP,
                CreateNewWindow = false
            };

            try
            {
                var processResult = await _processService.RunAsync(playlistProcess, cancellationToken);
                var entriesProcessResult = await _processService.RunAsync(entriesProcess, cancellationToken);

                if (processResult.ExitCode != 0)
                {
                    var errorOutput = await File.ReadAllTextAsync(playlistErrorFP, cancellationToken);
                    throw new InvalidOperationException($"Process exited with code {processResult.ExitCode}. Error output: {errorOutput}");
                }

                if (entriesProcessResult.ExitCode != 0)
                {
                    var errorOutput = await File.ReadAllTextAsync(entriesErrorFP, cancellationToken);
                    throw new InvalidOperationException($"Process exited with code {entriesProcessResult.ExitCode}. Error output: {errorOutput}");
                }

                var playlistMetadataOutput = await File.ReadAllLinesAsync(playlistMetadataFP, cancellationToken);
                var entriesMetadataOutput = await File.ReadAllLinesAsync(entriesFP, cancellationToken);

                var metadata = _parserService.ParseContainerMetadata(
                    playlistMetadataOutput.First(), 
                    entriesMetadataOutput.ToList(), 
                    downloadData.Category, 
                    downloadData.Format);

                return metadata;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while running the process to get container metadata.", ex);
            }
        }

        public async Task<MediaMetadataBase?> GetMetadataAsync(DownloadData downloadData, CancellationToken cancellationToken = default)
        {
            var metadataFP = Path.Combine(_downloadSettings.TemporaryDirectory, "metadata.json");
            var errorFP = Path.Combine(_downloadSettings.TemporaryDirectory, "metadata_error.txt");

            if (!Directory.Exists(_downloadSettings.TemporaryDirectory))
                Directory.CreateDirectory(_downloadSettings.TemporaryDirectory);

            var processData = new ProcessData
            {
                ToolName = "yt-dlp.exe",
                Arguments = _argumentService.GetSingleMetadataArguments(downloadData.Url),
                RedirectOutputFilePath = metadataFP,
                RedirectErrorFilePath = errorFP,
                CreateNewWindow = false
            };

            try
            {
                var processResult = await _processService.RunAsync(processData, cancellationToken);

                if (processResult.ExitCode != 0)
                {
                    var errorOutput = await File.ReadAllTextAsync(errorFP, cancellationToken);
                    throw new InvalidOperationException($"Process exited with code {processResult.ExitCode}. Error output: {errorOutput}");
                }

                var metadataOutput = await File.ReadAllLinesAsync(metadataFP, cancellationToken);
                var metadata = _parserService.ParseMetadata(metadataOutput.First(), downloadData.Category, downloadData.Format);

                return metadata;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while running the process to get metadata.", ex);
            }
        }
    }
}
