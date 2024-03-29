﻿using System.Text.Json;
using System.Text.RegularExpressions;
using OpenWeatherMap.Managers;
using OpenWeatherMap.Models;
using Spectre.Console;
using System.Threading;

namespace OpenWeatherMap
{
    internal class Program
    {
        //APIKEY.xml stores the apiKey
        //string apiKey is used for api call
        private static string apiKey = String.Empty;

        //Weather.db is SQLite databse it stores locations and weather events for the locations
        //SavedLocation.cs is is the class that stores location values from database
        //Location.cs is record class that stores api returned data
        private static List<SavedLocations> savedLocationsList;

        private static List<Location> location;
        private static CurrentWeather currentWeather;
        private static ForecastWeather forecastWeather;

        static async Task Main(string[] args)
        {
            //linux support??
            //https://stackoverflow.com/questions/53894813/how-to-use-console-setwindowsize-on-linux-using-net-core
            if (OperatingSystem.IsWindows())
            {
                //Console.WriteLine(Console.LargestWindowHeight);
                //Console.WriteLine(Console.LargestWindowWidth);
                //Console.SetWindowSize(150,40);
                Console.SetWindowSize(150,50);
                //Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
                
            }

            ManageConsoleDisplay.DisplayHeader();

            //load XML Doc
            ManageXML.LoadXML("APIKEY.xml");

            //get api key from XML if empty set it
            apiKey = ManageXML.GetAPIKey();
            if (apiKey == "")
            {
                AnsiConsole.WriteLine("No Open Weather Map API key detected you need to input one. Or input nothing and press Enter to quit.");
                string inputApiKey = AnsiConsole.Prompt(new TextPrompt<string>("What's your [green]API Key[/]?").AllowEmpty());
                if (inputApiKey != "")
                {
                    ManageXML.SetAPIKey(inputApiKey);
                    apiKey = inputApiKey;
                }
                else
                {
                    System.Environment.Exit(0);
                }
            }

            CheckForSavedLocations(ManageSQL.GetSavedLocations());
   
            //if saved weather ask to display that or get new data
            if (ManageSavedWeatherText.GetCurrentLocationText() != "" && ManageSavedWeatherText.GetCurrentWeatherText() != "")
            {
                if (AnsiConsole.Confirm("There is saved weather data. Type y to display saved data or n to get new weather."))
                {
                    ManageConsoleDisplay.GetAndDisplaySavedWeather();
                }
                else
                {
                    await GetCurrentWeatherOrForecast(true,true);
                    ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                }
            }
            else
            {
                await GetCurrentWeatherOrForecast(true, true);
                ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
            }
            //Run the recurring fetch weather task -- GetChoice blocks main thread so have to start recurring fetch here
            CancellationTokenSource source = new CancellationTokenSource();
            Task updateWeatherRecurring = Task.Run(() => { RecurringWeather(TimeSpan.FromHours(1), source.Token); });

            // Ask for the user's choice
            string choice = GetChoice();

            //loop to keep application running and display choices.
            bool quit = false;
            while (quit == false)
            {
                int locationId;
                switch (choice)
                {
                    case "Add a new location":
                        List<string> newLocation = GetNewLocationInput();
                        //isDefault 0 false 1 true
                        ManageSQL.SaveLocation(newLocation[0], newLocation[1], newLocation[2],0);
                        choice = GetChoice();
                        break;
                    case "Switch default location":
                        locationId = ChooseLocation();
                        ManageSQL.ChangeDefaultLocation(locationId);
                        choice = GetChoice();
                        break;
                    case "Remove a saved location":
                        //get location ID to remove
                        locationId = ChooseLocation();
                        //remove it
                        ManageSQL.RemoveSavedLocation(locationId);
                        //check if last location was removed -- if true add new one

                        //check if default was removed -- if true get new default
                        int? rowCount = ManageSQL.GetLocationRowCount();
                        int? defaultRow = ManageSQL.HasDefaultLocation();
                        if(defaultRow == 0 && rowCount == 0)
                        {
                            GetAndSaveDefaultLocation();
                        }
                        else if (defaultRow == 0 && rowCount != 0)
                        {
                            AnsiConsole.WriteLine("You removed you default location please pick another one");
                            locationId = ChooseLocation();
                            ManageSQL.ChangeDefaultLocation(locationId);
                        }

                        choice = GetChoice();
                        break;
                    case "Update weather":
                        await GetCurrentWeatherOrForecast(true, true);
                        ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                        choice = GetChoice();
                        break;
                    case "Get weather from a saved location":
                        locationId = ChooseLocation();
                        await GetCurrentWeatherOrForecast(true, false, locationId);
                        ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                        choice = GetChoice();
                        break;
                    case "Display saved weather":
                        if (ManageSavedWeatherText.GetCurrentWeatherText() != "")
                        {
                            ManageConsoleDisplay.GetAndDisplaySavedWeather();
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[bold red]There is no saved weather data.[/]");
                        }
                        choice = GetChoice();
                        break;
                    case "Get 5 day forecast":
                        await GetCurrentWeatherOrForecast(false, true);
                        ManageConsoleDisplay.DisplayForecastWeather(location, forecastWeather);
                        choice = GetChoice();
                        break;
                    case "Get 5 day forecast from a saved location":
                        locationId = ChooseLocation();
                        await GetCurrentWeatherOrForecast(false, false, locationId);
                        ManageConsoleDisplay.DisplayForecastWeather(location, forecastWeather);
                        choice = GetChoice();
                        break;
                    case "Display saved forecast":
                        if (ManageSavedWeatherText.GetForecastText() != "")
                        {
                            ManageConsoleDisplay.GetAndDisplaySavedForecast();
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[bold red]There is no saved forecast data.[/]");
                        }
                        choice = GetChoice();
                        break;
                    case "List all saved locations":
                        ListAllSavedLocations();
                        choice = GetChoice();
                        break;
                    case "Clear Console":
                        ClearConsole();
                        choice = GetChoice();
                        break;
                    case "Cancel Recurring Weather Update":
                        CancelRecurringWeather(source, updateWeatherRecurring);
                        choice = GetChoice();
                        break;
                    case "Quit":
                        CancelRecurringWeather(source, updateWeatherRecurring);
                        quit = true;
                        break;
                }
            }

            System.Environment.Exit(0);
        }

