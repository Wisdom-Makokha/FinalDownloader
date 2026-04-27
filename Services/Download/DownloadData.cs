using FinalDownloader.Models;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Download
{
    internal class DownloadData
    {
        public string Url { get; set; }
        public Category Category { get; set; }
        public string Format { get; set; }
        public bool IsPlaylist { get; set; }
        public int? PlaylistStartIndex { get; set; }
        public int? PlaylistEndIndex { get; set; }

        public override string ToString()
        {
            return $"Download Data Details:- \n" +
                $"Url: {Url}\n" +
                $"Category: {Category.Name}\n" +
                $"Format: {Format}\n" +
                $"Is Playlist: {IsPlaylist}\n" +
                (PlaylistStartIndex.HasValue ? $"Playlist Start Index: {PlaylistStartIndex}\n" : string.Empty)+
                (PlaylistEndIndex.HasValue ? $"Playlist End Index: {PlaylistEndIndex}" : string.Empty);
        }

        public static bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        public static void DisplayDownloadData(DownloadData data)
        {
            var grid = new Grid()
                .AddColumn(new GridColumn().PadRight(4))
                .AddColumn()
                .AddRow("[bold]URL:[/]", data.Url)
                .AddRow("[bold]Category:[/]", data.Category?.Name ?? "N/A")
                .AddRow("[bold]Format:[/]", data.Format)
                .AddRow("[bold]Is Playlist:[/]", data.IsPlaylist ? "[green]Yes[/]" : "[red]No[/]");

            if (data.IsPlaylist)
            {
                var rangeInfo = string.Empty;
                if (data.PlaylistStartIndex.HasValue && data.PlaylistEndIndex.HasValue)
                {
                    rangeInfo = $"{data.PlaylistStartIndex} - {data.PlaylistEndIndex}";
                }
                else if (data.PlaylistStartIndex.HasValue)
                {
                    rangeInfo = $"From {data.PlaylistStartIndex}";
                }
                else if (data.PlaylistEndIndex.HasValue)
                {
                    rangeInfo = $"Up to {data.PlaylistEndIndex}";
                }

                if (!string.IsNullOrEmpty(rangeInfo))
                {
                    grid.AddRow("[bold]Playlist Range:[/]", rangeInfo);
                }
            }

            var panel = new Panel(grid)
            {
                Header = new PanelHeader(" Download Data Details "),
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1),
                BorderStyle = Style.Parse("cyan"),
                Expand = true
            };

            AnsiConsole.Write(panel);
        }

    }
}
