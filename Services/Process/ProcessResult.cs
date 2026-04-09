using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Process
{
    internal class ProcessResult
    {
        public string? Output { get; set; }
        public string? Error { get; set; }
        public int ExitCode { get; set; }
        public TimeSpan ElapsedTime { get; set; }

        public ProcessResult(int exitCode, TimeSpan elapsedTime, string? output = "", string? error = "")
        {
            ExitCode = exitCode;
            ElapsedTime = elapsedTime;
            Output = output;
            Error = error;
        }
    }
}
