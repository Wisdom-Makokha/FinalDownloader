using FinalDownloader.Services.Download;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Progress
{
    internal class ProgressService : IProgressService
    {
        private readonly ConcurrentDictionary<string, ProgressTask> _progressDictionary = new ConcurrentDictionary<string, ProgressTask>();
        private ProgressContext? _progressContext = null;

        public async Task InitializeProgressTrackerAsync(Func<ProgressContext, Task> initializeAction)
        {
            await AnsiConsole.Progress()
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new SpinnerColumn(),
            })
            .HideCompleted(false)
            .StartAsync(async context =>
            {
                _progressContext = context;
                await initializeAction(context);
            });
        }

        public void CreateTask(string mediaTitle)
        {
            if (_progressContext == null)
            {
                throw new InvalidOperationException("Progress context is not initialized. Ensure InitializeProgressTrackerAsync has been called.");
            }

            var task = _progressContext.AddTask(mediaTitle, maxValue: 100);
            _progressDictionary[mediaTitle] = task;
        }

        public void CompleteTask(string mediaTitle, DownloadProgress progress)
        {
            if (_progressDictionary.TryRemove(mediaTitle, out var task))
            {
                task.Value = 100;
                task.Description = $"[{progress.StatusColor}]{progress.Status} [/]-[cyan] {mediaTitle}[/]";
                task.StopTask();
            }
        }

        public void FailTask(string mediaTitle, string errorMessage, DownloadProgress progress)
        {
            if (_progressDictionary.TryRemove(mediaTitle, out var task))
            {
                task.Description = $"[{progress.StatusColor}]{progress.Status}[/]" +
                    $"[Fuchsia]{errorMessage}[/]";
                task.StopTask();
            }
        }

        public void ReportProgress(string mediaTitle, DownloadProgress progress)
        {
            if (_progressDictionary.TryGetValue(mediaTitle, out var task))
            {
                task.Value = progress.Percent;
                task.Description = $"[{progress.StatusColor}]{progress.Status}[/] - {mediaTitle} " +
                $"| [grey]Percent: [/][white]{(progress.Percent.ToString() + "%"),-6}[/]" +
                $"| [grey]Speed: [/][white]{progress.Speed,-12}[/] " +
                $"| [grey]ETA: [/][white]{progress.EstimatedTimeRemaining,-6}[/]" +
                $"| [grey]Size: [/][white]{progress.DownloadSize}[/]";
            }
        }

        public void RefreshContext()
        {
            if (_progressContext == null)
            {
                throw new InvalidOperationException("Progress context is not initialized. Ensure InitializeProgressTrackerAsync has been called.");
            }

            _progressContext.Refresh();
        }
    }
}
