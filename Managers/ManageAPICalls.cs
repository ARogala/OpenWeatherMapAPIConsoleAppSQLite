using OpenWeatherMap.Models;
using Spectre.Console;
using System.Net.Http.Headers;
using System.Text.Json;

/* using Free API: https://openweathermap.org/price 
 * to get CurrentWeather or 3-hour Forecast 5 days we need to make a call to the Geocoding API first to convert City, State, Country to Lat Lon coords
 * App will have user enter at least one default location before any api call made -- City, State, Country and this will be saved in database -- uses SavedLocations class to map data
 */
namespace OpenWeatherMap.Managers
{
    internal static class ManageAPICalls
    {
        private static string apiKey = ManageXML.GetAPIKey();

        private static readonly HttpClient client = new HttpClient();

        private static int currentLocationId;

        /*
         * end point docs https://openweathermap.org/api/geocoding-api -- this gets lat lon from user City, State, Country input
         * use the geo api endpoint to get the location for USA or notUS
         * save the location Text -- app state
         * return the fetched Location
         */
        private static async Task<List<Location>> GetLatLongCoordsAsync(HttpClient client, SavedLocations locationForWeather, GetLocationFor locationFor)
        {
            string json;
            List<Location> location;
            if (locationForWeather.StateCode == "notUS")
            {
                json = await client.GetStringAsync($"http://api.openweathermap.org/geo/1.0/direct?q={locationForWeather.City},{locationForWeather.CountryCode}&limit=1&appid={apiKey}");
                if (locationFor == GetLocationFor.weather)
                {
                    ManageSavedWeatherText.SaveCurrentLocationText(json);
                }
                else if (locationFor == GetLocationFor.forecast)
                {
                    ManageSavedWeatherText.SaveForecastLocationText(json);
                }

                location = JsonSerializer.Deserialize<List<Location>>(json) ?? new();
            }
            else
            {
                json = await client.GetStringAsync($"http://api.openweathermap.org/geo/1.0/direct?q={locationForWeather.City},{locationForWeather.StateCode},{locationForWeather.CountryCode}&limit=1&appid={apiKey}");
                if (locationFor == GetLocationFor.weather)
                {
                    ManageSavedWeatherText.SaveCurrentLocationText(json);
                }
                else if (locationFor == GetLocationFor.forecast)
                {
                    ManageSavedWeatherText.SaveForecastLocationText(json);
                }

                //https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator
                // the ?? new() resolves the nullable warning
                location = JsonSerializer.Deserialize<List<Location>>(json) ?? new();
            }

            return location;
        }

        /*
         * This is the public method that will call GetLatLongCoordsAsync to get the location and lat lon needed for Weather and Forecast
         * if SavedLocation in database already has lat lon GetLatLongCoordsAsync will not be called 
         * 
         * 1. if defaultLocation true get the default location and set it equal to locationForWeather
         *    else get the location at the atLocationId and set it equal to locationForWeather  
         * 2. set the currentLocationId variable equal to the locationForWeather Id -- used to SaveCurrentWeather to database
         * 3. to prevent additional api calls if DB value locationForWeather has lat lon add locationForWeather to location list and save location text -- app state
         *    else get lat lon from api GetLatLongCoordsAsync and save lat lon to DB
         * 4. return the Location List -- length always 1
         * 
         * Note: GetLatLongCoordsAsync saves Location Text
         */
        public static async Task<List<Location>> GetLocation(GetLocationFor locationFor, bool defaultLocation, int? atLocationId = null)
        {
            //limit of 1 on api call so this location List will always have length of 1
            //new list each time -- length always 1
            List<Location> location = new List<Location>();
            SavedLocations locationForWeather = new SavedLocations();
            if(defaultLocation)
            {
                locationForWeather = ManageSQL.GetDefaultLocation();
            }
            else
            {
                locationForWeather = ManageSQL.GetLocationAtId((int)atLocationId);
            }
            //save current location id -- used to save current weather in GetCurrentWeather method
            currentLocationId = locationForWeather.LocationId;
            //to prevent additional api calls
            //if DB value locationForWeather has lat lon add locationForWeather to location list and save location text -- app state
            //else get lat lon from api and save lat lon to DB
            if (locationForWeather.Latitude != null && locationForWeather.Longitude != null)
            {
                location.Add(new Location(locationForWeather.City,
                    (float)locationForWeather.Latitude,
                    (float)locationForWeather.Longitude,
                    locationForWeather.CountryCode,
                    locationForWeather.StateCode)
                );
                string json = JsonSerializer.Serialize(location);
                //save to text for forecast and current weather
                if (locationFor == GetLocationFor.weather)
                {
                    ManageSavedWeatherText.SaveCurrentLocationText(json);
                }
                else if (locationFor == GetLocationFor.forecast)
                {
                    ManageSavedWeatherText.SaveForecastLocationText(json);
                }
            }
            //app state saved in GetLatLongCoordsAsync() so dont need to save text below
            else
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    location = await GetLatLongCoordsAsync(client, locationForWeather, locationFor);
                    //add lat lon to SQL DB
                    ManageSQL.AddLatLonToLocation(location[0].Latitude, location[0].Longitude, locationForWeather.LocationId);
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteException(e);
                }
            }

            return location;
        }

        /*
         * end point docs https://openweathermap.org/current
         * use the current weather endpoint to get the weather at the specific lat lon
         * save current weather to Text -- app state
         * return the fetched weather
         */
        private static async Task<CurrentWeather> GetCurrentWeatherAsync(HttpClient client, string lat, string lon)
        {
            string json = await client.GetStringAsync($"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&units=imperial&appid={apiKey}");
            ManageSavedWeatherText.SaveCurrentWeatherText(json);
            CurrentWeather? currentWeather = JsonSerializer.Deserialize<CurrentWeather>(json);
            return currentWeather;
        }

        /*
         * This is the public method that will call GetCurrentWeatherAsync to get weather at specific lat lon coords
         * 1. get current weather 
         * 2. save current weather to database
         * 3. return currentWeather
         */
        public static async Task<CurrentWeather> GetCurrentWeather(List<Location> location)
        {
            CurrentWeather currentWeather = null;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                //limit of 1 on api call so this location List will always have length of 1
                currentWeather = await GetCurrentWeatherAsync(client, location[0].Latitude.ToString(), location[0].Longitude.ToString());
                //save currentWeather to DB
                ManageSQL.SaveCurrentWeather(currentWeather, currentLocationId);
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            return currentWeather;
        }

        /*
         * end point docs https://openweathermap.org/forecast5
         * use the forecast endpoint to get the 5 day forecast at the specific lat lon
         * save the current forecast to Text -- app state
         * return the fetched forecast
         */
        private static async Task<ForecastWeather> GetForecastAsync(HttpClient client, string lat, string lon)
        {
            string json = await client.GetStringAsync($"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&units=imperial&appid={apiKey}");
            ManageSavedWeatherText.SaveForecastText(json);
            ForecastWeather? forecastWeather = JsonSerializer.Deserialize<ForecastWeather>(json);
            return forecastWeather;

        }
        
        /*
         * The is the public method that will call GetForecastAsync to get the forecast at specific lat lon coords
         * 1. get current forecast
         * 2. return current forecastWeather 
         * 
         * Note: Forecast is not saved to database
         */
        public static async Task<ForecastWeather> GetForecast(List<Location> location)
        {
            ForecastWeather forecastWeather = null;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                //limit of 1 on api call so this location List will always have length of 1
                forecastWeather = await GetForecastAsync(client, location[0].Latitude.ToString(), location[0].Longitude.ToString());
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            return forecastWeather;
        }

    }
}
