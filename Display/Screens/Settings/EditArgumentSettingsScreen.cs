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
    internal class EditArgumentSettingsScreen : IScreen
    {
        private readonly IConfigurationService _configurationService;

        public EditArgumentSettingsScreen(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public async Task<NavigationResult> DisplayAsync(NavigationContext ScreenContext)
        {
            Console.Clear();
            var argumentSettings = _configurationService.ArgumentSettings;

            Type type = argumentSettings.GetType();
            var properties = type.GetProperties().Select(p => p.Name).ToList();
            properties.Add("Back to Argument Settings Management Menu");

            var selectedProperty = await AnsiConsole.PromptAsync(
                new SelectionPrompt<string>()
                    .Title("Select an argument setting to edit:")
                    .PageSize(20)
                    .AddChoices(properties));

            switch (selectedProperty)
            {
                case "Back to Argument Settings Management Menu":
                    break;
                default:
                    await EditPropertyAsync(selectedProperty, argumentSettings);
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

        private async Task EditPropertyAsync(string propertyName, ArgumentSettings argumentSettings)
        {
            var propertyInfo = argumentSettings.GetType().GetProperty(propertyName);

            if (propertyInfo == null)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Property '{propertyName}' not found.");
                return;
            }
            
            var currentValue = propertyInfo.GetValue(argumentSettings)?.ToString() ?? string.Empty;
            var newValue = await AnsiConsole.PromptAsync(
                new TextPrompt<string>($"Current value for [yellow]{propertyName}[/]: [green]{currentValue}[/]\nEnter new value:")
                    .AllowEmpty());

            try
            {
                // Attempt to convert the input to the correct type
                var convertedValue = Convert.ChangeType(newValue, propertyInfo.PropertyType);
                propertyInfo.SetValue(argumentSettings, convertedValue);

                await _configurationService.SaveArgumentSettings(argumentSettings);

                AnsiConsole.MarkupLine($"[green]Success:[/] '{propertyName}' updated to [yellow]{newValue}[/].");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Failed to update '{propertyName}'. {ex.Message}");
            }
        }
    }
}
