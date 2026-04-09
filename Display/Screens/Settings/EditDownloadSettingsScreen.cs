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
    internal class EditDownloadSettingsScreen : IScreen
    {
        private readonly IConfigurationService _configurationService;

        public EditDownloadSettingsScreen(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public async Task<NavigationResult> DisplayAsync(NavigationContext ScreenContext)
        {
            Console.Clear();
            var downloadSettings = _configurationService.DownloadSettings;
            Type type = typeof(DownloadSettings);

            var properties = type.GetProperties().Select(p => p.Name).ToList();
            properties.Remove("TemporaryDirectory");
            properties.Add("Back to Download Settings Management Menu");

            var propertySelection = await AnsiConsole.PromptAsync(
                new SelectionPrompt<string>()
                    .Title("Select a setting to edit:")
                    .PageSize(20)
                    .AddChoices(properties)
            );

            switch (propertySelection)
            {
                case "Back to Download Settings Management Menu":
                    break;
                default:
                    await EditPropertyAsync(propertySelection, downloadSettings);
                    break;
            }

            return new NavigationResult
            {
                ScreenAction = NavigationAction.Pop,
                NextScreenKey = string.Empty,
                Data = null,
                MenuScreenCustomizationData = null
            };
        }

        private async Task EditPropertyAsync(string propertyName, DownloadSettings downloadSettings)
        {
            var propertyInfo = typeof(DownloadSettings).GetProperty(propertyName);
            if (propertyInfo == null)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Property '{propertyName}' not found.");
                return;
            }
            else if (typeof(IEnumerable<string>).IsAssignableFrom(propertyInfo.PropertyType) && propertyInfo.PropertyType != typeof(string))
            {
                await EditEnumerablePropertyAsync(propertyName, downloadSettings);
                return;
            }

            var currentValue = propertyInfo.GetValue(downloadSettings)?.ToString() ?? "null";
            var newValue = await AnsiConsole.PromptAsync(
                new TextPrompt<string>($"Current value for [yellow]{propertyName}[/]: [green]{currentValue}[/]\nEnter new value:"));
            
            try
            {
                // Convert the new value to the appropriate type
                var convertedValue = Convert.ChangeType(newValue, propertyInfo.PropertyType);
                propertyInfo.SetValue(downloadSettings, convertedValue);

                await _configurationService.SaveDownloadSettings(downloadSettings);
                
                AnsiConsole.MarkupLine($"[green]Successfully updated:[/] {propertyName.EscapeMarkup()} to [yellow]{newValue.EscapeMarkup()}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error updating setting:[/] {ex.Message}");
            }
        }

        private async Task EditEnumerablePropertyAsync(string propertyName, DownloadSettings downloadSettings)
        {
            var propertyInfo = typeof(DownloadSettings).GetProperty(propertyName);

            if (propertyInfo == null)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Property '{propertyName}' not found.");
                return;
            }

            var currentValue = propertyInfo.GetValue(downloadSettings) as IEnumerable<string>;
            var currentValueDisplay = currentValue != null ? string.Join(", ", currentValue) : "null";

            var newValue = await AnsiConsole.PromptAsync(
                new TextPrompt<string>($"Current value for [yellow]{propertyName}[/]: [green]{currentValueDisplay}[/]\nEnter new comma-separated values:"));

            try
            {
                var newValues = newValue.Split(',').Select(v => v.Trim()).ToList();
                propertyInfo.SetValue(downloadSettings, newValues);

                await _configurationService.SaveDownloadSettings(downloadSettings);

                AnsiConsole.MarkupLine($"[green]Successfully updated:[/] {propertyName.EscapeMarkup()} to [yellow]{string.Join(", ", newValues).EscapeMarkup()}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error updating setting:[/] {ex.Message}");
            }
        }
    }
}
