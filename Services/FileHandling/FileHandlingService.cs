using FinalDownloader.Data.Interface;
using FinalDownloader.Models.MediaMetadata;
using FinalDownloader.Models.Settings;
using FinalDownloader.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.FileHandling
{
    internal class FileHandlingService : IFileHandlingService
    {
        private readonly DownloadSettings _downloadSettings;
        private readonly ArgumentSettings _argumentSettings;
        private readonly ICategoryRepository _categoryRepository;

        public FileHandlingService(IConfigurationService configurationService, ICategoryRepository categoryRepository)
        {
            _downloadSettings = configurationService.DownloadSettings;
            _argumentSettings = configurationService.ArgumentSettings;
            _categoryRepository = categoryRepository;
        }

        public async Task MoveDownloadToDestination(MediaMetadataBase metadata)
        {
            try
            {
                var sourcePath = Path.Combine(_downloadSettings.TemporaryDirectory, metadata.Id);
                var category = await _categoryRepository.GetByIdAsync(metadata.CategoryId);

                var destinationPath = category!.FolderPath;

                if (!Directory.Exists(category.FolderPath))
                    Directory.CreateDirectory(destinationPath);

                var files = Directory.GetFiles(sourcePath);
                var foundFile = string.Empty;

                switch (category.Type)
                {
                    case MediaType.Video:
                        foundFile = files.FirstOrDefault(f => f.EndsWith(_argumentSettings.PreferredVideoFormat, StringComparison.OrdinalIgnoreCase));
                        break;
                    case MediaType.Audio:
                        foundFile = files.FirstOrDefault(f => f.EndsWith(_argumentSettings.PreferredAudioFormat, StringComparison.OrdinalIgnoreCase));
                        break;
                    default:
                        foundFile = files.FirstOrDefault(f => f.EndsWith(_argumentSettings.PreferredVideoFormat, StringComparison.OrdinalIgnoreCase))
                         ?? files.FirstOrDefault(f => f.EndsWith(_argumentSettings.PreferredAudioFormat, StringComparison.OrdinalIgnoreCase));
                        break;
                }

                var mediaFileWithoutExtension = Path.GetFileNameWithoutExtension(foundFile);
                var extension = Path.GetExtension(foundFile);

                if (!string.IsNullOrEmpty(foundFile) && string.Equals(metadata.Id, mediaFileWithoutExtension))
                {
                    var destFile = Path.Combine(destinationPath, $"{metadata.Title}{extension}");

                    if (!File.Exists(destFile))
                        File.Move(foundFile, destFile);
                }
                else
                    throw new ArgumentException("Downloaded file not found");

                HandleSubtitleFiles(files.ToList(), destinationPath, metadata.Title);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while moving downloaded media to final location for media ID: {metadata.Id}", ex);
            }
        }

        public Task CleanupTemporaryFilesAsync()
        {
            try
            {
                if (_downloadSettings.DeleteTempFiles)
                {
                    var tempFiles = Directory.GetFiles(_downloadSettings.TemporaryDirectory);
                    foreach (var file in tempFiles)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            // Log the error but continue with cleanup
                            Console.Error.WriteLine($"Failed to delete temporary file: {file}. Error: {ex.Message}");
                        }
                    }

                    Directory.Delete(_downloadSettings.TemporaryDirectory, recursive: true);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while cleaning up temporary files.", ex);
            }
        }

        private void HandleSubtitleFiles(List<string> files, string destinationFolder, string destinationFileName)
        {
            if (_argumentSettings.Subtitles)
            {
                var subtitleFile = files.FirstOrDefault(f => f.EndsWith(_argumentSettings.PreferredSubtitleFormat, StringComparison.OrdinalIgnoreCase));
                if (subtitleFile != null)
                {
                    var extension = Path.GetExtension(subtitleFile);
                    var destinationFile = Path.Combine(destinationFolder, $"{destinationFileName}{extension}");

                    if (!File.Exists(destinationFile))
                        File.Move(subtitleFile, destinationFile);
                }
            }
        }
    }
}
