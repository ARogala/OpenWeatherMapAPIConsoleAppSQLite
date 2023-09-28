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

        //SavedLocations.xml stores a list of saved city, stateCode, and countryCode
        //SavedLocation.cs is is the class that stores location values from ManageXML.GetSavedLocations() -- used for api to fetch weather
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
               
            //load XML Docs
            ManageXML.LoadXML("APIKEY.xml");
            //locationsDoc = LoadXML("SavedLocations.xml");
            ManageXML.LoadXML("SavedLocations.xml");

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

            //get locations from XML
            //if none get and save one
            savedLocationsList = ManageXML.GetSavedLocations();
            if (savedLocationsList.Count == 0)
            {
                ManageConsoleDisplay.DisplayHeader();
                AnsiConsole.WriteLine("No saved location found please enter one.");
                AnsiConsole.WriteLine("Note: The location you enter here will be your default location.");
                List<string> newLocation = GetNewLocationInput();
                
                ManageXML.SaveLocation(newLocation[0], newLocation[1], newLocation[2]);
               
                AnsiConsole.Clear();
            }
            
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
                    location = await ManageAPICalls.GetLocation(0, true);
                    currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                    ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                }
            }
            else
            {
                ManageConsoleDisplay.DisplayHeader();
                location = await ManageAPICalls.GetLocation(0, true);
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
                        ManageXML.SaveLocation(newLocation[0], newLocation[1], newLocation[2]);
                        AnsiConsole.Clear();
                        choice = GetChoice();
                        break;
                    case "Switch default location":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        ushort index1 = ChooseLocation();
                        ManageXML.ChangeDefaultLocation(index1);
                        //get weather for new default location
                        location = await ManageAPICalls.GetLocation(0, true);
                        currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                        ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                        choice = GetChoice();
                        break;
                    case "Remove a saved location":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        ushort index2 = ChooseLocation();
                        ManageXML.RemoveLocation(index2);
                        choice = GetChoice();
                        break;
                    case "Update weather":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        location = await ManageAPICalls.GetLocation(0, true);
                        currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                        ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);    
                        choice = GetChoice();
                        break;
                    case "Get weather from a saved location":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        ushort index3 = ChooseLocation();
                        location = await ManageAPICalls.GetLocation(index3, true);
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
                        location = await ManageAPICalls.GetLocation(0, false);
                        forecastWeather = await ManageAPICalls.GetForecast(location);
                        ManageConsoleDisplay.DisplayForecastWeather(location, forecastWeather);
                        choice = GetChoice();
                        break;
                    case "Get 5 day forecast from a saved location":
                        AnsiConsole.Clear();
                        ManageConsoleDisplay.DisplayHeader();
                        ushort index4 = ChooseLocation();
                        location = await ManageAPICalls.GetLocation(index4, false);
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
        }

        private static void ListAllSavedLocations()
        {
            savedLocationsList = ManageXML.GetSavedLocations();
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
                        "Add a new location", "Switch default location", "Remove a saved location",
                        "Update weather","Get weather from a saved location","Display saved weather","Get 5 day forecast",
                        "Get 5 day forecast from a saved location","Display saved forecast","List all saved locations","Quit"
                    }));
            return choice;
        }
        
        private static ushort ChooseLocation()
        {
            savedLocationsList = ManageXML.GetSavedLocations();
            SelectionPrompt<string> prompt = new SelectionPrompt<string>()
                .Title("Please select a location below")
                .PageSize(5)
                .MoreChoicesText("[green](Move up and down to reveal more choices)[/]");
            
            foreach (SavedLocations location in savedLocationsList)
            {
                prompt.AddChoice($"{location.City} -- {location.StateCode} -- {location.CountryCode}");
            }

            string choice = AnsiConsole.Prompt(prompt);
            //get index of choice
            ushort index = 0;
            foreach (SavedLocations location in savedLocationsList)
            {
                if ($"{location.City} -- {location.StateCode} -- {location.CountryCode}" == choice)
                {
                    break;
                }
                index++;
            }
            return index;
        }
       
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp, bool toLocal)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            if (toLocal)
            {
                dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            }
            else
            {
                dateTime = dateTime.AddSeconds(unixTimeStamp);
            }
            return dateTime;
        }

        public static double MetersToMiles(ushort meters)
        {
            return (meters / 1000) * 0.6213711922;
        }

        public static double mmToInch(float mm)
        {
            return (mm * 0.393701) / (10);
        }

        public static string WindDegToDir(float windDeg)
        {
            int deg = (int)Math.Round(windDeg, 0);
            
            if ((deg >= 349 && deg <= 360) || (deg >= 0 && deg <= 11))
            {
                return "N";
            }
            else if (deg >= 12 && deg <=34)
            {
                return "NNE";
            }
            else if (deg >= 35 && deg <= 56)
            {
                return "NE";
            }
            else if (deg >= 57  && deg <= 79)
            {
                return "ENE";
            }
            else if (deg >= 80 && deg <= 101)
            {
                return "E";
            }
            else if (deg >= 102 && deg <= 124)
            {
                return "ESE";
            }
            else if (deg >= 125 && deg <= 146)
            {
                return "SE";
            }
            else if (deg >= 147 && deg <= 169)
            {
                return "SSE";
            }
            else if (deg >= 170 && deg <= 191)
            {
                return "S";
            }
            else if (deg >= 192 && deg <= 214)
            {
                return "SSW";
            }
            else if (deg >= 215 && deg <= 236)
            {
                return "SW";
            }
            else if (deg >= 237 && deg <= 259)
            {
                return "WSW";
            }
            else if (deg >= 260 && deg <= 281)
            {
                return "W";
            }
            else if (deg >= 282 && deg <= 304)
            {
                return "WNW";
            }
            else if (deg >= 305 && deg <= 326)
            {
                return "NW";
            }
            // else if (deg >= 327 && deg <= 348) -- any remaining values will be in this range
            else
            {
                return "NNW";
            }
        }
    }
}