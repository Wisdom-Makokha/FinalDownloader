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
        public static void DisplayDetails(ArgumentSettings argumentSettings)
        {
            AnsiConsole.MarkupLine($"[bold]Preferred Resolution:[/] [cyan]{argumentSettings.PreferredResolution}[/]");
            AnsiConsole.MarkupLine($"[bold]Preferred Audio Format:[/] [cyan]{argumentSettings.PreferredAudioFormat}[/]");
            AnsiConsole.MarkupLine($"[bold]Preferred Video Format:[/] [cyan]{argumentSettings.PreferredVideoFormat}[/]");
            AnsiConsole.MarkupLine($"[bold]Preferred subtitle Format:[/] [cyan]{argumentSettings.PreferredSubtitleFormat}[/]");
            AnsiConsole.MarkupLine($"[bold]Audio Only:[/] [cyan]{argumentSettings.AudioOnly}[/]");
            AnsiConsole.MarkupLine($"[bold]Subtitles:[/] [cyan]{argumentSettings.Subtitles}[/]");
            AnsiConsole.MarkupLine($"[bold]Embed Metadata:[/] [cyan]{argumentSettings.EmbedMetadata}[/]");
            AnsiConsole.MarkupLine($"[bold]Embed Subtitles:[/] [cyan]{argumentSettings.EmbedSubtitles}[/]");
            AnsiConsole.MarkupLine($"[bold]Show Progress:[/] [cyan]{argumentSettings.Progress}[/]");
            AnsiConsole.MarkupLine($"[bold]Embed Thumbnail:[/] [cyan]{argumentSettings.Thumbnail}[/]");
            AnsiConsole.MarkupLine($"[bold]Simulate Download:[/] [cyan]{argumentSettings.Simulate}[/]");
            AnsiConsole.MarkupLine($"[bold]Ignore Errors:[/] [cyan]{argumentSettings.IgnoreErrors}[/]");
            AnsiConsole.MarkupLine($"[bold]Max Download Retries:[/] [cyan]{argumentSettings.MaxRetries}[/]");
        }
    }
}
