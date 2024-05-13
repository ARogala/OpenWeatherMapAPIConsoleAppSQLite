using OpenWeatherMap.Models;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public static List<string> GetNewLocationInput()
        {
            List<string> input = new List<string>();

            AnsiConsole.WriteLine("Please use ISO 3166 country codes. ");
            AnsiConsole.Markup("[link]https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes[/]");
            AnsiConsole.WriteLine("");
            AnsiConsole.WriteLine("State code (only for the US)");
            AnsiConsole.WriteLine("Outsite the US? Enter notUS for state and the app will take care of the rest");
            AnsiConsole.WriteLine("");
            string inputCity = AnsiConsole.Ask<string>("What [green]City[/] would you like weather for?");
            string inputStateCode = AskStateCode();
            string inputCountryCode = AskCountryCode();

            if (inputCity != "" && inputStateCode != "" && inputCountryCode != "")
            {
                input.Add(inputCity);
                input.Add(inputStateCode);
                input.Add(inputCountryCode);
            }
            return input;
        }

        private static string AskCountryCode()
        {
            return AnsiConsole.Prompt(
               new TextPrompt<string>("And the [green]County Code[/] is? (example US): ")
               .ValidationErrorMessage("[red]All country codes are 2 characters long and do NOT contain numbers.[/]")
               .Validate(countryCode =>
               {

                   if (Regex.IsMatch(countryCode, @"\d") || (Regex.IsMatch(countryCode, @"^[a-zA-Z0-9 ]*$") == false) || countryCode.Length != 2)
                   {
                       return ValidationResult.Error("[red]All country codes are 2 characters long and do NOT contain numbers.[/]");
                   }
                   else
                   {
                       return ValidationResult.Success();
                   }

               }));
        }

        private static string AskStateCode()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("What [green]State[/] is that city in? Use abbreviation. (example NH): ")
                .ValidationErrorMessage("[red]All US state codes are 2 characters long and do NOT contain numbers. Outside the US? Enter (case sensitive) notUS[/]")
                .Validate(stateCode =>
                {
                    if (stateCode.Length > 2 && stateCode != "notUS")
                    {
                        return ValidationResult.Error("[red]All US state codes are 2 characters long and do NOT contain numbers. Outside the US? Enter (case sensitive) notUS[/]");

                    }
                    else if (Regex.IsMatch(stateCode, @"\d") || (Regex.IsMatch(stateCode, @"^[a-zA-Z0-9 ]*$") == false) || (stateCode.Length != 2 && stateCode != "notUS"))
                    {
                        return ValidationResult.Error("[red]All US state codes are 2 characters long and do NOT contain numbers. Outside the US? Enter (case sensitive) notUS[/]");
                    }
                    else
                    {
                        return ValidationResult.Success();
                    }

                }));
        }

        public static List<string> GetDefaultLocation()
        {
            AnsiConsole.WriteLine("No saved or default location found please enter one.");
            AnsiConsole.WriteLine("Note: The location you enter here will be your default location.");
            AnsiConsole.WriteLine("Note: If you removed all locations or removed your default location you will be immediately asked to add one -- the app needs location to work.");
            List<string> newLocation = ManageUserInput.GetNewLocationInput();

            return newLocation;
        }

        public static List<string> SettingsPrompt(Dictionary<string, bool> settings)
        {
            List<string> newSettings = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Change your [green]settings[/] \n[blue]Note:[/] Setting Recurring Update from false to true requires reboot")
                    .NotRequired() // Not required to have a setting checked
                    .PageSize(5)
                    .MoreChoicesText("[grey](Move up and down to reveal more settings)[/]")
                    .InstructionsText(
                        "[grey](Press [blue]<space>[/] to toggle a setting, " +
                        "[green]<enter>[/] to accept)[/]")
                    .AddChoices(new[] {
                        "Display Saved Weather", "Suppress Header",
                        "Recurring Update", "Extended Menu"
                    })
                    .Select(settings["Display Saved Weather"] == true ? "Display Saved Weather" : "")
                    .Select(settings["Suppress Header"] == true ? "Suppress Header" : "")
                    .Select(settings["Recurring Update"] == true ? "Recurring Update" : "")
                    .Select(settings["Extended Menu"] == true ? "Extended Menu" : ""));
            return newSettings;
        }
    }
}
