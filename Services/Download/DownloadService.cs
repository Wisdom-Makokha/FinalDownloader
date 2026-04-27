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
using FinalDownloader.Services.MetadataRetrieval;
using FinalDownloader.Services.Parser;
using FinalDownloader.Services.Process;
using FinalDownloader.Services.Progress;
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
        private readonly IMetadataService _metadataService;
        private readonly DownloadSettings _downloadSettings;
        private readonly IProgressService _progressService;

        public bool ProcessingComplete { get; private set; } = false;
        private ConcurrentQueue<MediaMetadataBase> _downloadQueue;
        public ConcurrentDictionary<string, List<string>> DownloadErrors { get; private set; }

        public DownloadService(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            IProcessService processService,
            IArgumentService argumentService,
            IParserService parserService,
            IFileHandlingService fileHandlingService,
            IProgressService progressService,
            IConfigurationService configurationService,
            IMetadataService metadataService,
            IMediaMetadataBaseRepository mediaMetadataBaseRepository)
        {
            _dbContextFactory = dbContextFactory;
            _processService = processService;
            _argumentService = argumentService;
            _parserService = parserService;
            _fileHandlingService = fileHandlingService;
            _progressService = progressService;
            _metadataService = metadataService;

            _downloadQueue = new ConcurrentQueue<MediaMetadataBase>();
            DownloadErrors = new ConcurrentDictionary<string, List<string>> { };
            _downloadSettings = configurationService.DownloadSettings;
        }

        #region Queuing
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
        #endregion

        #region Queue Processing
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
                AnsiConsole.MarkupLine($"[green]Starting downloads...[/]");

                // for when downloads are less than the max degree of parallelism
                if (_downloadQueue.Count <= _downloadSettings.MaxConcurrentDownloads)
                {
                    var downloads = DequeueDownloads().Select(async item =>
                    {
                        bool success = await DownloadAsync(item, argumentSettings, cancellationToken);
                        if (success)
                            await _fileHandlingService.MoveDownloadToDestination(item);
                    });

                    await Task.WhenAll(downloads);
                }
                else
                {
                    await Parallel.ForEachAsync(DequeueDownloads(), options, async (metadata, ct) =>
                    {
                        try
                        {
                            bool success = await DownloadAsync(metadata, argumentSettings, ct);

                            if (success)
                                await _fileHandlingService.MoveDownloadToDestination(metadata);
                        }
                        catch
                        { }
                    });
                }
            }
            finally
            {
                await _fileHandlingService.CleanupTemporaryFilesAsync();
                ProcessingComplete = true;
            }
        }

        public async Task ProcessDownloadQueueWithProgress(ArgumentSettings argumentSettings, CancellationToken cancellationToken = default)
        {
            var throttler = new SemaphoreSlim(_downloadSettings.MaxConcurrentDownloads, _downloadSettings.MaxConcurrentDownloads + 2);
            int completedCount = 0;

            ProcessingComplete = false;
            try
            {
                AnsiConsole.MarkupLine($"[green]Starting downloads with progress tracking...[/]");
                await _progressService.InitializeProgressTrackerAsync(async context =>
                {
                    var tasks = new List<Task>();
                    ProgressTask? overallTask = null;

                    if (_downloadQueue.Count > 1)
                    {
                        overallTask = context.AddTask("Overall Progress", maxValue: _downloadQueue.Count());
                        overallTask.Value = 0;
                    }

                    foreach (var metadata in DequeueDownloads())
                    {
                        await throttler.WaitAsync(cancellationToken);
                        var task = Task.Run(async () =>
                        {
                            try
                            {
                                bool success = await DownloadWithProgressAsync(metadata, argumentSettings, cancellationToken);
                                if (success)
                                    await _fileHandlingService.MoveDownloadToDestination(metadata);
                            }
                            catch
                            { }
                            finally
                            {
                                throttler.Release();
                                completedCount++;

                                if (overallTask != null)
                                {
                                    overallTask.Value = completedCount;
                                }
                            }
                        }, cancellationToken);

                        tasks.Add(task);
                    }
                    await Task.WhenAll(tasks);

                    if (overallTask != null)
                    {
                        overallTask.Value = _downloadQueue.Count();
                        overallTask.StopTask();
                    }
                });
            }
            finally
            {
                await _fileHandlingService.CleanupTemporaryFilesAsync();
                ProcessingComplete = true;
            }
        }
        #endregion

        #region Single Download
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

                try
                {
                    metadata = _metadataService.UpdateMetadataWithDownloadInfo(metadata, downloadPath);
                }
                catch
                (Exception ex)
                {
                    errorList.Add($"Metadata Update Error - An error occurred while updating metadata for URL: {metadata.Url} - Message {ex.Message}");
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

        public async Task<bool> DownloadWithProgressAsync(MediaMetadataBase metadata, ArgumentSettings argumentSettings, CancellationToken cancellationToken = default)
        {
            var errorList = new List<string>();

            _progressService.CreateTask(metadata.ProgressTitle);

            var downloadPath = Path.Combine(_downloadSettings.TemporaryDirectory, metadata.Id);
            Directory.CreateDirectory(downloadPath);

            var outputCheck = new StreamWriter(Path.Combine(downloadPath, "progressoutput.txt"), append: true);
            var lineChecker = new StreamWriter(Path.Combine(downloadPath, "progressline.txt"), append: true);

            try
            {
                var processProgress = new Progress<string>(line =>
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        var currentProgress = _parserService.ParseProgressString(line);
                        if (currentProgress != null)
                        {
                            _progressService.ReportProgress(metadata.ProgressTitle, currentProgress);

                            outputCheck.WriteLine($"%:{currentProgress.Percent}|" +
                                $"sp:{currentProgress.Speed}|" +
                                $"eta:{currentProgress.EstimatedTimeRemaining}|" +
                                $"db:{currentProgress.DownloadedBytes}|");
                            //$"tb:{currentProgress.TotalBytes}");
                            lineChecker.WriteLine(line);
                        }
                    }
                });

                var errorProgress = new Progress<string>(errorList.Add);

                var processData = new ProcessData
                {
                    ToolName = "yt-dlp.exe",
                    Arguments = _argumentService.GetDownloadArguments(downloadPath, metadata.Url, argumentSettings),
                    CreateNewWindow = false,
                };

                if (await new MediaMetadataRepository(_dbContextFactory.CreateDbContext()).GetByIdAsync(metadata.Id) != null)
                {
                    errorList.Add($"InvalidOperationException - Media with ID {metadata.Id} already exists in the repository. Skipping download for URL: {metadata.Url}");
                    throw new InvalidOperationException($"Media with ID {metadata.Id} already exists in the repository. Skipping download for URL: {metadata.Url}");
                }

                var processResult = await _processService.RunWithProgressAsync(processData, processProgress, errorProgress, cancellationToken);

                if (processResult.ExitCode != 0)
                {
                    errorList.Add($"InvalidOperationException - Download process exited with code {processResult.ExitCode} for URL: {metadata.Url}");
                    throw new InvalidOperationException($"Download process exited with code {processResult.ExitCode} for URL: {metadata.Url}");
                }

                try
                {
                    metadata = _metadataService.UpdateMetadataWithDownloadInfo(metadata, downloadPath);
                }
                catch (Exception ex)
                {
                    errorList.Add($"Metadata Update Error - An error occurred while updating metadata for URL: {metadata.Url} - Message {ex.Message}");
                }

                await new MediaMetadataRepository(_dbContextFactory.CreateDbContext()).AddAsync(metadata, cancellationToken);
                DownloadErrors.TryAdd(metadata.Id, errorList);

                _progressService.CompleteTask(
                    metadata.ProgressTitle,
                    new DownloadProgress
                    {
                        Status = "Completed",
                        Speed = string.Empty,
                        IsCompleted = true
                    });

                return true;
            }
            catch (Exception ex)
            {
                errorList.Add($"InvalidOperationException - An error occurred while downloading media from URL: {metadata.Url} - Message {ex.Message}");
                DownloadErrors.TryAdd(metadata.Id, errorList);

                _progressService.RefreshContext();
                _progressService.FailTask(
                    metadata.ProgressTitle,
                    $"✗ Failed to download {metadata.ProgressTitle}: {ex.Message}",
                    new DownloadProgress
                    {
                        Status = "Failed",
                        Speed = string.Empty,
                        IsCompleted = true
                    });
                return false;
            }
            finally
            {
                outputCheck.Close();
                lineChecker.Close();
            }
        }
        #endregion
    }
}
