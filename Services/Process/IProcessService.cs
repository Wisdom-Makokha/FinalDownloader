using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Process
{
    internal interface IProcessService
    {
        Task<ProcessResult> RunAsync(ProcessData processData, CancellationToken cancellationToken = default);
        Task<ProcessResult> RunWithProgressAsync(ProcessData processData, IProgress<string> processPrgress, IProgress<string> errorProgress, CancellationToken cancelToken = default);
    }
}
