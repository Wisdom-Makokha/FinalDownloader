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

        public static void DisplayDetails(DownloadSettings settings)
        {
            var rows = new List<string>();

            // Temporary Directory
            rows.Add($"[bold cyan]Temporary Directory:[/] [dim]{EscapeMarkup(settings.TemporaryDirectory)}[/]");
            rows.Add($"[bold cyan]Delete Temp Files:[/] {(settings.DeleteTempFiles ? "[green]✓ Yes (Clean up after completion)[/]" : "[yellow]⚠️ No (Keep temp files)[/]")}");
            rows.Add($"[bold cyan]Max Concurrent Downloads:[/] {(settings.MaxConcurrentDownloads > 1 ? $"[yellow]{settings.MaxConcurrentDownloads}[/]" : $"[grey]{settings.MaxConcurrentDownloads}[/]")}");

            // Supported Video Formats
            rows.Add("");
            rows.Add("[bold underline]Supported Video Formats:[/]");
            if (settings.SupportedVideoFormats?.Any() == true)
            {
                var videoFormats = string.Join("  ", settings.SupportedVideoFormats.Select(f => $"[blue]{EscapeMarkup(f)}[/]"));
                rows.Add($"  {videoFormats}");
            }
            else
            {
                rows.Add("  [grey]None specified[/]");
            }

            // Supported Audio Formats
            rows.Add("");
            rows.Add("[bold underline]Supported Audio Formats:[/]");
            if (settings.SupportedAudioFormats?.Any() == true)
            {
                var audioFormats = string.Join("  ", settings.SupportedAudioFormats.Select(f => $"[green]{EscapeMarkup(f)}[/]"));
                rows.Add($"  {audioFormats}");
            }
            else
            {
                rows.Add("  [grey]None specified[/]");
            }

            // Supported Resolutions
            rows.Add("");
            rows.Add("[bold underline]Supported Resolutions:[/]");
            if (settings.SupportedResolutions?.Any() == true)
            {
                var resolutions = string.Join("  ", settings.SupportedResolutions.Select(r => $"[yellow]{EscapeMarkup(r)}[/]"));
                rows.Add($"  {resolutions}");
            }
            else
            {
                rows.Add("  [grey]None specified[/]");
            }

            // Supported Subtitle Formats
            rows.Add("");
            rows.Add("[bold underline]Supported Subtitle Formats:[/]");
            if (settings.SupportedSubtitleFormats?.Any() == true)
            {
                var subtitleFormats = string.Join("  ", settings.SupportedSubtitleFormats.Select(f => $"[magenta]{EscapeMarkup(f)}[/]"));
                rows.Add($"  {subtitleFormats}");
            }
            else
            {
                rows.Add("  [grey]None specified[/]");
            }

            // Summary statistics
            rows.Add("");
            rows.Add("[bold cyan]Total Supported Formats:[/] " +
                $"[blue]{settings.SupportedVideoFormats?.Count ?? 0} video[/] • " +
                $"[green]{settings.SupportedAudioFormats?.Count ?? 0} audio[/] • " +
                $"[magenta]{settings.SupportedSubtitleFormats?.Count ?? 0} subtitle[/]");

            var content = string.Join("\n", rows);

            var panel = new Panel(content)
            {
                Header = new PanelHeader("[bold yellow]🔧 Download Settings[/]"),
                Border = BoxBorder.Heavy,
                BorderStyle = new Style(foreground: Color.Cyan1),
                Padding = new Padding(2, 1, 2, 1),
                Expand = true
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }

        // Helper method to escape markup characters
        static string EscapeMarkup(string text)
        {
            return text?.Replace("[", "[[").Replace("]", "]]") ?? string.Empty;
        }
    }
}
