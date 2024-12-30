using System.Text.Json;
using System.Text.RegularExpressions;
using OpenWeatherMap.Managers;
using OpenWeatherMap.Models;
using Spectre.Console;
using System.Threading;
using CoordinateSharp;
using System.Net.Http.Headers;

namespace OpenWeatherMap
{
    public enum GetLocationFor { weather, forecast, airPollution, celestial }
    internal class Program
    {
        //Weather.db is SQLite database it stores locations and weather events for the locations
        //SavedLocation.cs is is the class that stores location values from database
        //Location.cs is record class that stores api returned data

        //APIKEY.xml stores the apiKey
        //string apiKey is used for api call
        private static string apiKey = String.Empty;

        private static List<Location> location;
        private static CurrentWeather currentWeather;
        private static ForecastWeather forecastWeather;
        //app setting from data base
        private static Dictionary<string, bool> settings = new Dictionary<string, bool>();
        static async Task Main(string[] args)
        {
            //get app settings 
            settings = ManageSQL.GetSettings();
            
            //CoordinatSharp set default to local time
            GlobalSettings.Allow_Coordinate_DateTimeKind_Specification = true;
            //linux support??
            //https://stackoverflow.com/questions/53894813/how-to-use-console-setwindowsize-on-linux-using-net-core
            if (OperatingSystem.IsWindows())
            {
                //Console.WriteLine(Console.LargestWindowHeight);
                //Console.WriteLine(Console.LargestWindowWidth);
                //Console.SetWindowSize(150,40);
                //Console.SetWindowSize(150,55);
                Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
                
            }

            ManageConsoleDisplay.DisplayHeader(settings["Suppress Header"]);

            //load XML Doc
            ManageXML.LoadXML("APIKEY.xml");

            //get api key from XML if empty set it
            apiKey = ManageXML.GetAPIKey();
            if (apiKey == "")
            {
                string inputApiKey = ManageUserInput.APIKeyPrompt();
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

            //ensure the app starts with a default location
            CheckForSavedLocations(ManageSQL.GetSavedLocations());
   
            //if saved weather ask to display that or get new data
            if (ManageSavedWeatherText.GetCurrentLocationText() != "" && ManageSavedWeatherText.GetCurrentWeatherText() != "" && settings["Display Saved Weather"] == true)
            {
                if (AnsiConsole.Confirm("There is saved weather data. Type y to display saved data or n to get new weather."))
                {
                    ManageConsoleDisplay.GetAndDisplaySavedWeather();
                }
                else
                {
                    await GetCurrentWeatherOrForecast(GetLocationFor.weather,true);
                    ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                }
            }
            else
            {
                //comment out to stop calls for testing
                await GetCurrentWeatherOrForecast(GetLocationFor.weather, true);
                ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
            }
            //Run the recurring fetch weather task -- GetChoice blocks main thread so have to start recurring fetch here
            CancellationTokenSource recurringWeatherSource = new CancellationTokenSource();
            CancellationTokenSource recurringStatsAndCelestialSource = new CancellationTokenSource();
            CancellationTokenSource recurringDisplaySavedWeatherSource = new CancellationTokenSource();
            Task updateWeatherRecurring;
            Task updateStatsAndCelestialRecurring;
            Task updateDisplaySavedWeatherRecurring;
            if (settings["Recurring Update"] == true)
            {
                updateWeatherRecurring = Task.Run(() => { RecurringWeather(TimeSpan.FromHours(1), recurringWeatherSource.Token); });
                updateStatsAndCelestialRecurring = Task.Run(() => { RecurringStatsAndCelestial(TimeSpan.FromMinutes(14), recurringStatsAndCelestialSource.Token); });
                updateDisplaySavedWeatherRecurring = Task.Run(() => { RecurringDisplaySavedWeather(TimeSpan.FromMinutes(7), recurringDisplaySavedWeatherSource.Token); });
                //leave in to test recurring methods
                //updateWeatherRecurring = Task.Run(() => { RecurringWeather(TimeSpan.FromSeconds(60), recurringWeatherSource.Token); });
                //updateStatsAndCelestialRecurring = Task.Run(() => { RecurringStatsAndCelestial(TimeSpan.FromMilliseconds(14000), recurringStatsAndCelestialSource.Token); });
                //updateDisplaySavedWeatherRecurring = Task.Run(() => { RecurringDisplaySavedWeather(TimeSpan.FromMilliseconds(7000), recurringDisplaySavedWeatherSource.Token); });
            }
            else
            {
                updateWeatherRecurring = Task.CompletedTask;
                updateStatsAndCelestialRecurring = Task.CompletedTask;
                updateDisplaySavedWeatherRecurring = Task.CompletedTask;
            }

            //set the user's menu choice
            string menuSelection = settings["Extended Menu"] == false ? "short" : "extended";
            string choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
            
            //loop to keep application running and display choices.
            bool quit = false;
            while (quit == false)
            {
                int locationId;
                switch (choice)
                {
                    case "Add a new location":
                        List<string> newLocation = ManageUserInput.GetNewLocationInput();
                        //isDefault 0 false 1 true
                        ManageSQL.SaveLocation(newLocation[0], newLocation[1], newLocation[2],0);
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Switch default location":
                        locationId = ManageUserInput.ChooseLocation();
                        ManageSQL.ChangeDefaultLocation(locationId);
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Edit a saved location":
                        locationId = ManageUserInput.ChooseLocation();
                        List<string> editLocation = ManageUserInput.GetNewLocationInput();
                        ManageSQL.EditLocation(editLocation[0], editLocation[1], editLocation[2], locationId);
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Remove a saved location":
                        //get location ID to remove
                        locationId = ManageUserInput.ChooseLocation();
                        //remove it
                        ManageSQL.RemoveSavedLocation(locationId);
                        //check if last location was removed -- if true add new one

                        //check if default was removed -- if true get new default
                        int? rowCount = ManageSQL.GetLocationRowCount();
                        int? defaultRow = ManageSQL.HasDefaultLocation();
                        if(defaultRow == 0 && rowCount == 0)
                        {
                            List<string> newDefaultLocation = ManageUserInput.GetDefaultLocation();
                            //isDefault 0 false 1 true
                            ManageSQL.SaveLocation(newDefaultLocation[0], newDefaultLocation[1], newDefaultLocation[2], 1);
                        }
                        else if (defaultRow == 0 && rowCount != 0)
                        {
                            AnsiConsole.WriteLine("You removed your default location please pick another one");
                            locationId = ManageUserInput.ChooseLocation();
                            ManageSQL.ChangeDefaultLocation(locationId);
                        }

                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Update weather":
                        await GetCurrentWeatherOrForecast(GetLocationFor.weather, true);
                        ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Get weather from a saved location":
                        locationId = ManageUserInput.ChooseLocation();
                        await GetCurrentWeatherOrForecast(GetLocationFor.weather, false, locationId);
                        ManageConsoleDisplay.DisplayCurrentWeather(location, currentWeather);
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
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
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Get 8 hour weather statistics":
                        AnsiConsole.Write(GetStatistics(8));
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Get 12 hour weather statistics":
                        AnsiConsole.Write(GetStatistics(12));
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Get 24 hour weather statistics":
                        AnsiConsole.Write(GetStatistics(24));
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Get 5 day forecast":
                        await GetCurrentWeatherOrForecast(GetLocationFor.forecast, true);
                        ManageConsoleDisplay.DisplayForecastWeather(location, forecastWeather);
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Get 5 day forecast from a saved location":
                        locationId = ManageUserInput.ChooseLocation();
                        await GetCurrentWeatherOrForecast(GetLocationFor.forecast, false, locationId);
                        ManageConsoleDisplay.DisplayForecastWeather(location, forecastWeather);
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
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
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Get celestial data":
                        List<Location> defaultLocation = await ManageAPICalls.GetLocation(GetLocationFor.celestial, true);
                        AnsiConsole.Write(ManageConsoleDisplay.DisplayCelestialData(defaultLocation));
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Get celestial data from a saved location":
                        locationId = ManageUserInput.ChooseLocation();
                        List<Location> locationCelestial = await ManageAPICalls.GetLocation(GetLocationFor.celestial, false, locationId);
                        AnsiConsole.Write(ManageConsoleDisplay.DisplayCelestialData(locationCelestial));
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "List all saved locations":
                        ListAllSavedLocations();
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Settings":
                        //get new settings from user -- the list contains only the checked == true keys
                        List<string> newSettings = ManageUserInput.SettingsPrompt(settings);
                        //build a new setting dictionary containing the updated settings 
                        Dictionary<string, bool> newSettingsDict = NewSettings(newSettings);
                        //check if newSettingsDict is equal to the current settings -- method assumes that the dictionaries have the same keys
                        bool areEqual = settings.OrderBy(kv => kv.Key).SequenceEqual(newSettingsDict.OrderBy(kv => kv.Key));
                        //if not equal 
                        //save new setting to database
                        //then update the global settings variable, adjust menu choice, and cancel recurring update if that was selected
                        if (!areEqual)
                        {
                            ManageSQL.UpdateSettings(
                                    newSettingsDict["Display Saved Weather"] == true ? 1 : 0,
                                    newSettingsDict["Suppress Header"] == true ? 1 : 0,
                                    newSettingsDict["Recurring Update"] == true ? 1 : 0,
                                    newSettingsDict["Extended Menu"] == true ? 1 : 0
                                );
                            settings = ManageSQL.GetSettings();
                            menuSelection = settings["Extended Menu"] == false ? "short" : "extended";
                            if (settings["Recurring Update"] == false)
                            {
                                CancelRecurringWeather(recurringWeatherSource, updateWeatherRecurring);
                                CancelRecurringStatsAndCelestial(recurringStatsAndCelestialSource, updateStatsAndCelestialRecurring);
                                CancelRecurringDisplaySavedWeather(recurringDisplaySavedWeatherSource, updateDisplaySavedWeatherRecurring);
                            }
                        }
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Clear Console":
                        ClearConsole();
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Cancel Recurring Weather Update":
                        CancelRecurringWeather(recurringWeatherSource, updateWeatherRecurring);
                        CancelRecurringStatsAndCelestial(recurringStatsAndCelestialSource, updateStatsAndCelestialRecurring);
                        CancelRecurringDisplaySavedWeather(recurringDisplaySavedWeatherSource, updateDisplaySavedWeatherRecurring);
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Display more options":
                        menuSelection = "extended";
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Display short menu":
                        menuSelection = "short";
                        choice = menuSelection == "short" ? ManageUserInput.GetShortChoice() : ManageUserInput.GetChoice();
                        break;
                    case "Quit":
                        CancelRecurringWeather(recurringWeatherSource, updateWeatherRecurring);
                        CancelRecurringStatsAndCelestial(recurringStatsAndCelestialSource, updateStatsAndCelestialRecurring);
                        CancelRecurringDisplaySavedWeather(recurringDisplaySavedWeatherSource, updateDisplaySavedWeatherRecurring);
                        quit = true;
                        break;
                }
            }

            System.Environment.Exit(0);
        }

        private static void ListAllSavedLocations()
        {
            List<SavedLocations> savedLocationsList = ManageSQL.GetSavedLocations();
            foreach (SavedLocations  location in savedLocationsList)
            {
                AnsiConsole.WriteLine($"{location.City} -- {location.StateCode} -- {location.CountryCode} -- default = {location.IsDefalut}");
            }
            AnsiConsole.WriteLine("");
        }
        
        private static Dictionary<string, bool> NewSettings(List<string> newSettings)
        {
            List<string> settingsKeys = new List<string> { "Display Saved Weather", "Suppress Header", "Recurring Update", "Extended Menu" };
            Dictionary<string, bool> newSettingsDict = new Dictionary<string, bool>();
            foreach (string key in settingsKeys)
            {
                if (newSettings.Contains(key))
                {
                    newSettingsDict.Add(key, true);
                }
                else
                {
                    newSettingsDict.Add(key, false);
                }
            }

            return newSettingsDict;
        }

        private static void CheckForSavedLocations(List<SavedLocations> savedLocationsList)
        {
            if (savedLocationsList.Count == 0)
            {
                List<string> newDefaultLocation = ManageUserInput.GetDefaultLocation();
                //isDefault 0 false 1 true
                ManageSQL.SaveLocation(newDefaultLocation[0], newDefaultLocation[1], newDefaultLocation[2], 1);
            }
        }

        private static void ClearConsole()
        {
            AnsiConsole.Clear();
            ManageConsoleDisplay.DisplayHeader(settings["Suppress Header"]);
        }
        
        private static async Task GetCurrentWeatherOrForecast(GetLocationFor locationFor, bool defaultLocation, int? atLocationId = null)
        {
            location = await ManageAPICalls.GetLocation(locationFor, defaultLocation, atLocationId);
            if (locationFor == GetLocationFor.weather)
            {
                currentWeather = await ManageAPICalls.GetCurrentWeather(location);
            }
            else if(locationFor == GetLocationFor.forecast)
            {
                forecastWeather = await ManageAPICalls.GetForecast(location);
            }
        }

        private static Panel GetStatistics(int hours)
        {
            //get default locationId 
            int defaultLocationId = (int)ManageSQL.GetDefaultLocationId();
            //check that there is enough weather data points for default location to get stats
            int weatherRowCount = (int)ManageSQL.GetWeatherRowCountInTimeRange(hours, defaultLocationId);
            if (weatherRowCount == 0 || weatherRowCount == 1)
            {
                return ManageConsoleDisplay.DisplayStatisticsError();
            }
            else
            {
                //averages
                Dictionary<string, float> averages = ManageSQL.GetAverageValuesInTimeRange(hours, defaultLocationId);
                //get max min values
                Dictionary<string, float> maxMin = ManageSQL.GetMaxMinValuesInTimeRange(hours, defaultLocationId);
                //get totals values
                Dictionary<string, float> totals = ManageSQL.GetTotalValuesInTimeRange(hours, defaultLocationId);
                //return the stats panel
                return ManageConsoleDisplay.DisplayStatistics(averages, maxMin, totals, weatherRowCount);
            }
        }

        private static async Task RecurringWeather(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellationToken);
                await GetCurrentWeatherOrForecast(GetLocationFor.weather, true);
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

        private static async Task RecurringStatsAndCelestial(TimeSpan interval, CancellationToken cancellationToken)
        {
            ushort count = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                count++;
                await Task.Delay(interval, cancellationToken);
                //prevent overlap of API Recurring Weather JSON RESPONSE and Get Stats
                //ANYNUMBER % 1 == 0 if its a whole number
                //if minute/60 is a fraction show Get Stats else dont
                //see excel file for better explination
                if (((count * 14) / 60.00) % 1 != 0)
                {
                    ClearConsole();
                    List<Location> defaultLocation = await ManageAPICalls.GetLocation(GetLocationFor.celestial, true);
                    Grid statCelestialGrid = new Grid();
                    statCelestialGrid.AddColumn();
                    statCelestialGrid.AddColumn();
                    statCelestialGrid.AddRow(ManageConsoleDisplay.DisplayCelestialData(defaultLocation), GetStatistics(8));
                    AnsiConsole.Write(statCelestialGrid);
                }
                //reset count before ushort limit reached
                //keep it in line with Display saved count hence count*2
                if (count * 2 == 60000)
                {
                    count = 0;
                }
            }
        }
        
