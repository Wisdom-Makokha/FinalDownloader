using ConsoleStackNavigation.NavigationSystem;
using ConsoleStackNavigation.NavigationSystem.Interface;
using FinalDownloader.Constants;
using FinalDownloader.Data.Interface;
using FinalDownloader.Display.ScreenData;
using FinalDownloader.Display.Screens.Category;
using FinalDownloader.Models.MediaMetadata;
using FinalDownloader.Models.Settings;
using FinalDownloader.Services.Configuration;
using FinalDownloader.Services.Download;
using FinalDownloader.Services.MetadataRetrieval;
using FinalDownloader.Services.Process;
using FinalDownloader.Services.Update;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display.Screens.Download
{
    internal class DownloadScreen : IScreen
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDownloadService _downloadService;
        private readonly IMetadataService _metadataService;
        private readonly ArgumentSettings _argumentSettings;
        private readonly IMediaContainerBaseRepository _mediaContainerBaseRepository;

        public DownloadScreen(
            ICategoryRepository categoryRepository,
            IDownloadService downloadService,
            IMetadataService metadataService,
            IMediaContainerBaseRepository mediaContainerBaseRepository,
            IConfigurationService configurationService)
        {
            _categoryRepository = categoryRepository;
            _downloadService = downloadService;
            _metadataService = metadataService;
            _mediaContainerBaseRepository = mediaContainerBaseRepository;
            _argumentSettings = configurationService.ArgumentSettings;

        }

        public async Task<NavigationResult> DisplayAsync(NavigationContext navigationContext)
        {
            Console.Clear();

            var downloadData = new DownloadData();
            var downloadList = new List<MediaMetadataBase>();
            using var cts = new CancellationTokenSource();

            // Get default category and list of category names
            var (category, categories) = await GetCategoryAndListAsync(cts.Token);
            categories.Add("##Create New Category##");

            if (category == null)
            {
                AnsiConsole.MarkupLine("[yellow]No default category set. Please select a category for the download.[/]");

                if (categories.Count > 0)
                {
                    var selectedCategoryName = await PromptForCategorySelectionAsync(categories);
                    
                    if (selectedCategoryName == "##Create New Category##")
                    {
                        return new NavigationResult
                        {
                            ScreenAction = NavigationAction.Replace,
                            NextScreenKey = ScreenNames.AddCategoryScreen,
                            Data = null
                        };
                    }

                    category = await _categoryRepository.GetByNameAsync(selectedCategoryName);

                    await _categoryRepository.SetDefaultCategoryAsync(category!.Name, cts.Token);
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]No categories available. Please create a category before downloading.[/]");

                    return new NavigationResult
                    {
                        ScreenAction = NavigationAction.Replace,
                        NextScreenKey = ScreenNames.AddCategoryScreen,
                        Data = null
                    };
                }
            }
            else
            {
                AnsiConsole.MarkupLine($"Default category is [green]{category.Name}[/].");
                var useDefaultOptions = await AnsiConsole.ConfirmAsync("Use default options?");

                if (!useDefaultOptions)
                {
                    if (categories.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[yellow]No categories available. Please create a category before downloading.[/]");

                        return new NavigationResult
                        {
                            ScreenAction = NavigationAction.Replace,
                            NextScreenKey = ScreenNames.AddCategoryScreen,
                            Data = null
                        };
                    }

                    var selectedCategoryName = await PromptForCategorySelectionAsync(categories);
                    if (selectedCategoryName == "##Create New Category##")
                    {
                        return new NavigationResult
                        {
                            ScreenAction = NavigationAction.Replace,
                            NextScreenKey = ScreenNames.AddCategoryScreen,
                            Data = null
                        };
                    }
                    else
                        category = await _categoryRepository.GetByNameAsync(selectedCategoryName);
                }
            }

            Console.Write("\n");
            AnsiConsole.MarkupLine($"Selected category: [green]{category!.Name}[/]");
            CategoryDetailsScreen.DisplayDetails(category!);

            Console.Write("\n");

            ApplyCategorySettings(category!, downloadData);

            downloadData.Url = await PromptForUrlAsync();
            downloadData.IsPlaylist = await AnsiConsole.ConfirmAsync("Is this a playlist?");

            if (downloadData.IsPlaylist)
            {
                downloadList.AddRange(await HandlePlaylistAsync(downloadData, cts.Token));
            }
            else
            {
                downloadList.Add(await HandleSingleMediaAsync(downloadData, cts.Token));
            }

            await AnsiConsole.PromptAsync(
                new TextPrompt<string>("[grey]Press [[Enter]] to go back...[/]")
                .AllowEmpty()
            );

            Console.Clear();
            Console.Write("\n");
            DownloadData.DisplayDownloadData(downloadData);
            Console.Write("\n");

            if (_argumentSettings.Progress)
            {
                await _downloadService.ProcessDownloadQueueWithProgress(_argumentSettings, cts.Token);
            }
            else
            {
                await AnsiConsole.Status()
                .StartAsync("Downloading files....", async ctx =>
                {
                    await _downloadService.ProcessDownloadQueue(_argumentSettings, cts.Token);
                });
            }

            await AnsiConsole.PromptAsync(
                new TextPrompt<string>("[grey]Press [[Enter]] to go back...[/]")
                .AllowEmpty()
            );

            return new NavigationResult
            {
                ScreenAction = NavigationAction.Pop,
                NextScreenKey = string.Empty,
                Data = null
            };
        }

        #region CategorySelection
        private async Task<(Models.Category? Category, List<string> Categories)> GetCategoryAndListAsync(CancellationToken token)
        {
            var category = await _categoryRepository.GetDefaultCategoryAsync();
            var categories = (await _categoryRepository.GetAllCategoryNamesAsync()).ToList();
            return (category, categories);
        }

        private async Task<string> PromptForCategorySelectionAsync(List<string> categories)
        {
            var selectedCategoryName = await AnsiConsole.PromptAsync(
                new SelectionPrompt<string>()
                    .Title("Select a [green]category[/] for the download:")
                    .AddChoices(categories)
                    .PageSize(15));
            return selectedCategoryName;
        }

        private void ApplyCategorySettings(Models.Category category, DownloadData downloadData)
        {
            _argumentSettings.PreferredResolution = category.Resolution;
            _argumentSettings.AudioOnly = category.Type == MediaType.Audio;
            _argumentSettings.Subtitles = category.Subtitles;

            downloadData.Category = category;
            downloadData.Format = category.Type == MediaType.Audio
                ? _argumentSettings.PreferredAudioFormat
                : _argumentSettings.PreferredVideoFormat;
        }
        #endregion

        private async Task<string> PromptForUrlAsync()
        {
            var url = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("Enter the [green]URL[/] to download:")
                    .Validate<string>(u =>
                    {
                        if (string.IsNullOrWhiteSpace(u))
                        {
                            return ValidationResult.Error("[red]URL cannot be empty.[/]");
                        }
                        if (!Uri.IsWellFormedUriString(u, UriKind.Absolute))
                        {
                            return ValidationResult.Error("[red]Please enter a valid URL.[/]");
                        }
                        return ValidationResult.Success();
                    }));
            return url;
        }

        #region Metadata Retrieval
        private async Task<List<MediaMetadataBase>> HandlePlaylistAsync(DownloadData downloadData, CancellationToken token)
        {
            var metadataContainer = new MediaContainerBase();

            int? playlistStartIndex;
            int playlistEndIndex = 0;
            List<MediaMetadataBase> itemsToDownload;

            await AnsiConsole.Status()
            .StartAsync("Retrieving playlist metadata...", async ctx =>
            {
                var result = await _metadataService.GetContainerMetadataAsync(downloadData, token);
                if (result != null)
                {
                    metadataContainer = result;
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Failed to retrieve playlist metadata. Please check the URL and try again.[/]");
                    throw new Exception("Failed to retrieve playlist metadata.");
                }
            });

            var confirmLimit = await AnsiConsole.ConfirmAsync("Set playlist limits -start index and end index-?");

            if (confirmLimit)
            {
                playlistStartIndex = await AnsiConsole.PromptAsync(
                    new TextPrompt<int?>($"Enter the [green]start index[/] for the playlist. [grey]Video count: {metadataContainer.Entries.Count()}[/]\n Enter value:")
                    .Validate(ind =>
                    {
                        if (ind < 0 || ind > metadataContainer.Entries.Count)
                        {
                            return ValidationResult.Error($"[red]Please enter a valid index between 0 and {metadataContainer.Entries.Count}.[/]");
                        }
                        return ValidationResult.Success();
                    }));
                playlistStartIndex--;

                playlistEndIndex = await AnsiConsole.PromptAsync(
                    new TextPrompt<int>($"Enter the [green]end index[/] for the playlist. [grey]Video count: {metadataContainer.Entries.Count()}[/]\nEnter value:")
                    .Validate(ind =>
                    {
                        if (ind < 0 || ind > metadataContainer.Entries.Count)
                        {
                            return ValidationResult.Error($"[red]Please enter a valid index between 0 and {metadataContainer.Entries.Count}.[/]");
                        }
                        if (ind < playlistStartIndex)
                        {
                            return ValidationResult.Error("[red]End index cannot be less than start index.[/]");
                        }
                        return ValidationResult.Success();
                    }));
                playlistEndIndex--;

                itemsToDownload = metadataContainer.Entries
                    .Skip(playlistStartIndex.Value)
                    .Take(playlistEndIndex - playlistStartIndex.Value + 1)
                    .ToList();
            }
            else
                itemsToDownload = metadataContainer.Entries.ToList();

            var containerInsert = await _mediaContainerBaseRepository.AddAsync(metadataContainer, token);

            if (containerInsert != null)
            {
                foreach (var it in itemsToDownload)
                {
                    it.MediaContainerBaseId = containerInsert.UniqueId;
                }
            }

            _downloadService.EnqueueDownloads(itemsToDownload);

            return itemsToDownload;
        }

        private async Task<MediaMetadataBase> HandleSingleMediaAsync(DownloadData downloadData, CancellationToken token)
        {
            var mediaBase = new MediaMetadataBase();

            await AnsiConsole.Status()
            .StartAsync("Retrieving URL metadata...", async ctx =>
            {
                var result = await _metadataService.GetMetadataAsync(downloadData, token);
                if (result != null)
                {
                    mediaBase = result;
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Failed to retrieve media metadata. Please check the URL and try again.[/]");
                    throw new Exception("Failed to retrieve media metadata.");
                }
            });

            //_downloadService.AddDownloadToChannel(mediaBase);
            _downloadService.EnqueueDownload(mediaBase);

            return mediaBase;
        }
        #endregion
    }
}
