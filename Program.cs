using System.Text.Json;
using System.Text.RegularExpressions;
using Spectre.Console;

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
               
            //load XML Doc
            ManageXML.LoadXML("APIKEY.xml");

            //get api key from XML if empty set it
            apiKey = ManageXML.GetAPIKey();
            if (apiKey == "")
            {
                ManageConsoleDisplay.DisplayHeader();
                AnsiConsole.WriteLine("No Open Weather Map API key detected you need to input one.");
                string inputApiKey = AnsiConsole.Ask<string>("What's your [green]API Key[/]?");
                if (inputApiKey != "")
                {
                    ManageXML.SetAPIKey(inputApiKey);
                    apiKey = inputApiKey;
                }
                //clear console so Header doesnt display twice
                AnsiConsole.Clear();
            }

            CheckForSavedLocations(ManageSQL.GetSavedLocations());
   
            //if saved weather ask to display that or get new data
            if (ManageSavedWeatherText.GetCurrentLocationText() != "" && ManageSavedWeatherText.GetCurrentWeatherText() != "")
            {
                if (AnsiConsole.Confirm("There is saved weather data. Type y to display saved data or n to get new weather."))
                {
                    AnsiConsole.Clear();
                    ManageConsoleDisplay.DisplayHeader();
                    ManageConsoleDisplay.GetAndDisplaySavedWeather();
                }
                else
                {
                    AnsiConsole.Clear();
                    ManageConsoleDisplay.DisplayHeader();
                    location = await ManageAPICalls.GetLocation(true,true);
                    currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                    ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                }
            }
            else
            {
                ManageConsoleDisplay.DisplayHeader();
                location = await ManageAPICalls.GetLocation(true,true);
                currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
            }
            
           
            // Ask for the user's choice
            string choice = GetChoice();

            //loop to keep application running and display choices.
            bool quit = false;
            while (quit == false)
            {
                switch (choice)
                {
                    case "Add a new location":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        List<string> newLocation = GetNewLocationInput();
                        //isDefault 0 false 1 true
                        ManageSQL.SaveLocation(newLocation[0], newLocation[1], newLocation[2],0);
                        AnsiConsole.Clear();
                        choice = GetChoice();
                        break;
                    case "Switch default location":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        int? newDefaultLocationId1 = ChooseLocation();
                        ManageSQL.ChangeDefaultLocation(newDefaultLocationId1);
                        //get weather for new default location
                        location = await ManageAPICalls.GetLocation(true,true);
                        currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                        ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                        choice = GetChoice();
                        break;
                    case "Remove a saved location":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        //get location ID to remove
                        int? newDefaultLocationId2 = ChooseLocation();
                        //remove it
                        ManageSQL.RemoveSavedLocation(newDefaultLocationId2);
                        //check if last location was removed -- if true add new one
                        int? rowCount = ManageSQL.GetLocationRowCount();
                        if (rowCount == 0)
                        {
                            AnsiConsole.Clear();
                            GetAndSaveDefaultLocation();
                        }
                        //check if default was removed -- if true get new default
                        int? defaultRow = ManageSQL.HasDefaultLocation();
                        if(defaultRow == 0 && rowCount == 0)
                        {
                            AnsiConsole.Clear();
                            GetAndSaveDefaultLocation();
                        }
                        else if (defaultRow == 0 && rowCount != 0)
                        {
                            AnsiConsole.Clear();
                            ManageConsoleDisplay.DisplayHeader();
                            AnsiConsole.WriteLine("You removed you default location please pick another one");
                            int? newDefaultLocationId3 = ChooseLocation();
                            ManageSQL.ChangeDefaultLocation(newDefaultLocationId3);
                            AnsiConsole.Clear();
                            ManageConsoleDisplay.DisplayHeader();
                        }

                        choice = GetChoice();
                        break;
                    case "Update weather":
                        AnsiConsole.Clear();   
                        ManageConsoleDisplay.DisplayHeader();
                        location = await ManageAPICalls.GetLocation(true,true);
                        currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                        ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                        choice = GetChoice();
                        break;
                    case "Get weather from a saved location":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        ushort index3 = (ushort)ChooseLocation();
                        location = await ManageAPICalls.GetLocation(true,false,index3);
                        currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                        ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                        choice = GetChoice();
                        break;
                    case "Display saved weather":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        ManageConsoleDisplay.GetAndDisplaySavedWeather();
                        choice = GetChoice();
                        break;
                    case "Get 5 day forecast":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        location = await ManageAPICalls.GetLocation(false,true);
                        forecastWeather = await ManageAPICalls.GetForecast(location);
                        ManageConsoleDisplay.DisplayForecastWeather(location, forecastWeather);
                        choice = GetChoice();
                        break;
                    case "Get 5 day forecast from a saved location":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        ushort index4 = (ushort)ChooseLocation();
                        location = await ManageAPICalls.GetLocation(false,false,index4);
                        forecastWeather = await ManageAPICalls.GetForecast(location);
                        ManageConsoleDisplay.DisplayForecastWeather(location, forecastWeather);
                        choice = GetChoice();
                        break;
                    case "Display saved forecast":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
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
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        ListAllSavedLocations();
                        choice = GetChoice();
                        break;
                    case "Quit":
                        quit = true;
                        break;
                }
            }

            System.Environment.Exit(0);
        }

        private static void ListAllSavedLocations()
        {
            savedLocationsList = ManageSQL.GetSavedLocations();
            foreach (SavedLocations savedLocation in savedLocationsList)
            {
                AnsiConsole.WriteLine($"{savedLocation.City} -- {savedLocation.StateCode} -- {savedLocation.CountryCode}");
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
                        "Update weather","Get weather from a saved location","Display saved weather","Get 5 day forecast",
                        "Get 5 day forecast from a saved location","Display saved forecast","Add a new location", 
                        "Switch default location", "Remove a saved location","List all saved locations","Quit"
                    }));
            return choice;
        }
        
        private static int? ChooseLocation()
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
            int? newDefaultLocationId = null;
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
            ManageConsoleDisplay.DisplayHeader();
            AnsiConsole.WriteLine("No saved or default location found please enter one.");
            AnsiConsole.WriteLine("Note: The location you enter here will be your default location.");
            AnsiConsole.WriteLine("Note: If you removed all locations or removed your default location you will be immediately asked to add one -- the app needs location to work.");
            List<string> newLocation = GetNewLocationInput();

            //isDefault 0 false 1 true
            ManageSQL.SaveLocation(newLocation[0], newLocation[1], newLocation[2], 1);
            
            AnsiConsole.Clear();
        }
       
    }
}