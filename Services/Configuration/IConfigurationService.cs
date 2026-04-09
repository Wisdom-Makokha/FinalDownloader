using FinalDownloader.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Configuration
{
    internal interface IConfigurationService
    {
        DownloadSettings DownloadSettings { get; }
        ArgumentSettings ArgumentSettings { get; }
        void Reload();
        Task SaveDownloadSettings(DownloadSettings downloadSettings);
        Task SaveArgumentSettings(ArgumentSettings argumentSettings);
    }
}
