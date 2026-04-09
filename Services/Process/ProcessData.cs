using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Process
{
    internal class ProcessData
    {
        public string ToolName { get; set; }
        public string Arguments { get; set; }
        public string? RedirectOutputFilePath { get; set; }
        public string? RedirectErrorFilePath { get; set; }
        public bool CreateNewWindow { get; set; } = true;
    }
}