        private static void CancelRecurringStatsAndCelestial(CancellationTokenSource source, Task updateStatsAndCelestialRecurring)
        {
            if (!source.IsCancellationRequested)
            {
                source.Cancel();
                source.Dispose();
                updateStatsAndCelestialRecurring.Dispose();
                AnsiConsole.WriteLine("Recurring statistics and celestial update canceled");
            }
            else if (source.IsCancellationRequested)
            {
                AnsiConsole.WriteLine("Recurring statistics and celestial update already canceled");
            }
        }

        private static async Task RecurringDisplaySavedWeather(TimeSpan interval, CancellationToken cancellationToken)
        {
            ushort count = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                count++;
                await Task.Delay(interval, cancellationToken);
                //only display saved for odd intervals -- so display saved and get stats don't overlap
                if (count % 2 != 0)
                {
                    ClearConsole();
                    if (ManageSavedWeatherText.GetCurrentWeatherText() != "")
                    {
                        ManageConsoleDisplay.GetAndDisplaySavedWeather();
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[bold red]There is no saved weather data.[/]");
                    }
                }
                //reset count before ushort limit reached
                if (count == 60000)
                {
                    count = 0;
                }
            }
        }

        private static void CancelRecurringDisplaySavedWeather(CancellationTokenSource source, Task updateDisplaySavedWeatherRecurring)
        {
            if(!source.IsCancellationRequested)
            {
                source.Cancel(); 
                source.Dispose();
                updateDisplaySavedWeatherRecurring.Dispose();
                AnsiConsole.WriteLine("Recurring display saved weather update canceled");
            }
            else if(source.IsCancellationRequested)
            {
                AnsiConsole.WriteLine("Recurring display saved weather update already canceled");
            }
        }
    }
}