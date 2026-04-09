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
    internal class DeleteCategoryScreen : ScreenBase<CategoryData>
    {
        private readonly ICategoryRepository _categoryRepository;

        public DeleteCategoryScreen(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        protected override async Task<NavigationResult> DisplayWithDataAsync(CategoryData data)
        {
            Console.Clear();
            var category = _categoryRepository.GetByNameAsync(data.Name);

            bool confirm = await AnsiConsole.ConfirmAsync($"Are you sure you want to delete the category '{data.Name}'?");

            if (confirm)
            {
                await _categoryRepository.DeleteAsync(category.Id);
                AnsiConsole.MarkupLine($"[green]Category '{data.Name}' has been deleted.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]Deletion of category '{data.Name}' has been cancelled.[/]");
            }

            await AnsiConsole.PromptAsync(new TextPrompt<string>("Press [green]Enter[/] to go back...").AllowEmpty());

            return new NavigationResult
            {
                ScreenAction = NavigationAction.Pop,
                NextScreenKey = string.Empty,
                Data = null,
                MenuScreenCustomizationData = null
            };
        }
    }
}
