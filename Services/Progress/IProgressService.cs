using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Progress
{
    internal interface IProgressService
    {
        Task InitializeProgressTrackerAsync(Func<ProgressContext, Task> initializeAction);
        void CreateTask(string mediaTitle);
        void ReportProgress(string mediaTitle, DownloadProgress progress);
        void CompleteTask(string mediaTitle, DownloadProgress progress);
        void FailTask(string mediaTitle, string errorMessage, DownloadProgress progress);
        void RefreshContext();
    }
}
