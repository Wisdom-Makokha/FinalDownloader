using ConsoleStackNavigation.BaseScreens;
using ConsoleStackNavigation.NavigationSystem;
using ConsoleStackNavigation.NavigationSystem.Interface;
using FinalDownloader.Display.ScreenData;
using FinalDownloader.Services.Download;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FinalDownloader.Display.Screens.Download
{
    internal class DownloadProgressScreen : ScreenBase<ProgressScreenData>
    {
        private readonly IDownloadService _downloadService;

        public DownloadProgressScreen(IDownloadService downloadService)
        {
            _downloadService = downloadService;
        }

        protected override async Task<NavigationResult> DisplayWithDataAsync(ProgressScreenData progressScreenData)
        {
            Console.Clear();
            AnsiConsole.MarkupLine("Initialising progress tracker... ");
            AnsiConsole.MarkupLine($"Download Queue count: {progressScreenData.DownloadItems.Count()}");
            await AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new SpinnerColumn(),
                })
                .HideCompleted(false)
                .StartAsync(async context =>
                {
                    //await _downloadService.ProcessDownloadQueue(progressScreenData.ArgumentSettings, progressScreenData.CancellationToken, context);
                    _downloadService.EnqueueDownloads(progressScreenData.DownloadItems);

                    var downloadTask = _downloadService.ProcessDownloadQueue(progressScreenData.ArgumentSettings, progressScreenData.CancellationToken);

                    var mainTask = context.AddTask("[bold white] Overall progress[/]");
                    mainTask.MaxValue = progressScreenData.DownloadItems.Count();
                    mainTask.Value = 0;
                    int completedCount = 0;

                    var tasks = new Dictionary<string, ProgressTask>();

                    while (!_downloadService.ProcessingComplete && !progressScreenData.CancellationToken.IsCancellationRequested)
                    {
                        //AnsiConsole.MarkupLine("Taking snapshot... ");
                        var snapshot = _downloadService.DownloadProgress.ToArray();
                        mainTask.Value = completedCount;

                        foreach (var entry in snapshot)
                        {
                            var key = entry.Key;
                            var info = entry.Value;

                            if (!tasks.TryGetValue(key, out var task))
                            {
                                task = context.AddTask(key);
                                task.MaxValue = 100;
                                task.Value = 0;
                                tasks.Add(key, task);
                            }

                            task.Value = info.Percent;
                            task.Description = $"[{info.StatusColor}]{info.Status}[/] - {key} " +
                            $"| [grey]Percent: [/][white]{info.Percent.ToString(), -4}[/]" +
                            $"| [grey]Speed: [/][white]{info.Speed, -6}[/]" +
                            $"| [grey]ETA: [/][white]{info.EstimatedTimeRemaining, -5}[/]" +
                            $"| [grey]Downloaded: [/][white]{info.DownloadSize,-7}[/]";
                            //$"| Total: {info.TotalSize}[/]";

                            if (info.IsCompleted && !task.IsFinished)
                            {
                                task.Description = $"[{info.StatusColor}]{info.Status} [/]-[cyan] {key}[/]";
                                completedCount++;
                                task.StopTask();
                            }
                        }

                        if (_downloadService.ProcessingComplete)
                        {
                            break;
                        }

                        await Task.Delay(50, progressScreenData.CancellationToken);
                    }

                    mainTask.StopTask();
                });

            await AnsiConsole.PromptAsync(new TextPrompt<string>("Press [green]Enter[/] to go back...").AllowEmpty());

            return new NavigationResult
            {
                NextScreenKey = string.Empty,
                ScreenAction = NavigationAction.Pop,
                Data = null,
                MenuScreenCustomizationData = null,
            };
        }
    }
}
