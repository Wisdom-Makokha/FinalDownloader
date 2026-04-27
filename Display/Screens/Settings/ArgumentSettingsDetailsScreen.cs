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
    internal class ArgumentSettingsDetailsScreen : IScreen
    {
        private readonly ArgumentSettings _argumentSettings;

        public ArgumentSettingsDetailsScreen(IConfigurationService configurationService)
        {
            _argumentSettings = configurationService.ArgumentSettings;
        }

        public async Task<NavigationResult> DisplayAsync(NavigationContext ScreenContext)
        {
            Console.Clear();

            AnsiConsole.MarkupLine("[bold silver underline]Argument Settings Details[/]");
            Console.Write("\n\n");

            DisplayDetails(_argumentSettings);

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
        public static void DisplayDetails(ArgumentSettings settings)
        {
            // Group settings by category for better organization
            var formatRows = new List<string>
            {
                $"[bold cyan]Preferred Resolution:[/] {(string.IsNullOrEmpty(settings.PreferredResolution) ? "[grey]Not specified[/]" : $"[green]{EscapeMarkup(settings.PreferredResolution)}[/]")}",
                $"[bold cyan]Preferred Video Format:[/] {(string.IsNullOrEmpty(settings.PreferredVideoFormat) ? "[grey]Not specified[/]" : $"[blue]{EscapeMarkup(settings.PreferredVideoFormat)}[/]")}",
                $"[bold cyan]Preferred Audio Format:[/] {(string.IsNullOrEmpty(settings.PreferredAudioFormat) ? "[grey]Not specified[/]" : $"[green]{EscapeMarkup(settings.PreferredAudioFormat)}[/]")}",
                $"[bold cyan]Preferred Subtitle Format:[/] {(string.IsNullOrEmpty(settings.PreferredSubtitleFormat) ? "[grey]Not specified[/]" : $"[magenta]{EscapeMarkup(settings.PreferredSubtitleFormat)}[/]")}"
            };

            var featureRows = new List<string>
            {
                $"[bold cyan]Audio Only Mode:[/] {(settings.AudioOnly ? "[yellow]🎵 Enabled[/]" : "[grey]Disabled[/]")}",
                $"[bold cyan]Download Subtitles:[/] {(settings.Subtitles ? "[green]✓ Yes[/]" : "[grey]No[/]")}",
                $"[bold cyan]Embed Metadata:[/] {(settings.EmbedMetadata ? "[green]✓ Yes[/]" : "[grey]No[/]")}",
                $"[bold cyan]Embed Subtitles:[/] {(settings.EmbedSubtitles ? "[green]✓ Yes[/]" : "[grey]No[/]")}",
                $"[bold cyan]Download Thumbnail:[/] {(settings.Thumbnail ? "[yellow] Yes[/]" : "[grey]No[/]")}"
            };

            var behaviorRows = new List<string>
            {
                $"[bold cyan]Show Progress:[/] {(settings.Progress ? "[green]✓ Enabled[/]" : "[grey]Disabled[/]")}",
                $"[bold cyan]Simulation Mode:[/] {(settings.Simulate ? "[yellow]⚠️ Dry Run[/]" : "[grey]Disabled[/]")}",
                $"[bold cyan]Ignore Errors:[/] {(settings.IgnoreErrors ? "[red]⚠️ Continue on Error[/]" : "[grey]Disabled[/]")}",
                $"[bold cyan]Max Retries:[/] {(settings.MaxRetries > 0 ? $"[yellow]{settings.MaxRetries}[/]" : "[grey]0 (No retry)[/]")}"
            };

            // Create sections with headers
            var content = new List<string>
            {
                "[bold underline]Format Preferences:[/]",
                string.Join("\n", formatRows),
                "",
                "[bold underline]Features:[/]",
                string.Join("\n", featureRows),
                "",
                "[bold underline]Runtime Behavior:[/]",
                string.Join("\n", behaviorRows)
            };

            var panel = new Panel(string.Join("\n", content))
            {
                Header = new PanelHeader("[bold yellow] Argument Settings[/]"),
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
