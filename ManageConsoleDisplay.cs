using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenWeatherMap
{
    internal static class ManageConsoleDisplay
    {
        public static void DisplayHeader()
        {
            Grid headerGrid = new Grid();
            headerGrid.AddColumn();
            headerGrid.AddRow(new FigletText("Weather App").Centered().Color(Color.Green));
            headerGrid.AddRow(Align.Center(new Panel("[green bold]Powered by: [link]https://openweathermap.org[/][/]").NoBorder()));
            AnsiConsole.Write(headerGrid);
        }
    }
}
