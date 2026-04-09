using ConsoleStackNavigation.BaseScreens;
using ConsoleStackNavigation.NavigationSystem;
using FinalDownloader.Data.Interface;
using FinalDownloader.Display.ScreenData;
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

            await AnsiConsole.PromptAsync(new TextPrompt<string>("Press [green]Enter[/] to go back...").AllowEmpty());
            return new NavigationResult
            {
                ScreenAction = NavigationAction.Pop,
                NextScreenKey = string.Empty,
                Data = null
            };
        }

        public static void DisplayDetails(Models.Category category)
        {
            AnsiConsole.MarkupLine($"[bold]Name:[/]             [cyan]{category.Name}[/]");
            AnsiConsole.MarkupLine($"[bold]Description:[/]      [cyan]{category.Description}[/]");
            AnsiConsole.MarkupLine($"[bold]Folder Path:[/]      [cyan]{category.FolderPath}[/]");
            AnsiConsole.MarkupLine($"[bold]Media Type:[/]       [cyan]{category.Type} [/]");
            AnsiConsole.MarkupLine($"[bold]Resolution:[/]       [cyan]{category.Resolution} [/]");
            AnsiConsole.MarkupLine($"[bold]Subtitles:[/]        [cyan]{(category.Subtitles ? "Yes" : "No")}[/]");
            AnsiConsole.MarkupLine($"[bold]Default Category:[/] [cyan]{(category.IsDefault ? "Yes" : "No")} [/]");
        }
    }
}
