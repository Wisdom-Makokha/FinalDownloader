using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Models.Settings
{
    internal class ApplicationSettings
    {
        public string TemporaryDirectory { get; set; }
        public bool DeleteTempFiles { get; set; }
        public int MaxConcurrentDownloads { get; set; }
    }
}