        private static void ListAllSavedLocations()
        {
            savedLocationsList = ManageSQL.GetSavedLocations();
            foreach (SavedLocations  location in savedLocationsList)
            {
                AnsiConsole.WriteLine($"{location.City} -- {location.StateCode} -- {location.CountryCode} -- default = {location.IsDefalut}");
            }
            AnsiConsole.WriteLine("");
        }
        
        private static List<string> GetNewLocationInput()
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
                    else if (Regex.IsMatch(stateCode, @"\d") || (Regex.IsMatch(stateCode, @"^[a-zA-Z0-9 ]*$")==false) || (stateCode.Length != 2 && stateCode != "notUS"))
                    {
                        return ValidationResult.Error("[red]All US state codes are 2 characters long and do NOT contain numbers. Outside the US? Enter (case sensitive) notUS[/]");
                    }
                    else
                    {
                        return ValidationResult.Success();
                    }
                        
                }));
        }

        private static string GetChoice()
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Please select an option below")
                    .PageSize(5)
                    .MoreChoicesText("[green](Move up and down to reveal more choices)[/]")
                    .AddChoices(new[] {
                        "Clear Console","Update weather","Get weather from a saved location","Display saved weather","Get 5 day forecast",
                        "Get 5 day forecast from a saved location","Display saved forecast","Add a new location", 
                        "Switch default location", "Remove a saved location","List all saved locations","Cancel Recurring Weather Update","Quit"
                    }));
            return choice;
        }
        
        private static int ChooseLocation()
        {
            savedLocationsList = ManageSQL.GetSavedLocations();
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
       
        private static void CheckForSavedLocations(List<SavedLocations> savedLocationsList)
        {
            if (savedLocationsList.Count == 0)
            {
                GetAndSaveDefaultLocation();
            }
        }

        private static void GetAndSaveDefaultLocation()
        {
            AnsiConsole.WriteLine("No saved or default location found please enter one.");
            AnsiConsole.WriteLine("Note: The location you enter here will be your default location.");
            AnsiConsole.WriteLine("Note: If you removed all locations or removed your default location you will be immediately asked to add one -- the app needs location to work.");
            List<string> newLocation = GetNewLocationInput();

            //isDefault 0 false 1 true
            ManageSQL.SaveLocation(newLocation[0], newLocation[1], newLocation[2], 1);
        }

        private static void ClearConsole()
        {
            AnsiConsole.Clear();
            ManageConsoleDisplay.DisplayHeader();
        }
        
        private static async Task GetCurrentWeatherOrForecast(bool forCurrentWeather, bool defaultLocation, int? atLocationId = null)
        {
            location = await ManageAPICalls.GetLocation(forCurrentWeather, defaultLocation, atLocationId);
            if (forCurrentWeather)
            {
                currentWeather = await ManageAPICalls.GetCurrentWeather(location);
            }
            else
            {
                forecastWeather = await ManageAPICalls.GetForecast(location);
            }
        }

        private static async Task RecurringWeather(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellationToken);
                await GetCurrentWeatherOrForecast(true, true);
                ClearConsole();
                ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
            }
        }

        private static void CancelRecurringWeather(CancellationTokenSource source, Task updateWeatherRecurring)
        {
            if (!source.IsCancellationRequested)
            {
                source.Cancel();
                source.Dispose();
                updateWeatherRecurring.Dispose();
                AnsiConsole.WriteLine("Recurring weather update canceled.");
            }
            else if (source.IsCancellationRequested)
            {
                AnsiConsole.WriteLine("Recurring weather update already canceled.");
            }
        }
    }
}