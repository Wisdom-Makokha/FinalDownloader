using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Process
{
    internal class ProcessService : IProcessService
    {
        public async Task<ProcessResult> RunAsync(ProcessData processData, CancellationToken cancellationToken = default)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var process = CreateProcess(processData);
            var processResult = new ProcessResult(
                elapsedTime: new TimeSpan(0, 0, 0),
                exitCode: 0,
                output: string.Empty,
                error: string.Empty);

            var redirectOutput = !string.IsNullOrEmpty(processData.RedirectOutputFilePath);
            var redirectError = !string.IsNullOrEmpty(processData.RedirectErrorFilePath);
            var stopwatch = new Stopwatch();

            StreamWriter? outputWriter = null;
            StreamWriter? errorWriter = null;

            try
            {
                using (linkedCts.Token.Register(() =>
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        processResult.Error += ex.Message + " ---> ";
                    }
                }
                ))
                {
                    if (redirectOutput)
                    {
                        // remove if exists
                        if (File.Exists(processData.RedirectOutputFilePath))
                            File.Delete(processData.RedirectOutputFilePath);

                        outputWriter = new StreamWriter(processData.RedirectOutputFilePath!, append: true);
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data != null) outputWriter.WriteLine(e.Data);
                        };
                    }

                    if (redirectError)
                    {
                        // remove if exists
                        if (File.Exists(processData.RedirectErrorFilePath))
                            File.Delete(processData.RedirectErrorFilePath);

                        errorWriter = new StreamWriter(processData.RedirectErrorFilePath!, append: true);
                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data != null) errorWriter.WriteLine(e.Data);
                        };
                    }

                    stopwatch.Start();
                    process.Start();

                    if (redirectOutput) process.BeginOutputReadLine();
                    else processResult.Output = await process.StandardOutput.ReadToEndAsync();

                    if (redirectError) process.BeginErrorReadLine();
                    else processResult.Error = await process.StandardError.ReadToEndAsync();

                    await Task.WhenAll(process.WaitForExitAsync(linkedCts.Token));
                    linkedCts.Token.ThrowIfCancellationRequested();
                    
                    processResult.ExitCode = process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                processResult.Error += ex.Message + " ---> ";
            }
            finally
            {
                stopwatch.Stop();
                processResult.ElapsedTime = stopwatch.Elapsed;

                if (outputWriter != null)
                    await outputWriter.DisposeAsync();

                if (errorWriter != null)
                    await errorWriter.DisposeAsync();
            }

            return processResult;
        }

        public async Task<ProcessResult> RunWithProgressAsync(
            ProcessData processData,
            IProgress<string> processProgress,
            IProgress<string> errorProgress,
            CancellationToken cancelToken = default)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
            var process = CreateProcess(processData);
            var processResult = new ProcessResult(
                exitCode: 0,
                elapsedTime: new TimeSpan(0, 0, 0),
                output: null,
                error: null);

            var stopwatch = new Stopwatch();

            try
            {
                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null) processProgress.Report(args.Data);
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null) processProgress.Report(args.Data);
                };

                using (linkedCts.Token.Register(() =>
                {
                    try
                    {
                        if (!process.HasExited) { process.Kill(); }
                    }
                    catch (InvalidOperationException ex)
                    {
                        errorProgress.Report(ex.Message);
                    }
                }))
                {
                    process.Start();
                    stopwatch.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await process.WaitForExitAsync(linkedCts.Token);

                    process.CancelOutputRead();
                    process.CancelErrorRead();

                    linkedCts.Token.ThrowIfCancellationRequested();
                    processResult.ExitCode = process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                errorProgress.Report(ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                processResult.ElapsedTime = stopwatch.Elapsed;
            }

            return processResult;
        }

        private System.Diagnostics.Process CreateProcess(ProcessData processData)
        {
            return new System.Diagnostics.Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = processData.ToolName,
                    Arguments = processData.Arguments,
                    CreateNoWindow = processData.CreateNewWindow,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = false,
                    StandardInputEncoding = null,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                },
                // EnableRaisingEvents property tells the Process component to fire the Exited event
                // and to call the event handlers for OutputDataReceived and ErrorDataReceived.
                EnableRaisingEvents = true,
            };
        }
    }
}
