using FinalDownloader.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Arguments
{
    internal interface IArgumentService
    {
        string GetSingleMetadataArguments(string url);
        string GetPlaylistMetadataArguments(string url);
        string GetPlaylistEntriesMetadataArguments(string url);
        string GetDownloadArguments(string temporaryDirectory, string url, ArgumentSettings argumentSettings);
        string GetUpdateArguments();
    }
}
