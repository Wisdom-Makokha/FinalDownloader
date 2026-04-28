using ConsoleStackNavigation.BaseScreens;
using ConsoleStackNavigation.NavigationSystem;
using ConsoleStackNavigation.NavigationSystem.Interface;
using FinalDownloader.Constants;
using FinalDownloader.Display.ScreenData;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display.Screens
{
    internal class WelcomeScreen : ScreenBase<WelcomeScreenData>
    {
        protected override async Task<NavigationResult> DisplayWithDataAsync(WelcomeScreenData screenData)
        {
            Console.Clear();
            //AnsiConsole.Write(new FigletText(screenData.Title).Centered().Color(color: Color.Yellow));
            //AnsiConsole.MarkupLine(new string('-', Console.WindowWidth));
            //AnsiConsole.MarkupLine($"[yellow]{screenData.WelcomeMessage}[/]");

            var panel = new Panel(new Rows(
                new FigletText(screenData.Title).Centered().Color(color: Color.Green),
                Text.NewLine,
                new Markup($"[yellow]by [/][green]{screenData.DeveloperName.ToUpper()}[/]").Centered(),
                Text.NewLine,
                new Markup($"[yellow]{screenData.WelcomeMessage}[/]").Centered()
                ))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green),
                Padding = new Padding(1, 1),
            };
            AnsiConsole.Write(panel);
            await Task.Delay(100);

            Console.Write("\n\n");
            ScreenUtility.PauseScreen();

            return new NavigationResult
            {
                ScreenAction = NavigationAction.Pop,
                NextScreenKey = MenuNames.MainMenu,
                Data = null
            };
        }
    }
}
