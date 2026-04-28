using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display
{
    internal static class ScreenUtility
    {
        public static void PauseScreen(string message = "Press Enter to continue...")
        {
            var padLength = (Console.WindowWidth / 2) + (message.Length / 2);
            AnsiConsole.Prompt(new TextPrompt<string>($"\n[darkorange]{message.PadLeft(padLength).EscapeMarkup()}[/]\n").AllowEmpty());
        }
    }
}
