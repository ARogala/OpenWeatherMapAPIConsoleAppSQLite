using OpenWeatherMap.Models;
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

        public static string GetChoice()
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Please select an option below")
                    .PageSize(5)
                    .MoreChoicesText("[green](Move up and down to reveal more choices)[/]")
                    .AddChoices(new[] {
                        "Clear Console","Update weather","Get weather from a saved location","Display saved weather",
                        "Get 5 day forecast","Get 5 day forecast from a saved location","Display saved forecast",
                        "Get celestial data","Get celestial data from a saved location",
                        "Get 8 hour weather statistics","Get 12 hour weather statistics","Get 24 hour weather statistics",
                        "Add a new location","Switch default location","Edit a saved location", "Remove a saved location","List all saved locations",
                        "Cancel Recurring Weather Update","Display short menu","Settings","Quit"
                    }));
            return choice;
        }

        public static string GetShortChoice()
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Please select an option below")
                    .PageSize(5)
                    .MoreChoicesText("[green](Move up and down to reveal more choices)[/]")
                    .AddChoices(new[] {
                        "Clear Console","Update weather","Get 8 hour weather statistics","Get 5 day forecast","Display more options","Settings","Quit"
                    }));
            return choice;
        }

        public static int ChooseLocation()
        {
            List<SavedLocations> savedLocationsList = ManageSQL.GetSavedLocations();
            SelectionPrompt<string> prompt = new SelectionPrompt<string>()
                .Title("Please select a location below")
                .PageSize(5)
                .MoreChoicesText("[green](Move up and down to reveal more choices)[/]");

            foreach (SavedLocations location in savedLocationsList)
            {
                prompt.AddChoice($"{location.City} -- {location.StateCode} -- {location.CountryCode} -- default = {location.IsDefalut}");
            }

            string choice = AnsiConsole.Prompt(prompt);
            //get new default location id
            int newDefaultLocationId = 0;
            foreach (SavedLocations location in savedLocationsList)
            {
                if ($"{location.City} -- {location.StateCode} -- {location.CountryCode} -- default = {location.IsDefalut}" == choice)
                {
                    newDefaultLocationId = location.LocationId;
                }
            }
            return newDefaultLocationId;
        }
    }
}
