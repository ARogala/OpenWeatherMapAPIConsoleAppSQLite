using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenWeatherMap.Managers
{
    internal static class ManageUserInput
    {
        public static string APIKeyPrompt()
        {
            AnsiConsole.WriteLine("No OpenWeatherMap API key detected you need to input one. Or input nothing and press Enter to quit.");
            string inputApiKey = AnsiConsole.Prompt(new TextPrompt<string>("What's your [green]API Key[/]?").AllowEmpty());
            return inputApiKey;
        }

    }
}
