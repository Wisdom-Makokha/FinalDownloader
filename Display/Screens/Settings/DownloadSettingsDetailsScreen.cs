using ConsoleStackNavigation.NavigationSystem;
using ConsoleStackNavigation.NavigationSystem.Interface;
using FinalDownloader.Models.Settings;
using FinalDownloader.Services.Configuration;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display.Screens.Settings
{
    internal class DownloadSettingsDetailsScreen : IScreen
    {
        private readonly DownloadSettings _downloadSettings;

        public DownloadSettingsDetailsScreen(IConfigurationService configurationService)
        {
            _downloadSettings = configurationService.DownloadSettings;
        }

        public async Task<NavigationResult> DisplayAsync(NavigationContext ScreenContext)
        {
            Console.Clear();

            AnsiConsole.MarkupLine("[bold silver underline]Download Settings Details[/]");
            Console.Write("\n\n");

            DisplayDetails(_downloadSettings);
            
            Console.Write("\n\n");

            await AnsiConsole.PromptAsync(
                new TextPrompt<string>("[grey]Press [[Enter]] to go back...[/]")
                .AllowEmpty()
            );

            return new NavigationResult
            {
                ScreenAction = NavigationAction.Pop,
                NextScreenKey = string.Empty,
                Data = null,
                MenuScreenCustomizationData = null
            };
        }

        public static void DisplayDetails(DownloadSettings downloadSettings)
        {
            AnsiConsole.MarkupLine($"[bold]Download Path:[/] [cyan]{downloadSettings.TemporaryDirectory}[/]");
            AnsiConsole.MarkupLine($"[bold]Max Concurrent Downloads:[/] [cyan]{downloadSettings.MaxConcurrentDownloads}[/]");
            AnsiConsole.MarkupLine($"[bold]Delete temporary files:[/] [cyan]{downloadSettings.DeleteTempFiles}[/]");
            AnsiConsole.Markup("[bold]Supported video formats:\n -[/]");
            foreach (var format in downloadSettings.SupportedVideoFormats)
            {
                AnsiConsole.Markup($"[cyan]{format}, [/]");
            }
            AnsiConsole.Markup("[bold]\nSupported audio formats:\n - [/]");
            foreach (var format in downloadSettings.SupportedAudioFormats)
            {
                AnsiConsole.Markup($"[cyan]{format}, [/]");
            }
            AnsiConsole.Markup("[bold]\nSupported resolutions:\n - [/]");
            foreach (var resolution in downloadSettings.SupportedResolutions)
            {
                AnsiConsole.Markup($"[cyan]{resolution}, [/]");
            }
            AnsiConsole.Markup("[bold]\nSupported subtitle formats:\n - [/]");
            foreach (var subtitle in downloadSettings.SupportedSubtitleFormats)
            {
                AnsiConsole.Markup($"[cyan]{subtitle}[/]");
            }
        }
    }
}
