using ConsoleStackNavigation.NavigationSystem;
using ConsoleStackNavigation.NavigationSystem.Interface;
using FinalDownloader.Services.Update;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display.Screens.Settings
{
    internal class UpdateScreen : IScreen
    {
        private readonly IUpdateService _updateService;

        public UpdateScreen(IUpdateService updateService)
        {
            _updateService = updateService;
        }

        public async Task<NavigationResult> DisplayAsync(NavigationContext ScreenContext)
        {
            Console.Clear();

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Checking for updates...", async ctx =>
                {
                    bool isUpdated = await _updateService.UpdateToolAsync();
                    if (isUpdated)
                    {
                        ctx.Status("Tool updated successfully.");
                    }
                    else
                    {
                        ctx.Status("No updates found. Starting application...");
                    }
                    await Task.Delay(4000);
                });

            return new NavigationResult
            {
                NextScreenKey = string.Empty,
                ScreenAction = NavigationAction.Pop,
                Data = null,
                MenuScreenCustomizationData = null,
            };
        }
    }
}
