using FinalDownloader.Constants;
using FinalDownloader.Data;
using FinalDownloader.Data.Interface;
using FinalDownloader.Data.Repository;
using FinalDownloader.Models;
using FinalDownloader.Models.MediaMetadata;
using FinalDownloader.Models.Settings;
using FinalDownloader.Services.Arguments;
using FinalDownloader.Services.Configuration;
using FinalDownloader.Services.FileHandling;
using FinalDownloader.Services.Parser;
using FinalDownloader.Services.Process;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Download
{
    internal class DownloadService : IDownloadService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IProcessService _processService;
        private readonly IArgumentService _argumentService;
        private readonly IParserService _parserService;
        private readonly IFileHandlingService _fileHandlingService;
        private readonly DownloadSettings _downloadSettings;

        public bool ProcessingComplete { get; private set; } = false;
        public ConcurrentDictionary<string, DownloadProgress> DownloadProgress { get; private set; }
        private ConcurrentQueue<MediaMetadataBase> _downloadQueue;
        public ConcurrentDictionary<string, List<string>> DownloadErrors { get; private set; }

        public DownloadService(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            IProcessService processService,
            IArgumentService argumentService,
            IParserService parserService,
            IFileHandlingService fileHandlingService,
            IConfigurationService configurationService,
            IMediaMetadataBaseRepository mediaMetadataBaseRepository)
        {
            _dbContextFactory = dbContextFactory;
            _processService = processService;
            _argumentService = argumentService;
            _parserService = parserService;
            _fileHandlingService = fileHandlingService;

            DownloadProgress = new ConcurrentDictionary<string, DownloadProgress>();
            _downloadQueue = new ConcurrentQueue<MediaMetadataBase>();
            DownloadErrors = new ConcurrentDictionary<string, List<string>> { };
            _downloadSettings = configurationService.DownloadSettings;
        }

        #region Download Queue
        public void EnqueueDownload(MediaMetadataBase mediaMetadataBase)
        {
            if (mediaMetadataBase != null)
                _downloadQueue.Enqueue(mediaMetadataBase);
            else
                throw new ArgumentNullException(nameof(mediaMetadataBase));
        }

        public void EnqueueDownloads(IEnumerable<MediaMetadataBase> metadataList)
        {
            if (metadataList == null)
                throw new ArgumentNullException(nameof(metadataList));

            var count = metadataList.Count();

            foreach (var download in metadataList)
            {
                download.DownloadIndex = count;
                count--;
                EnqueueDownload(download);
            }
        }

        private IEnumerable<MediaMetadataBase> DequeueDownloads()
        {
            while (_downloadQueue.TryDequeue(out var download))
            {
                yield return download;
            }
        }

        public async Task ProcessDownloadQueue(ArgumentSettings argumentSettings, CancellationToken cancellationToken = default)
        {
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _downloadSettings.MaxConcurrentDownloads
            };

            ProcessingComplete = false;

            try
            {
                await Parallel.ForEachAsync(DequeueDownloads(), options, async (metadata, ct) =>
                {
                    try
                    {
                        bool success;

                        if (argumentSettings.Progress)
                            success = await DownloadWithProgressAsync(metadata, argumentSettings, ct);
                        //success = true;
                        else
                            success = await DownloadAsync(metadata, argumentSettings, ct);
                        //success = true;

                        if (success)
                            await _fileHandlingService.MoveDownloadToDestination(metadata);
                    }
                    catch
                    { }
                });
            }
            finally
            {
                await _fileHandlingService.CleanupTemporaryFilesAsync();
                ProcessingComplete = true;
            }
        }
        #endregion

        private async Task<bool> DownloadAsync(MediaMetadataBase metadata, ArgumentSettings argumentSettings, CancellationToken cancellationToken = default)
        {
            var errorList = new List<string>();

            var downloadPath = Path.Combine(_downloadSettings.TemporaryDirectory, metadata.Id);
            Directory.CreateDirectory(downloadPath);

            var processData = new ProcessData
            {
                ToolName = "yt-dlp.exe",
                Arguments = _argumentService.GetDownloadArguments(downloadPath, metadata.Url, argumentSettings),
                CreateNewWindow = false,
            };

            try
            {
                var existingMetadata = await new MediaMetadataRepository(_dbContextFactory.CreateDbContext()).GetByIdAsync(metadata.Id);

                if (existingMetadata != null)
                {
                    errorList.Add($"InvalidOperationException - Media with ID {metadata.Id} already exists in the repository. Skipping download for URL: {metadata.Url}");
                    throw new InvalidOperationException($"Media with ID {metadata.Id} already exists in the repository. Skipping download for URL: {metadata.Url}");
                }

                var processResult = await _processService.RunAsync(processData, cancellationToken);

                if (processResult.ExitCode != 0)
                {
                    errorList.Add($"InvalidOperationException - Download process exited with code {processResult.ExitCode} for URL: {metadata.Url}");
                    errorList.Add(processResult.Error!);
                    throw new InvalidOperationException($"Download process exited with code {processResult.ExitCode} for URL: {metadata.Url}");
                }

                await new MediaMetadataRepository(_dbContextFactory.CreateDbContext()).AddAsync(metadata, cancellationToken);
                DownloadErrors.TryAdd(metadata.Id, errorList);

                return true;
            }
            catch (Exception ex)
            {
                errorList.Add($"InvalidOperationException - An error occurred while downloading media from URL: {metadata.Url} - Message {ex.Message}");
                DownloadErrors.TryAdd(metadata.Id, errorList);

                return false;
            }
        }

        public async Task<bool> DownloadWithProgressAsync(MediaMetadataBase mediaMetadata, ArgumentSettings argumentSettings, CancellationToken cancellationToken = default)
        {
            var errorList = new List<string>();

            var currentProgress = new DownloadProgress
            {
                IsCompleted = false,
                Speed = string.Empty,
                Status = "Downloading"
            };

            var downloadPath = Path.Combine(_downloadSettings.TemporaryDirectory, mediaMetadata.Id);
            Directory.CreateDirectory(downloadPath);

            var outputCheck = new StreamWriter(Path.Combine(downloadPath, "progressoutput.txt"), append: true);
            var lineChecker = new StreamWriter(Path.Combine(downloadPath, "progressline.txt"), append: true);

            DownloadProgress.TryAdd(mediaMetadata.ProgressTitle, currentProgress);

            try
            {
                var processProgress = new Progress<string>(line =>
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        var downloadProgress = _parserService.ParseProgressString(line);
                        if (downloadProgress != null)
                        {
                            DownloadProgress.TryUpdate(mediaMetadata.ProgressTitle, downloadProgress, currentProgress);
                            outputCheck.WriteLine($"%:{downloadProgress.Percent}|" +
                                $"sp:{downloadProgress.Speed}|" +
                                $"eta:{downloadProgress.EstimatedTimeRemaining}|" +
                                $"db:{downloadProgress.DownloadedBytes}|");
                                //$"tb:{downloadProgress.TotalBytes}");
                            lineChecker.WriteLine(line);
                            currentProgress = downloadProgress;
                        }
                    }
                });

                var errorProgress = new Progress<string>(errorList.Add);

                var processData = new ProcessData
                {
                    ToolName = "yt-dlp.exe",
                    Arguments = _argumentService.GetDownloadArguments(downloadPath, mediaMetadata.Url, argumentSettings),
                    CreateNewWindow = false,
                };

                var existingMetadata = await new MediaMetadataRepository(_dbContextFactory.CreateDbContext()).GetByIdAsync(mediaMetadata.Id);

                if (existingMetadata != null)
                {
                    errorList.Add($"InvalidOperationException - Media with ID {mediaMetadata.Id} already exists in the repository. Skipping download for URL: {mediaMetadata.Url}");
                    throw new InvalidOperationException($"Media with ID {mediaMetadata.Id} already exists in the repository. Skipping download for URL: {mediaMetadata.Url}");
                }

                var processResult = await _processService.RunWithProgressAsync(processData, processProgress, errorProgress, cancellationToken);

                if (processResult.ExitCode != 0)
                {
                    errorList.Add($"InvalidOperationException - Download process exited with code {processResult.ExitCode} for URL: {mediaMetadata.Url}");
                    throw new InvalidOperationException($"Download process exited with code {processResult.ExitCode} for URL: {mediaMetadata.Url}");
                }

                await new MediaMetadataRepository(_dbContextFactory.CreateDbContext()).AddAsync(mediaMetadata, cancellationToken);
                DownloadErrors.TryAdd(mediaMetadata.Id, errorList);

                DownloadProgress.TryUpdate(
                    mediaMetadata.ProgressTitle,
                    new DownloadProgress
                    {
                        Status = "Completed",
                        Speed = string.Empty,
                        IsCompleted = true
                    }, currentProgress);

                return true;
            }
            catch (Exception ex)
            {
                DownloadProgress.TryUpdate(
                    mediaMetadata.ProgressTitle,
                    new DownloadProgress
                    {
                        Status = "Failed",
                        Speed = string.Empty,
                        IsCompleted = true
                    }, currentProgress);

                DownloadErrors.TryAdd(mediaMetadata.Id, errorList);
                AnsiConsole.MarkupLine($"  [red]✗[/] Failed to download {mediaMetadata.ProgressTitle}: {ex.Message}");

                return false;
            }
            finally
            {
                outputCheck.Close();
                lineChecker.Close();
            }
        }
    }
}
