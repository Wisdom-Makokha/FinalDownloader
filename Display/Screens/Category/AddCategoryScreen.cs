using ConsoleStackNavigation.BaseScreens;
using ConsoleStackNavigation.NavigationSystem;
using ConsoleStackNavigation.NavigationSystem.Interface;
using FinalDownloader.Constants;
using FinalDownloader.Data.Interface;
using FinalDownloader.Display.ScreenData;
using FinalDownloader.Models.MediaMetadata;
using FinalDownloader.Models.Settings;
using FinalDownloader.Services.Configuration;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display.Screens.Category
{
    internal class AddCategoryScreen : IScreen
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly DownloadSettings _downloadSettings;

        public AddCategoryScreen(ICategoryRepository categoryRepository, IConfigurationService configurationService)
        {
            _categoryRepository = categoryRepository;
            _downloadSettings = configurationService.DownloadSettings;
        }

        public async Task<NavigationResult> DisplayAsync(NavigationContext navigationContext)
        {
            Console.Clear();
            var categoryNames = await _categoryRepository.GetAllCategoryNamesAsync();

            AnsiConsole.MarkupLine("[bold silver underline]Add New Category[/]\n\n");
            var categoryName = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("[yellow]Enter the [/][green]name[/][yellow] of the new category: [/]")
                    .Validate(name =>
                    {
                        if (string.IsNullOrWhiteSpace(name))
                            return ValidationResult.Error("[red]Category name cannot be empty.[/]");
                        if (categoryNames.Any(c => c.Equals(name, StringComparison.OrdinalIgnoreCase)))
                            return ValidationResult.Error("[red]A category with this name already exists.[/]");
                        return ValidationResult.Success();
                    }));

            var categoryFolderPath = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("[yellow]Enter the [/][green]folder path[/][yellow] for the category: [/]")
                    .Validate(folderPath =>
                    {
                        if (string.IsNullOrWhiteSpace(folderPath))
                            return ValidationResult.Success();
                        if (!Directory.Exists(folderPath))
                            return ValidationResult.Error("[red]The specified folder path does not exist.[/]");
                        return ValidationResult.Success();
                    }));

            var description = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("[yellow]Enter a [/][green]description[/][yellow] for the category (optional): [/]")
                    .AllowEmpty());

            var type = await AnsiConsole.PromptAsync(
                new SelectionPrompt<MediaType>()
                    .Title("[yellow]Which media type is the category expected to hold: [/]")
                    .AddChoices(MediaType.Audio, MediaType.Video, MediaType.VideoNAudio));
            AnsiConsole.MarkupLine($"[yellow]Which media type is the category expected to hold: [/]{type}");

            var resolution = await AnsiConsole.PromptAsync(
                new SelectionPrompt<string>()
                    .Title("[yellow]What is the expected resolution for the category? (optional)[/]")
                    .AddChoices(_downloadSettings.SupportedResolutions));
            AnsiConsole.MarkupLine($"[yellow]What is the expected resolution for the category? (optional): [/]{resolution}");

            var subtitles = await AnsiConsole.PromptAsync(
                new ConfirmationPrompt("[yellow]Does the category require [/][green]subtitles[/]?\n\n"));

            //var isDefault = await AnsiConsole.PromptAsync(
            //    new ConfirmationPrompt("[yellow]Should this category be set as [/][green]default[/]?"));
            Console.Write("\n\n");

            var newCategory = new Models.Category
            {
                Name = categoryName,
                Description = description,
                FolderPath = categoryFolderPath,
                Type = type,
                Resolution = resolution,
                Subtitles = subtitles,
                //IsDefault = isDefault
            };

            AnsiConsole.MarkupLine($"[yellow]You are about to create a new category with the following details:[/]");
            AnsiConsole.MarkupLine($"[green]Name:[/] {categoryName}");
            AnsiConsole.MarkupLine($"[green]Folder Path:[/] {categoryFolderPath}");
            AnsiConsole.MarkupLine($"[green]Description:[/] {description}");
            AnsiConsole.MarkupLine($"[green]Media Type:[/] {type}");
            AnsiConsole.MarkupLine($"[green]Expected Resolution:[/] {resolution}");
            AnsiConsole.MarkupLine($"[green]Requires Subtitles:[/] {(subtitles ? "Yes" : "No")}");
            //AnsiConsole.MarkupLine($"[green]Set as Default:[/] {(isDefault ? "Yes" : "No")}");
            Console.Write("\n\n");

            bool confirmation = await AnsiConsole.ConfirmAsync($"Are you sure you want to add the category [green]{categoryName}[/]?");

            if (!confirmation)
            {
                Console.WriteLine("[yellow]Category creation cancelled.[/]");
            }
            else
            {
                await _categoryRepository.AddAsync(newCategory);
                AnsiConsole.MarkupLine($"[green]Category '{categoryName}' added successfully![/]");
            }
            Console.Write("\n\n");

            await AnsiConsole.PromptAsync(new TextPrompt<string>("Press [green]Enter[/] to go back...").AllowEmpty());

            return new NavigationResult
            {
                ScreenAction = NavigationAction.Pop,
                NextScreenKey = string.Empty,
                Data = null
            };
        }
    }
}
