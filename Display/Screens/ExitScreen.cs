using ConsoleStackNavigation.BaseScreens;
using ConsoleStackNavigation.NavigationSystem;
using ConsoleStackNavigation.NavigationSystem.Interface;
using ConsoleStackNavigation.ScreenUtilties;
using FinalDownloader.Display.ScreenData;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display.Screens
{
    internal class ExitScreen : ScreenBase<ExitScreenData>
    {
        protected override async Task<NavigationResult> DisplayWithDataAsync(ExitScreenData screenData)
        {
            Console.Clear();
            int DelayTime = 1000;

            Printer.PrintScreenTitle(screenData.Title);
            AnsiConsole.MarkupLine($"[yellow]{screenData.ExitMessage}[/]");
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Exiting...", async ctx =>
                {
                    ctx.Status("Cleaning up... ");
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    await Task.Delay(DelayTime);
                });

            return new NavigationResult
            {
                ScreenAction = NavigationAction.Clear,
                NextScreenKey = string.Empty,
                Data = null
            };
        }
    }
}
