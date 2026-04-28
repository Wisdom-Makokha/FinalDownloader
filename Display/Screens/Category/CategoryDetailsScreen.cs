using ConsoleStackNavigation.BaseScreens;
using ConsoleStackNavigation.NavigationSystem;
using FinalDownloader.Data.Interface;
using FinalDownloader.Display.ScreenData;
using FinalDownloader.Models.MediaMetadata;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display.Screens.Category
{
    internal class CategoryDetailsScreen : ScreenBase<CategoryData>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryDetailsScreen(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        protected override async Task<NavigationResult> DisplayWithDataAsync(CategoryData categoryData)
        {
            Console.Clear();
            var category = await _categoryRepository.GetByNameAsync(categoryData.Name);

            if (category == null)
            {
                AnsiConsole.MarkupLine($"[red]Category '{categoryData.Name}' not found.[/]");
                await AnsiConsole.PromptAsync(new TextPrompt<string>("Press [green]Enter[/] to go back...").AllowEmpty());
                return new NavigationResult
                {
                    ScreenAction = NavigationAction.Pop,
                    NextScreenKey = string.Empty,
                    Data = null
                };
            }

            AnsiConsole.MarkupLine($"[bold silver underline]Category Details[/]");
            Console.Write("\n\n");

            DisplayDetails(category);

            Console.Write("\n\n");

            ScreenUtility.PauseScreen("Press [green]Enter[/] to go back...");

            return new NavigationResult
            {
                ScreenAction = NavigationAction.Pop,
                NextScreenKey = string.Empty,
                Data = null
            };
        }

        public static void DisplayDetails(Models.Category category)
        {
            // Color code based on MediaType
            var mediaTypeColor = category.Type switch
            {
                MediaType.Video => "blue",
                MediaType.Audio => "green",
                _ => "grey"
            };

            var rows = new List<string>
            {
                $"[bold cyan]ID:[/] [grey]{category.Id}[/]",
                $"[bold cyan]Name:[/] [yellow]{EscapeMarkup(category.Name)}[/]",
                $"[bold cyan]Folder Path:[/] [dim]{EscapeMarkup(category.FolderPath)}[/]",
                $"[bold cyan]Media Type:[/] [{mediaTypeColor}]{category.Type}[/]",
                $"[bold cyan]Description:[/] [italic]{EscapeMarkup(category.Description ?? "[grey]N/A[/]")}[/]",
                $"[bold cyan]Resolution:[/] {(string.IsNullOrEmpty(category.Resolution) ? "[grey]Not specified[/]" : $"[green]{category.Resolution}[/]")}",
                $"[bold cyan]Subtitles:[/] {(category.Subtitles ? "[green]✓ Enabled[/]" : "[red]✗ Disabled[/]")}",
                $"[bold cyan]Default Category:[/] {(category.IsDefault ? "[yellow]★ Yes (Default)[/]" : "[grey]No[/]")}"
            };

            // Add item count with icon
            var itemCount = category.Items?.Count ?? 0;
            var itemIcon = category.Type switch
            {
                MediaType.Video => "🎬",
                MediaType.Audio => "🎵",
                _ => "📄"
            };
            rows.Add($"[bold cyan]Items Count:[/] {(itemCount > 0 ? $"[green]{itemIcon} {itemCount} items[/]" : "[grey]0 items[/]")}");

            var content = string.Join("\n", rows);

            // Choose header icon based on MediaType
            var headerIcon = category.Type switch
            {
                MediaType.Video => "🎥",
                MediaType.Audio => "🎧",
                _ => "📁"
            };

            var panel = new Panel(content)
            {
                Header = new PanelHeader($"[bold yellow]{headerIcon} Category: {EscapeMarkup(category.Name)}[/]"),
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
