using ConsoleStackNavigation.NavigationSystem;
using ConsoleStackNavigation.NavigationSystem.Interface;
using FinalDownloader.Data.Interface;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display.Screens.Category
{
    internal class SetDefaultCategoryScreen : IScreen
    {
        private readonly ICategoryRepository _categoryRepository;

        public SetDefaultCategoryScreen(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<NavigationResult> DisplayAsync(NavigationContext navigationContext)
        {
            Console.Clear();
            // steps
            // 1. Get list of categories
            // 2. Ask user to select a category to set as default
            // 3. Set the selected category as default in the database

            var categoryNames = await _categoryRepository.GetAllCategoryNamesAsync();

            var categoryPick = await AnsiConsole.PromptAsync(
                new SelectionPrompt<string>()
                    .Title("Select a category to set as default:")
                    .PageSize(10)
                    .AddChoices(categoryNames)
            );

            bool confirmation = await AnsiConsole.ConfirmAsync($"Are you sure you want to set [yellow]{categoryPick}[/] as the default category?");

            if (confirmation)
            {
                await _categoryRepository.SetDefaultCategoryAsync(categoryPick);
                AnsiConsole.MarkupLine($"[green]Default category set to:[/] [yellow]{categoryPick}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Operation cancelled. Default category not changed.[/]");
            }

            return new NavigationResult
            {
                ScreenAction = NavigationAction.Pop,
                NextScreenKey = string.Empty,
                Data = null
            };
        }
    }
}
