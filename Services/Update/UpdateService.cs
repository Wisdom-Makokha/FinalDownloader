using FinalDownloader.Services.Arguments;
using FinalDownloader.Services.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Update
{
    internal class UpdateService : IUpdateService
    {
        private readonly IProcessService _processService;
        private readonly IArgumentService _argumentService;

        public UpdateService(IProcessService processService, IArgumentService argumentService)
        {
            _processService = processService;
            _argumentService = argumentService;
        }

        public async Task<bool> UpdateToolAsync()
        {
            // check for updates for yt-dlp and ffmpeg
            // if there is an update, download and install it
            // return true if there was an update, false otherwise
            var updateArguments = _argumentService.GetUpdateArguments();
            var processData = new ProcessData
            {
                ToolName = "yt-dlp.exe",
                Arguments = updateArguments,
                RedirectOutputFilePath = null,
                RedirectErrorFilePath = null,
            };

            var result = await _processService.RunAsync(processData);
            if (result.ExitCode == 0)
            {
                return true;
            }

            return false;
        }
    }
}
