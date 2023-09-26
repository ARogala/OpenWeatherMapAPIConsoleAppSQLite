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
                DisplayHeader();
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
                DisplayHeader();
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
                    DisplayHeader();
                    GetAndDisplaySavedWeather();
                }
                else
                {
                    AnsiConsole.Clear();
                    DisplayHeader();
                    location = await ManageAPICalls.GetLocation(0, true);
                    currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                    DisplayCurrentWeather(location, currentWeather);
                }
            }
            else
            {
                DisplayHeader();
                location = await ManageAPICalls.GetLocation(0, true);
                currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                DisplayCurrentWeather(location, currentWeather);
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
                        DisplayHeader();
                        List<string> newLocation = GetNewLocationInput();
                        ManageXML.SaveLocation(newLocation[0], newLocation[1], newLocation[2]);
                        AnsiConsole.Clear();
                        choice = GetChoice();
                        break;
                    case "Switch default location":
                        AnsiConsole.Clear();
                        DisplayHeader();
                        ushort index1 = ChooseLocation();
                        ManageXML.ChangeDefaultLocation(index1);
                        //get weather for new default location
                        location = await ManageAPICalls.GetLocation(0, true);
                        currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                        DisplayCurrentWeather(location, currentWeather);
                        choice = GetChoice();
                        break;
                    case "Remove a saved location":
                        AnsiConsole.Clear();
                        DisplayHeader();
                        ushort index2 = ChooseLocation();
                        ManageXML.RemoveLocation(index2);
                        choice = GetChoice();
                        break;
                    case "Update weather":
                        AnsiConsole.Clear();
                        DisplayHeader();
                        location = await ManageAPICalls.GetLocation(0, true);
                        currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                        DisplayCurrentWeather(location, currentWeather);    
                        choice = GetChoice();
                        break;
                    case "Get weather from a saved location":
                        AnsiConsole.Clear();
                        DisplayHeader();
                        ushort index3 = ChooseLocation();
                        location = await ManageAPICalls.GetLocation(index3, true);
                        currentWeather = await ManageAPICalls.GetCurrentWeather(location);
                        DisplayCurrentWeather(location, currentWeather);
                        choice = GetChoice();
                        break;
                    case "Display saved weather":
                        AnsiConsole.Clear();
                        DisplayHeader();
                        GetAndDisplaySavedWeather();
                        choice = GetChoice();
                        break;
                    case "Get 5 day forecast":
                        AnsiConsole.Clear();
                        DisplayHeader();
                        location = await ManageAPICalls.GetLocation(0, false);
                        forecastWeather = await ManageAPICalls.GetForecast(location);
                        DisplayForecastWeather(location, forecastWeather);
                        choice = GetChoice();
                        break;
                    case "Get 5 day forecast from a saved location":
                        AnsiConsole.Clear();
                        DisplayHeader();
                        ushort index4 = ChooseLocation();
                        location = await ManageAPICalls.GetLocation(index4, false);
                        forecastWeather = await ManageAPICalls.GetForecast(location);
                        DisplayForecastWeather(location, forecastWeather);
                        choice = GetChoice();
                        break;
                    case "Display saved forecast":
                        AnsiConsole.Clear();
                        DisplayHeader();
                        if (ManageSavedWeatherText.GetForecastText() != "")
                        {
                            GetAndDisplaySavedForecast();
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[bold red]There is no saved forecast data.[/]");
                        }
                        choice = GetChoice();
                        break;
                    case "List all saved locations":
                        AnsiConsole.Clear();
                        DisplayHeader();
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
        private static void DisplayHeader()
        {
            Grid headerGrid = new Grid();
            headerGrid.AddColumn();
            headerGrid.AddRow(new FigletText("Weather App").Centered().Color(Color.Green));
            headerGrid.AddRow(Align.Center(new Panel("[green bold]Powered by: [link]https://openweathermap.org[/][/]").NoBorder()));
            AnsiConsole.Write(headerGrid);
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
       
        private static Panel LocationDisplayPanel(List<Location> location)
        {
            List<Markup> markup = new List<Markup>();
            //limit of 1 on api call so this List will always have length of 1
            foreach (Location loc in location)
            {
                markup.Add(new Markup($"[bold green]City: [/]{loc.Name}"));
                markup.Add(new Markup($"[bold green]Latitude: [/]{String.Format("{0:0.0000}", loc.Latitude)}"));
                markup.Add(new Markup($"[bold green]Longitude: [/]{String.Format("{0:0.0000}", loc.Longitude)}"));
                markup.Add(new Markup($"[bold green]Country: [/]{loc.Country}"));
                markup.Add(new Markup($"[bold green]State: [/]{loc.State}"));
            }
            Rows locationRows = new Rows(markup);
            Panel locationPanel = new Panel(locationRows);
            locationPanel.Header = new PanelHeader("Location:");
            locationPanel.Width = 30;
           
            return locationPanel;
        }
       
        private static Panel CurrentWeatherDisplayPanel(CurrentWeather currentWeather) 
        {
            List<Markup> markup = new List<Markup>();
            if (currentWeather != null)
            {
                List<WeatherRecord> wther = currentWeather.Weather;
                foreach (WeatherRecord weather in wther)
                {
                    //Console.WriteLine(weather.main);
                    //Console.WriteLine($"Current Conditions: {weather.description}");
                    //Console.WriteLine(weather.icon);
                    markup.Add(new Markup($"[bold green]Current Conditions: [/]{weather.description}"));
                }
                markup.Add(new Markup($"[bold green]Temperture: [/]{currentWeather.Main.temp} Degrees F"));
                markup.Add(new Markup($"[bold green]Temperature Feels Like: [/]{currentWeather.Main.feels_like} Degrees F"));
                markup.Add(new Markup($"[bold green]Minimal Currently Observed Temperature: [/]{currentWeather.Main.temp_min} Degrees F"));
                markup.Add(new Markup($"[bold green]Maximal Currently Observed Temperature: [/]{currentWeather.Main.temp_max} Degrees F"));
                markup.Add(new Markup("------------------------------------------------------------------------------------------------------------------"));
                markup.Add(new Markup("[blue]At the Earth’s surface the air pressure of the atmosphere is usually within the range 980 to 1030 hPa.[/]"));
                markup.Add(new Markup("[blue]In general, a rising barometer means improving weather. In general, a falling barometer means worsening weather[/]"));
                markup.Add(new Markup("[blue]one hPa = one millibar = one thousandth of a “bar”[/]"));
                markup.Add(new Markup("------------------------------------------------------------------------------------------------------------------"));
                markup.Add(new Markup($"[bold green]Pressure Sea Level hPa: [/]{currentWeather.Main.pressure} hPa"));
                //api not returning sea_level and grnd_level for weather call
                //markup.Add(new Markup($"[bold green]Pressure Sea Level hPa: [/]{currentWeather.Main.sea_level} hPa"));
                //markup.Add(new Markup($"[bold green]Pressure Ground Level hPa: [/]{currentWeather.Main.grnd_level} hPa"));
                markup.Add(new Markup($"[bold green]Humidity: [/]{currentWeather.Main.humidity} %"));
                markup.Add(new Markup("[blue]The maximum value of the visibility is 10 km or 10000 m or about 6.2137 miles[/]"));
                markup.Add(new Markup($"[bold green]Visibility: [/]{String.Format("{0:0.0000}", MetersToMiles(currentWeather.Visibility))} miles"));
                markup.Add(new Markup($"[bold green]Wind Speed: [/]{currentWeather.Wind.speed} miles/hr"));
                markup.Add(new Markup("[blue]The first and the most important thing to remember: wind direction is always determined " +
                    "by where the wind is blowing FROM, not where it is blowing towards. " +
                    "Meteorological wind direction is defined as the direction from which it originates. " +
                    "For example, a northerly wind blows from the north to the south. Wind direction is measured in degrees clockwise from due north. " +
                    "Hence, a wind coming from the south has a wind direction of 180 degrees; one from the east is 90 degrees.[/]"));
                markup.Add(new Markup($"[bold green]Wind Direction: [/]{currentWeather.Wind.deg} degrees"));
                markup.Add(new Markup($"[bold green]Wind Cardinal Direction: [/]{WindDegToDir(currentWeather.Wind.deg)}"));
                markup.Add(new Markup($"[bold green]Wind Gusts: [/]{currentWeather.Wind.gust} miles/hr"));
                markup.Add(new Markup($"[bold green]Cloud Cover: [/]{currentWeather.Clouds.all} %"));

                if (currentWeather.Rain != null)
                {
                    markup.Add(new Markup($"[bold green]Rain Volume Last Hour: [/]{String.Format("{0:0.0000}", mmToInch(currentWeather.Rain.hr1))} inches"));
                    markup.Add(new Markup($"[bold green]Rain Volume Last 3 Hours: [/]{String.Format("{0:0.0000}", mmToInch(currentWeather.Rain.hr3))} inches"));
                }
                if (currentWeather.Snow != null)
                {
                    markup.Add(new Markup($"[bold green]Snow Volume Last Hour: [/]{String.Format("{0:0.0000}", mmToInch(currentWeather.Snow.hr1))} inches"));
                    markup.Add(new Markup($"[bold green]Snow Volume Last 3 Hours: [/]{String.Format("{0:0.0000}", mmToInch(currentWeather.Snow.hr3))} inches"));
                }
                markup.Add(new Markup("[blue]Note: Time is converted to local time. So if you chosen location is far away from your location times will appear incorrectly.[/]"));
                markup.Add(new Markup($"[bold green]Time Of Data Calculation: [/]{UnixTimeStampToDateTime(currentWeather.UnixTimeStamp,true)}"));
                markup.Add(new Markup($"[bold green]Sunrise: [/]{UnixTimeStampToDateTime(currentWeather.SunRiseSetUnixStamp.sunrise,true)}"));
                markup.Add(new Markup($"[bold green]Sunset: [/]{UnixTimeStampToDateTime(currentWeather.SunRiseSetUnixStamp.sunset, true)}"));
                markup.Add(new Markup($"[bold green]City: [/]{currentWeather.Name}"));
            }

            Rows currentWeatherRows = new Rows(markup);
            Panel currentWeatherPanel = new Panel(currentWeatherRows);
            currentWeatherPanel.Header = new PanelHeader("Current Weather:");
            currentWeatherPanel.Width = 120;
            
            return currentWeatherPanel;
        }

        private static void DisplayCurrentWeather(List<Location> location, CurrentWeather currentWeather) 
        {
            Grid weatherGrid = new Grid();
            weatherGrid.AddColumn();
            weatherGrid.AddColumn();
            Panel locationPanel = LocationDisplayPanel(location);
            Panel currentWeatherPanel = CurrentWeatherDisplayPanel(currentWeather);
            weatherGrid.AddRow(locationPanel, currentWeatherPanel);
            AnsiConsole.Write(weatherGrid);
        }

        private static List<Panel> ForecastWeatherDisplayPanels(ForecastWeather forecastWeather)
        {
            List <Panel> forecastPanels = new List<Panel>();
            
            List<ForecastRecord> forecastList = forecastWeather.Forecast;
            foreach (ForecastRecord forecastRecord in forecastList)
            {
                List<Markup> markup = new List<Markup>();
                List<WeatherRecord> wther = forecastRecord.Weather;
                markup.Add(new Markup($"[bold green]Time of Data Forecasted: [/][bold blue]{forecastRecord.TimeDataForecastedISO_UTC}[/]"));
                markup.Add(new Markup($"[bold green]Time Of Data Forecasted: [/][bold blue]{UnixTimeStampToDateTime(forecastRecord.TimeDataForecastedUNIX_UTC, false)}[/]"));
                
                foreach (WeatherRecord weather in wther)
                {
                    markup.Add(new Markup($"[bold green]Conditions: [/][bold blue]{weather.description}[/]"));
                }
                markup.Add(new Markup($"[bold green]Probability of Precipitation: [/][bold blue]{forecastRecord.ProbabilityPrecipitation}[/]"));
                if (forecastRecord.Rain != null)
                {
                    markup.Add(new Markup($"[bold green]Rain Volume Last 3 Hours: [/][bold blue]{String.Format("{0:0.0000}", mmToInch(forecastRecord.Rain.hr3))} inches[/]"));
                }
                if (forecastRecord.Snow != null)
                {
                    markup.Add(new Markup($"[bold green]Snow Volume Last 3 Hours: [/][bold blue]{String.Format("{0:0.0000}", mmToInch(forecastRecord.Snow.hr3))} inches[/]"));
                }
                markup.Add(new Markup($"[bold green]Temperture: [/][bold blue]{forecastRecord.Main.temp} Degrees F[/]"));
                markup.Add(new Markup("--------------------------------------------------------------------------------------------"));
                
                markup.Add(new Markup($"[bold green]Pressure Sea Level hPa: [/]{forecastRecord.Main.pressure} hPa"));
                markup.Add(new Markup($"[bold green]Pressure Ground Level hPa: [/]{forecastRecord.Main.grnd_level} hPa"));
                markup.Add(new Markup($"[bold green]Humidity: [/]{forecastRecord.Main.humidity} %"));
                markup.Add(new Markup($"[bold green]Visibility: [/]{String.Format("{0:0.0000}", MetersToMiles(forecastRecord.Visibility))} miles"));
                markup.Add(new Markup($"[bold green]Wind Speed: [/]{forecastRecord.Wind.speed} miles/hr"));
                markup.Add(new Markup($"[bold green]Wind Direction: [/]{forecastRecord.Wind.deg} degrees"));
                markup.Add(new Markup($"[bold green]Wind Cardinal Direction: [/]{WindDegToDir(forecastRecord.Wind.deg)}"));
                markup.Add(new Markup($"[bold green]Wind Gusts: [/]{forecastRecord.Wind.gust} miles/hr"));
                markup.Add(new Markup($"[bold green]Cloud Cover: [/]{forecastRecord.Clouds.all} %"));
                
                
                markup.Add(new Markup($"[bold green]City: [/]{forecastWeather.City.name}"));
                markup.Add(new Markup($"[bold green]Population: [/]{String.Format("{0:n0}", forecastWeather.City.population)}"));
                markup.Add(new Markup($"[bold green]Sunrise: [/]{UnixTimeStampToDateTime(forecastWeather.City.sunrise, true)}"));
                markup.Add(new Markup($"[bold green]Sunset: [/]{UnixTimeStampToDateTime(forecastWeather.City.sunset, true)}"));

                Rows forecastRows = new Rows(markup);
                Panel forecastPanel = new Panel(forecastRows);
                forecastPanel.Header = new PanelHeader($"Forecast Weather: [bold blue]{UnixTimeStampToDateTime(forecastRecord.TimeDataForecastedUNIX_UTC, false).DayOfWeek}[/]");
                forecastPanel.Width = 50;
                forecastPanels.Add(forecastPanel);
            }
            return forecastPanels;
        }
        
        private static void DisplayForecastWeather(List<Location> location, ForecastWeather forecastWeather)
        {
            Panel locationPanel = LocationDisplayPanel(location);
            AnsiConsole.Write(locationPanel);
            List<Panel> forecastPanels = ForecastWeatherDisplayPanels(forecastWeather);
            foreach (Panel panel in forecastPanels)
            {
                AnsiConsole.Write(panel);
            }
        }
        private static void GetAndDisplaySavedWeather()
        {
            List<Location> location = JsonSerializer.Deserialize<List<Location>>(ManageSavedWeatherText.GetCurrentLocationText()) ?? new();
            CurrentWeather? currentWeather = JsonSerializer.Deserialize<CurrentWeather>(ManageSavedWeatherText.GetCurrentWeatherText());
            DisplayCurrentWeather(location, currentWeather);
        }

        private static void GetAndDisplaySavedForecast()
        {
            List<Location> location = JsonSerializer.Deserialize<List<Location>>(ManageSavedWeatherText.GetForecastLocationText()) ?? new();
            ForecastWeather? forecastWeather = JsonSerializer.Deserialize<ForecastWeather>(ManageSavedWeatherText.GetForecastText());
            DisplayForecastWeather(location, forecastWeather);
        }
        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp, bool toLocal)
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

        private static double MetersToMiles(ushort meters)
        {
            return (meters / 1000) * 0.6213711922;
        }

        private static double mmToInch(float mm)
        {
            return (mm * 0.393701) / (10);
        }

        private static string WindDegToDir(float windDeg)
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