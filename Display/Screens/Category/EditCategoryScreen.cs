using ConsoleStackNavigation.BaseScreens;
using ConsoleStackNavigation.NavigationSystem;
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
    internal class EditCategoryScreen : ScreenBase<CategoryData>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly DownloadSettings _downdSettings;

        public EditCategoryScreen(ICategoryRepository categoryRepository, IConfigurationService configurationService)
        {
            _categoryRepository = categoryRepository;
            _downdSettings = configurationService.DownloadSettings;
        }

        protected override async Task<NavigationResult> DisplayWithDataAsync(CategoryData data)
        {
            Console.Clear();
            var category = await _categoryRepository.GetByNameAsync(data.Name);

            if (category == null)
            {
                AnsiConsole.MarkupLine($"[red]Category with name {data.Name} not found.[/]");
            }
            else
            {
                Type type = category.GetType();
                var properties = type.GetProperties().Select(ps => ps.Name).ToList();
                properties.Add("Back To Category Management Menu");

                string uselessProperty = "Items";

                if (properties.Contains(uselessProperty.ToLower().Trim()))
                    properties.Remove(uselessProperty);

                var propertyToEdit = await AnsiConsole.PromptAsync(
                    new SelectionPrompt<string>()
                        .Title("Select a property to edit:")
                        .AddChoices(properties));

                switch (propertyToEdit)
                {
                    case "Name":
                        await UpdateCategoryName(category);
                        break;
                    case "FolderPath":
                        await UpdateCategoryFolderPath(category);
                        break;
                    case "Description":
                        await UpdateCategoryDescription(category);
                        break;
                    case "Type":
                        await UpdateCategoryType(category);
                        break;
                    case "Resolution":
                        await UpdateCategoryResolution(category);
                        break;
                    case "Subtitles":
                        await UpdateCategorySubtitles(category);
                        break;
                    case "IsDefault":
                        await UpdateCategoryIsDefault(category);
                        break;
                    case "BACK":
                        break;
                    default:
                        AnsiConsole.MarkupLine("[red]Invalid selection.[/]");
                        AnsiConsole.MarkupLine("[red]Wait! How did you do that?[/]");
                        break;
                }
            }

            return new NavigationResult
            {
                ScreenAction = NavigationAction.Pop,
                NextScreenKey = string.Empty,
                Data = data,
                MenuScreenCustomizationData = null
            };
        }

        private async Task UpdateCategoryName(Models.Category category)
        {
            var categoryNames = await _categoryRepository.GetAllCategoryNamesAsync();

            var newName = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("[yellow]Enter a [/][green]new name[/][yellow] of the category: [/]")
                    .Validate(name =>
                    {
                        if (string.IsNullOrWhiteSpace(name))
                            return ValidationResult.Error("[red]Category name cannot be empty.[/]");
                        if (categoryNames.Any(c => c.Equals(name, StringComparison.OrdinalIgnoreCase)))
                            return ValidationResult.Error("[red]A category with this name already exists.[/]");
                        return ValidationResult.Success();
                    }));

            bool confirm = await AnsiConsole.PromptAsync(
                new ConfirmationPrompt($"[yellow]Are you sure you want to change the category name to [/][green]{newName}[/][yellow]?[/]"));

            if (!confirm)
            {
                AnsiConsole.MarkupLine("[red]Category name update cancelled.[/]");
                return;
            }

            category.Name = newName;
            await _categoryRepository.UpdateAsync(category);

            AnsiConsole.MarkupLine($"[green]Category name updated successfully to {newName}.[/]");
        }

        private async Task UpdateCategoryFolderPath(Models.Category category)
        {
            var newFolderPath = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("[yellow]Enter a [/][green]new folder path[/][yellow] for the category: [/]")
                    .Validate(folderPath =>
                    {
                        if (string.IsNullOrWhiteSpace(folderPath))
                            return ValidationResult.Success();
                        if (!Directory.Exists(folderPath))
                            return ValidationResult.Error("[red]The specified folder path does not exist.[/]");
                        return ValidationResult.Success();
                    }));

            bool confirm = await AnsiConsole.PromptAsync(
                new ConfirmationPrompt($"[yellow]Are you sure you want to change the category folder path to [/][green]{newFolderPath}[/][yellow]?[/]"));

            if (!confirm)
            {
                AnsiConsole.MarkupLine("[red]Category folder path update cancelled.[/]");
                return;
            }

            category.FolderPath = newFolderPath;
            await _categoryRepository.UpdateAsync(category);

            AnsiConsole.MarkupLine($"[green]Category folder path updated successfully to {newFolderPath}.[/]");
        }

        private async Task UpdateCategoryDescription(Models.Category category)
        {
            var newDescription = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("[yellow]Enter a [/][green]new description[/][yellow] for the category (optional): [/]")
                    .AllowEmpty());

            bool confirm = await AnsiConsole.PromptAsync(
                new ConfirmationPrompt($"[yellow]Are you sure you want to change the category description to [/][green]{newDescription}[/][yellow]?[/]"));

            if (!confirm)
            {
                AnsiConsole.MarkupLine("[red]Category description update cancelled.[/]");
                return;
            }

            category.Description = newDescription;
            await _categoryRepository.UpdateAsync(category);

            AnsiConsole.MarkupLine($"[green]Category description updated successfully.[/]");
        }

        private async Task UpdateCategoryType(Models.Category category)
        {
            var newType = await AnsiConsole.PromptAsync(
                new SelectionPrompt<MediaType>()
                    .Title("[yellow]Which media type is the category expected to hold: [/] ")
                    .AddChoices(MediaType.Audio, MediaType.Video, MediaType.VideoNAudio));

            bool confirm = await AnsiConsole.PromptAsync(
                new ConfirmationPrompt($"[yellow]Are you sure you want to change the category media type to [/][green]{newType}[/][yellow]?[/]"));

            if (!confirm)
            {
                AnsiConsole.MarkupLine("[red]Category media type update cancelled.[/]");
                return;
            }

            category.Type = newType;
            await _categoryRepository.UpdateAsync(category);

            AnsiConsole.MarkupLine($"[green]Category media type updated successfully to {newType}.[/]");
        }

        private async Task UpdateCategoryResolution(Models.Category category)
        {
            var newResolution = await AnsiConsole.PromptAsync(
                new SelectionPrompt<string>()
                    .Title("[yellow]What is the expected resolution for the category? (optional)[/]")
                    .AddChoices(_downdSettings.SupportedResolutions));

            bool confirm = await AnsiConsole.PromptAsync(
                new ConfirmationPrompt($"[yellow]Are you sure you want to change the category expected resolution to [/][green]{newResolution}[/][yellow]?[/]"));

            if (!confirm)
            {
                AnsiConsole.MarkupLine("[red]Category expected resolution update cancelled.[/]");
                return;
            }

            category.Resolution = newResolution;
            await _categoryRepository.UpdateAsync(category);

            AnsiConsole.MarkupLine($"[green]Category expected resolution updated successfully to {newResolution}.[/]");
        }

        private async Task UpdateCategorySubtitles(Models.Category category)
        {
            var newSubtitles = await AnsiConsole.PromptAsync(
                new ConfirmationPrompt("[yellow]Does the category require [/][green]subtitles[/]?"));

            bool confirm = await AnsiConsole.PromptAsync(
                new ConfirmationPrompt($"[yellow]Are you sure you want to change the category subtitles requirement to [/][green]{(newSubtitles ? "required" : "not required")}[/][yellow]?[/]"));

            if (!confirm)
            {
                AnsiConsole.MarkupLine("[red]Category subtitles requirement update cancelled.[/]");
                return;
            }

            category.Subtitles = newSubtitles;
            await _categoryRepository.UpdateAsync(category);

            AnsiConsole.MarkupLine($"[green]Category subtitles requirement updated successfully to {(newSubtitles ? "required" : "not required")}.[/]");
        }

        private async Task UpdateCategoryIsDefault(Models.Category category)
        {
            var newIsDefault = await AnsiConsole.PromptAsync(
                new ConfirmationPrompt("[yellow]Should this category be set as [/][green]default[/]?"));

            bool confirm = await AnsiConsole.PromptAsync(
                new ConfirmationPrompt($"[yellow]Are you sure you want to change the category default status to [/][green]{(newIsDefault ? "default" : "not default")}[/][yellow]?[/]"));

            if (!confirm)
            {
                AnsiConsole.MarkupLine("[red]Category default status update cancelled.[/]");
                return;
            }

            category.IsDefault = newIsDefault;
            await _categoryRepository.SetDefaultCategoryAsync(category.Name);

            AnsiConsole.MarkupLine($"[green]Category default status updated successfully to {(newIsDefault ? "default" : "not default")}.[/]");
        }
    }
}
