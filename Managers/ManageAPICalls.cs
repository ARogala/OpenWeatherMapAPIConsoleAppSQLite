using OpenWeatherMap.Models;
using Spectre.Console;
using System.Net.Http.Headers;
using System.Text.Json;


namespace OpenWeatherMap.Managers
{
    internal static class ManageAPICalls
    {
        private static string apiKey = ManageXML.GetAPIKey();

        private static readonly HttpClient client = new HttpClient();

        private static int currentLocationId;
        private static async Task<List<Location>> GetLatLongCoordsAsync(HttpClient client, SavedLocations locationForWeather, bool forCurrentWeather)
        {
            string json;
            List<Location> location;
            if (locationForWeather.StateCode == "notUS")
            {
                json = await client.GetStringAsync($"http://api.openweathermap.org/geo/1.0/direct?q={locationForWeather.City},{locationForWeather.CountryCode}&limit=1&appid={apiKey}");
                if (forCurrentWeather)
                {
                    ManageSavedWeatherText.SaveCurrentLocationText(json);
                }
                else
                {
                    ManageSavedWeatherText.SaveForecastLocationText(json);
                }

                location = JsonSerializer.Deserialize<List<Location>>(json) ?? new();
            }
            else
            {
                json = await client.GetStringAsync($"http://api.openweathermap.org/geo/1.0/direct?q={locationForWeather.City},{locationForWeather.StateCode},{locationForWeather.CountryCode}&limit=1&appid={apiKey}");
                if (forCurrentWeather)
                {
                    ManageSavedWeatherText.SaveCurrentLocationText(json);
                }
                else
                {
                    ManageSavedWeatherText.SaveForecastLocationText(json);
                }

                location = JsonSerializer.Deserialize<List<Location>>(json) ?? new();
            }

            return location;
        }

        private static List<Location> GetLatLongCoordsTest(bool forCurrentWeather)
        {
            string json = @"[
                {
                ""name"": ""Highland Lakes"",
                ""lat"": 41.1732669,
                ""lon"": -74.45902935626548,
                ""country"": ""US"",
                ""state"": ""New Jersey""
                }
            ]";
            if (forCurrentWeather)
            {
                ManageSavedWeatherText.SaveCurrentLocationText(json);
            }
            else
            {
                ManageSavedWeatherText.SaveForecastLocationText(json);
            }

            List<Location> location = JsonSerializer.Deserialize<List<Location>>(json) ?? new();
            return location;

        }

        public static async Task<List<Location>> GetLocation(bool forCurrentWeather, bool defaultLocation, int? atLocationId = null)
        {
            List<Location> location = new List<Location>();

            List<SavedLocations> savedLocationsList = ManageSQL.GetSavedLocations();
            //default to id = 0 but need to get right loaction based on Id or defaultLocation bool value
            SavedLocations locationForWeather = savedLocationsList[0];
            if (defaultLocation)
            {
                foreach (SavedLocations savedLocation in savedLocationsList)
                {
                    if (savedLocation.IsDefalut == 1)
                    {
                        locationForWeather = savedLocation;
                        break;
                    }
                }
            }
            else
            {
                foreach (SavedLocations savedLocation in savedLocationsList)
                {
                    if (savedLocation.LocationId == atLocationId)
                    {
                        locationForWeather = savedLocation;
                        break;
                    }
                }
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
                if (forCurrentWeather)
                {
                    ManageSavedWeatherText.SaveCurrentLocationText(json);
                }
                else
                {
                    ManageSavedWeatherText.SaveForecastLocationText(json);
                }
            }
            else
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    location = await GetLatLongCoordsAsync(client, locationForWeather, forCurrentWeather);
                    //location = GetLatLongCoordsTest(forCurrentWeather);
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

        private static async Task<CurrentWeather> GetCurrentWeatherAsync(HttpClient client, string lat, string lon)
        {
            string json = await client.GetStringAsync($"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&units=imperial&appid={apiKey}");
            ManageSavedWeatherText.SaveCurrentWeatherText(json);
            CurrentWeather? currentWeather = JsonSerializer.Deserialize<CurrentWeather>(json);
            return currentWeather;
        }

        private static CurrentWeather GetCurrentWeatherTest()
        {
            string json = @"{
                ""coord"": {
                ""lon"": 10.99,
                ""lat"": 44.34
                  },
                  ""weather"": [
                    {
                      ""id"": 501,
                      ""main"": ""Rain"",
                      ""description"": ""moderate rain"",
                      ""icon"": ""10d""
                    }
                  ],
                  ""base"": ""stations"",
                  ""main"": {
                    ""temp"": 298.48,
                    ""feels_like"": 298.74,
                    ""temp_min"": 297.56,
                    ""temp_max"": 300.05,
                    ""pressure"": 1015,
                    ""humidity"": 64,
                    ""sea_level"": 1015,
                    ""grnd_level"": 933
                  },
                  ""visibility"": 10000,
                  ""wind"": {
                    ""speed"": 0.62,
                    ""deg"": 349,
                    ""gust"": 1.18
                  },
                  ""rain"": {
                    ""1h"": 3.16
                  },
                  ""clouds"": {
                    ""all"": 100
                  },
                  ""dt"": 1661870592,
                  ""sys"": {
                    ""type"": 2,
                    ""id"": 2075663,
                    ""country"": ""IT"",
                    ""sunrise"": 1661834187,
                    ""sunset"": 1661882248
                  },
                  ""timezone"": 7200,
                  ""id"": 3163858,
                  ""name"": ""Zocca"",
                  ""cod"": 200
            }";
            ManageSavedWeatherText.SaveCurrentWeatherText(json);
            CurrentWeather? currentWeather = JsonSerializer.Deserialize<CurrentWeather>(json);
            return currentWeather;

        }

        public static async Task<CurrentWeather> GetCurrentWeather(List<Location> location)
        {
            CurrentWeather currentWeather = null;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                //limit of 1 on api call so this location List will always have length of 1
                currentWeather = await GetCurrentWeatherAsync(client, location[0].Latitude.ToString(), location[0].Longitude.ToString());
                //currentWeather = GetCurrentWeatherTest();
                //save currentWeather to DB
                ManageSQL.SaveCurrentWeather(currentWeather, currentLocationId);
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            return currentWeather;
        }

        private static async Task<ForecastWeather> GetForecastAsync(HttpClient client, string lat, string lon)
        {
            string json = await client.GetStringAsync($"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&units=imperial&appid={apiKey}");
            ManageSavedWeatherText.SaveForecastText(json);
            ForecastWeather? forecastWeather = JsonSerializer.Deserialize<ForecastWeather>(json);
            return forecastWeather;

        }
        private static ForecastWeather GetForecastTest()
        {
            string json = @"{""cod"":""200"",""message"":0,""cnt"":40,""list"":[{""dt"":1695654000,""main"":{""temp"":57.63,""feels_like"":57.38,""temp_min"":56.71,""temp_max"":57.63,""pressure"":1017,""sea_level"":1017,""grnd_level"":1000,""humidity"":91,""temp_kf"":0.51},""weather"":[{""id"":501,""main"":""Rain"",""description"":""moderate rain"",""icon"":""10d""}],""clouds"":{""all"":100},""wind"":{""speed"":11.95,""deg"":42,""gust"":28.12},""visibility"":10000,""pop"":1,""rain"":{""3h"":3.72},""sys"":{""pod"":""d""},""dt_txt"":""2023-09-25 15:00:00""},{""dt"":1695664800,""main"":{""temp"":55.27,""feels_like"":54.91,""temp_min"":53.87,""temp_max"":55.27,""pressure"":1018,""sea_level"":1018,""grnd_level"":1001,""humidity"":94,""temp_kf"":0.78},""weather"":[{""id"":500,""main"":""Rain"",""description"":""light rain"",""icon"":""10d""}],""clouds"":{""all"":100},""wind"":{""speed"":12.46,""deg"":40,""gust"":27.09},""visibility"":10000,""pop"":1,""rain"":{""3h"":1.42},""sys"":{""pod"":""d""},""dt_txt"":""2023-09-25 18:00:00""},{""dt"":1695675600,""main"":{""temp"":55,""feels_like"":54.72,""temp_min"":55,""temp_max"":55,""pressure"":1020,""sea_level"":1020,""grnd_level"":1002,""humidity"":96,""temp_kf"":0},""weather"":[{""id"":500,""main"":""Rain"",""description"":""light rain"",""icon"":""10d""}],""clouds"":{""all"":100},""wind"":{""speed"":12.01,""deg"":42,""gust"":29.57},""visibility"":10000,""pop"":0.61,""rain"":{""3h"":0.77},""sys"":{""pod"":""d""},""dt_txt"":""2023-09-25 21:00:00""},{""dt"":1695686400,""main"":{""temp"":56.16,""feels_like"":55.94,""temp_min"":56.16,""temp_max"":56.16,""pressure"":1022,""sea_level"":1022,""grnd_level"":1004,""humidity"":95,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04n""}],""clouds"":{""all"":100},""wind"":{""speed"":11.56,""deg"":45,""gust"":27.34},""visibility"":10000,""pop"":0.48,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-26 00:00:00""},{""dt"":1695697200,""main"":{""temp"":54.1,""feels_like"":53.55,""temp_min"":54.1,""temp_max"":54.1,""pressure"":1023,""sea_level"":1023,""grnd_level"":1005,""humidity"":92,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04n""}],""clouds"":{""all"":100},""wind"":{""speed"":11.54,""deg"":40,""gust"":28.01},""visibility"":10000,""pop"":0.2,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-26 03:00:00""},{""dt"":1695708000,""main"":{""temp"":54.32,""feels_like"":53.64,""temp_min"":54.32,""temp_max"":54.32,""pressure"":1024,""sea_level"":1024,""grnd_level"":1006,""humidity"":89,""temp_kf"":0},""weather"":[{""id"":500,""main"":""Rain"",""description"":""light rain"",""icon"":""10n""}],""clouds"":{""all"":100},""wind"":{""speed"":11.59,""deg"":48,""gust"":27.42},""visibility"":10000,""pop"":0.28,""rain"":{""3h"":0.11},""sys"":{""pod"":""n""},""dt_txt"":""2023-09-26 06:00:00""},{""dt"":1695718800,""main"":{""temp"":54.82,""feels_like"":54.23,""temp_min"":54.82,""temp_max"":54.82,""pressure"":1024,""sea_level"":1024,""grnd_level"":1006,""humidity"":90,""temp_kf"":0},""weather"":[{""id"":500,""main"":""Rain"",""description"":""light rain"",""icon"":""10n""}],""clouds"":{""all"":100},""wind"":{""speed"":11.1,""deg"":42,""gust"":25.64},""visibility"":10000,""pop"":0.47,""rain"":{""3h"":0.11},""sys"":{""pod"":""n""},""dt_txt"":""2023-09-26 09:00:00""},{""dt"":1695729600,""main"":{""temp"":53.24,""feels_like"":52.74,""temp_min"":53.24,""temp_max"":53.24,""pressure"":1026,""sea_level"":1026,""grnd_level"":1008,""humidity"":95,""temp_kf"":0},""weather"":[{""id"":500,""main"":""Rain"",""description"":""light rain"",""icon"":""10d""}],""clouds"":{""all"":100},""wind"":{""speed"":9.78,""deg"":31,""gust"":25.81},""visibility"":10000,""pop"":0.77,""rain"":{""3h"":0.72},""sys"":{""pod"":""d""},""dt_txt"":""2023-09-26 12:00:00""},{""dt"":1695740400,""main"":{""temp"":52.72,""feels_like"":52.2,""temp_min"":52.72,""temp_max"":52.72,""pressure"":1027,""sea_level"":1027,""grnd_level"":1009,""humidity"":96,""temp_kf"":0},""weather"":[{""id"":500,""main"":""Rain"",""description"":""light rain"",""icon"":""10d""}],""clouds"":{""all"":100},""wind"":{""speed"":10.87,""deg"":34,""gust"":27.11},""visibility"":10000,""pop"":0.97,""rain"":{""3h"":2.24},""sys"":{""pod"":""d""},""dt_txt"":""2023-09-26 15:00:00""},{""dt"":1695751200,""main"":{""temp"":52.7,""feels_like"":52.18,""temp_min"":52.7,""temp_max"":52.7,""pressure"":1027,""sea_level"":1027,""grnd_level"":1009,""humidity"":96,""temp_kf"":0},""weather"":[{""id"":500,""main"":""Rain"",""description"":""light rain"",""icon"":""10d""}],""clouds"":{""all"":100},""wind"":{""speed"":10.27,""deg"":41,""gust"":25.61},""visibility"":10000,""pop"":0.97,""rain"":{""3h"":1.18},""sys"":{""pod"":""d""},""dt_txt"":""2023-09-26 18:00:00""},{""dt"":1695762000,""main"":{""temp"":52.77,""feels_like"":52.21,""temp_min"":52.77,""temp_max"":52.77,""pressure"":1027,""sea_level"":1027,""grnd_level"":1009,""humidity"":95,""temp_kf"":0},""weather"":[{""id"":500,""main"":""Rain"",""description"":""light rain"",""icon"":""10d""}],""clouds"":{""all"":100},""wind"":{""speed"":8.12,""deg"":42,""gust"":22.7},""visibility"":10000,""pop"":0.36,""rain"":{""3h"":0.36},""sys"":{""pod"":""d""},""dt_txt"":""2023-09-26 21:00:00""},{""dt"":1695772800,""main"":{""temp"":52.36,""feels_like"":51.48,""temp_min"":52.36,""temp_max"":52.36,""pressure"":1028,""sea_level"":1028,""grnd_level"":1009,""humidity"":89,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04n""}],""clouds"":{""all"":100},""wind"":{""speed"":7.09,""deg"":51,""gust"":23.04},""visibility"":10000,""pop"":0.1,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-27 00:00:00""},{""dt"":1695783600,""main"":{""temp"":51.67,""feels_like"":50.54,""temp_min"":51.67,""temp_max"":51.67,""pressure"":1028,""sea_level"":1028,""grnd_level"":1010,""humidity"":85,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04n""}],""clouds"":{""all"":100},""wind"":{""speed"":6.51,""deg"":39,""gust"":20.18},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-27 03:00:00""},{""dt"":1695794400,""main"":{""temp"":47.57,""feels_like"":44.6,""temp_min"":47.57,""temp_max"":47.57,""pressure"":1028,""sea_level"":1028,""grnd_level"":1009,""humidity"":86,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04n""}],""clouds"":{""all"":88},""wind"":{""speed"":6.31,""deg"":19,""gust"":19.89},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-27 06:00:00""},{""dt"":1695805200,""main"":{""temp"":44.37,""feels_like"":40.39,""temp_min"":44.37,""temp_max"":44.37,""pressure"":1027,""sea_level"":1027,""grnd_level"":1009,""humidity"":80,""temp_kf"":0},""weather"":[{""id"":801,""main"":""Clouds"",""description"":""few clouds"",""icon"":""02n""}],""clouds"":{""all"":15},""wind"":{""speed"":7.02,""deg"":28,""gust"":23.38},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-27 09:00:00""},{""dt"":1695816000,""main"":{""temp"":46.09,""feels_like"":42.31,""temp_min"":46.09,""temp_max"":46.09,""pressure"":1028,""sea_level"":1028,""grnd_level"":1010,""humidity"":74,""temp_kf"":0},""weather"":[{""id"":800,""main"":""Clear"",""description"":""clear sky"",""icon"":""01d""}],""clouds"":{""all"":8},""wind"":{""speed"":7.36,""deg"":38,""gust"":23.73},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-27 12:00:00""},{""dt"":1695826800,""main"":{""temp"":57.56,""feels_like"":55.6,""temp_min"":57.56,""temp_max"":57.56,""pressure"":1028,""sea_level"":1028,""grnd_level"":1010,""humidity"":55,""temp_kf"":0},""weather"":[{""id"":800,""main"":""Clear"",""description"":""clear sky"",""icon"":""01d""}],""clouds"":{""all"":1},""wind"":{""speed"":8.32,""deg"":67,""gust"":12.95},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-27 15:00:00""},{""dt"":1695837600,""main"":{""temp"":63.88,""feels_like"":61.93,""temp_min"":63.88,""temp_max"":63.88,""pressure"":1026,""sea_level"":1026,""grnd_level"":1008,""humidity"":42,""temp_kf"":0},""weather"":[{""id"":802,""main"":""Clouds"",""description"":""scattered clouds"",""icon"":""03d""}],""clouds"":{""all"":26},""wind"":{""speed"":7.61,""deg"":75,""gust"":10.54},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-27 18:00:00""},{""dt"":1695848400,""main"":{""temp"":63.19,""feels_like"":61.47,""temp_min"":63.19,""temp_max"":63.19,""pressure"":1025,""sea_level"":1025,""grnd_level"":1007,""humidity"":48,""temp_kf"":0},""weather"":[{""id"":801,""main"":""Clouds"",""description"":""few clouds"",""icon"":""02d""}],""clouds"":{""all"":22},""wind"":{""speed"":5.84,""deg"":77,""gust"":9.4},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-27 21:00:00""},{""dt"":1695859200,""main"":{""temp"":53.26,""feels_like"":51.48,""temp_min"":53.26,""temp_max"":53.26,""pressure"":1026,""sea_level"":1026,""grnd_level"":1007,""humidity"":68,""temp_kf"":0},""weather"":[{""id"":802,""main"":""Clouds"",""description"":""scattered clouds"",""icon"":""03n""}],""clouds"":{""all"":31},""wind"":{""speed"":3.76,""deg"":32,""gust"":4.34},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-28 00:00:00""},{""dt"":1695870000,""main"":{""temp"":51.73,""feels_like"":50.27,""temp_min"":51.73,""temp_max"":51.73,""pressure"":1026,""sea_level"":1026,""grnd_level"":1008,""humidity"":78,""temp_kf"":0},""weather"":[{""id"":803,""main"":""Clouds"",""description"":""broken clouds"",""icon"":""04n""}],""clouds"":{""all"":84},""wind"":{""speed"":3.2,""deg"":9,""gust"":3.24},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-28 03:00:00""},{""dt"":1695880800,""main"":{""temp"":49.21,""feels_like"":47.19,""temp_min"":49.21,""temp_max"":49.21,""pressure"":1026,""sea_level"":1026,""grnd_level"":1007,""humidity"":86,""temp_kf"":0},""weather"":[{""id"":802,""main"":""Clouds"",""description"":""scattered clouds"",""icon"":""03n""}],""clouds"":{""all"":47},""wind"":{""speed"":5.14,""deg"":22,""gust"":10.8},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-28 06:00:00""},{""dt"":1695891600,""main"":{""temp"":45.95,""feels_like"":43.54,""temp_min"":45.95,""temp_max"":45.95,""pressure"":1026,""sea_level"":1026,""grnd_level"":1008,""humidity"":87,""temp_kf"":0},""weather"":[{""id"":800,""main"":""Clear"",""description"":""clear sky"",""icon"":""01n""}],""clouds"":{""all"":0},""wind"":{""speed"":4.9,""deg"":13,""gust"":9.73},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-28 09:00:00""},{""dt"":1695902400,""main"":{""temp"":48.06,""feels_like"":45.52,""temp_min"":48.06,""temp_max"":48.06,""pressure"":1027,""sea_level"":1027,""grnd_level"":1008,""humidity"":82,""temp_kf"":0},""weather"":[{""id"":801,""main"":""Clouds"",""description"":""few clouds"",""icon"":""02d""}],""clouds"":{""all"":11},""wind"":{""speed"":5.7,""deg"":26,""gust"":15.95},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-28 12:00:00""},{""dt"":1695913200,""main"":{""temp"":59.49,""feels_like"":57.96,""temp_min"":59.49,""temp_max"":59.49,""pressure"":1026,""sea_level"":1026,""grnd_level"":1008,""humidity"":60,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04d""}],""clouds"":{""all"":86},""wind"":{""speed"":7.18,""deg"":69,""gust"":11.36},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-28 15:00:00""},{""dt"":1695924000,""main"":{""temp"":65.59,""feels_like"":64.06,""temp_min"":65.59,""temp_max"":65.59,""pressure"":1025,""sea_level"":1025,""grnd_level"":1007,""humidity"":47,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04d""}],""clouds"":{""all"":90},""wind"":{""speed"":7.43,""deg"":86,""gust"":10.18},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-28 18:00:00""},{""dt"":1695934800,""main"":{""temp"":64.49,""feels_like"":63.23,""temp_min"":64.49,""temp_max"":64.49,""pressure"":1024,""sea_level"":1024,""grnd_level"":1006,""humidity"":55,""temp_kf"":0},""weather"":[{""id"":801,""main"":""Clouds"",""description"":""few clouds"",""icon"":""02d""}],""clouds"":{""all"":12},""wind"":{""speed"":5.82,""deg"":99,""gust"":8.48},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-28 21:00:00""},{""dt"":1695945600,""main"":{""temp"":55.53,""feels_like"":54.45,""temp_min"":55.53,""temp_max"":55.53,""pressure"":1025,""sea_level"":1025,""grnd_level"":1007,""humidity"":78,""temp_kf"":0},""weather"":[{""id"":802,""main"":""Clouds"",""description"":""scattered clouds"",""icon"":""03n""}],""clouds"":{""all"":39},""wind"":{""speed"":2.01,""deg"":110,""gust"":2.33},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-29 00:00:00""},{""dt"":1695956400,""main"":{""temp"":53.06,""feels_like"":52.2,""temp_min"":53.06,""temp_max"":53.06,""pressure"":1025,""sea_level"":1025,""grnd_level"":1007,""humidity"":88,""temp_kf"":0},""weather"":[{""id"":802,""main"":""Clouds"",""description"":""scattered clouds"",""icon"":""03n""}],""clouds"":{""all"":46},""wind"":{""speed"":2.39,""deg"":21,""gust"":2.73},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-29 03:00:00""},{""dt"":1695967200,""main"":{""temp"":52.75,""feels_like"":52.05,""temp_min"":52.75,""temp_max"":52.75,""pressure"":1025,""sea_level"":1025,""grnd_level"":1007,""humidity"":92,""temp_kf"":0},""weather"":[{""id"":803,""main"":""Clouds"",""description"":""broken clouds"",""icon"":""04n""}],""clouds"":{""all"":71},""wind"":{""speed"":3.62,""deg"":31,""gust"":4.21},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-29 06:00:00""},{""dt"":1695978000,""main"":{""temp"":53.55,""feels_like"":52.92,""temp_min"":53.55,""temp_max"":53.55,""pressure"":1025,""sea_level"":1025,""grnd_level"":1007,""humidity"":92,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04n""}],""clouds"":{""all"":100},""wind"":{""speed"":4.09,""deg"":32,""gust"":8.86},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-29 09:00:00""},{""dt"":1695988800,""main"":{""temp"":54.34,""feels_like"":53.37,""temp_min"":54.34,""temp_max"":54.34,""pressure"":1026,""sea_level"":1026,""grnd_level"":1007,""humidity"":83,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04d""}],""clouds"":{""all"":100},""wind"":{""speed"":5.48,""deg"":45,""gust"":14.67},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-29 12:00:00""},{""dt"":1695999600,""main"":{""temp"":60.98,""feels_like"":59.74,""temp_min"":60.98,""temp_max"":60.98,""pressure"":1026,""sea_level"":1026,""grnd_level"":1008,""humidity"":63,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04d""}],""clouds"":{""all"":98},""wind"":{""speed"":5.66,""deg"":77,""gust"":9.19},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-29 15:00:00""},{""dt"":1696010400,""main"":{""temp"":65.25,""feels_like"":63.91,""temp_min"":65.25,""temp_max"":65.25,""pressure"":1025,""sea_level"":1025,""grnd_level"":1007,""humidity"":52,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04d""}],""clouds"":{""all"":91},""wind"":{""speed"":4.74,""deg"":94,""gust"":6.82},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-29 18:00:00""},{""dt"":1696021200,""main"":{""temp"":63.39,""feels_like"":62.29,""temp_min"":63.39,""temp_max"":63.39,""pressure"":1025,""sea_level"":1025,""grnd_level"":1007,""humidity"":61,""temp_kf"":0},""weather"":[{""id"":804,""main"":""Clouds"",""description"":""overcast clouds"",""icon"":""04d""}],""clouds"":{""all"":86},""wind"":{""speed"":4.38,""deg"":93,""gust"":9.08},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-29 21:00:00""},{""dt"":1696032000,""main"":{""temp"":55.53,""feels_like"":54.45,""temp_min"":55.53,""temp_max"":55.53,""pressure"":1026,""sea_level"":1026,""grnd_level"":1008,""humidity"":78,""temp_kf"":0},""weather"":[{""id"":803,""main"":""Clouds"",""description"":""broken clouds"",""icon"":""04n""}],""clouds"":{""all"":66},""wind"":{""speed"":2.64,""deg"":73,""gust"":4.29},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-30 00:00:00""},{""dt"":1696042800,""main"":{""temp"":54,""feels_like"":53.13,""temp_min"":54,""temp_max"":54,""pressure"":1026,""sea_level"":1026,""grnd_level"":1008,""humidity"":86,""temp_kf"":0},""weather"":[{""id"":803,""main"":""Clouds"",""description"":""broken clouds"",""icon"":""04n""}],""clouds"":{""all"":75},""wind"":{""speed"":3.38,""deg"":31,""gust"":4.85},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-30 03:00:00""},{""dt"":1696053600,""main"":{""temp"":52.25,""feels_like"":51.49,""temp_min"":52.25,""temp_max"":52.25,""pressure"":1026,""sea_level"":1026,""grnd_level"":1008,""humidity"":92,""temp_kf"":0},""weather"":[{""id"":803,""main"":""Clouds"",""description"":""broken clouds"",""icon"":""04n""}],""clouds"":{""all"":53},""wind"":{""speed"":3.78,""deg"":7,""gust"":4.76},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-30 06:00:00""},{""dt"":1696064400,""main"":{""temp"":50.67,""feels_like"":49.75,""temp_min"":50.67,""temp_max"":50.67,""pressure"":1026,""sea_level"":1026,""grnd_level"":1008,""humidity"":92,""temp_kf"":0},""weather"":[{""id"":800,""main"":""Clear"",""description"":""clear sky"",""icon"":""01n""}],""clouds"":{""all"":9},""wind"":{""speed"":4.12,""deg"":358,""gust"":5.21},""visibility"":10000,""pop"":0,""sys"":{""pod"":""n""},""dt_txt"":""2023-09-30 09:00:00""},{""dt"":1696075200,""main"":{""temp"":52.88,""feels_like"":51.91,""temp_min"":52.88,""temp_max"":52.88,""pressure"":1026,""sea_level"":1026,""grnd_level"":1008,""humidity"":86,""temp_kf"":0},""weather"":[{""id"":800,""main"":""Clear"",""description"":""clear sky"",""icon"":""01d""}],""clouds"":{""all"":8},""wind"":{""speed"":3.44,""deg"":6,""gust"":8.77},""visibility"":10000,""pop"":0,""sys"":{""pod"":""d""},""dt_txt"":""2023-09-30 12:00:00""}],""city"":{""id"":5103396,""name"":""Rockaway"",""coord"":{""lat"":40.8923,""lon"":-74.4774},""country"":""US"",""population"":6438,""timezone"":-14400,""sunrise"":1695638884,""sunset"":1695682275}}";
            ManageSavedWeatherText.SaveForecastText(json);
            ForecastWeather? forecastWeather = JsonSerializer.Deserialize<ForecastWeather>(json);
            return forecastWeather;
        }

        public static async Task<ForecastWeather> GetForecast(List<Location> location)
        {
            ForecastWeather forecastWeather = null;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                //limit of 1 on api call so this location List will always have length of 1
                forecastWeather = await GetForecastAsync(client, location[0].Latitude.ToString(), location[0].Longitude.ToString());
                //forecastWeather = GetForecastTest();
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            return forecastWeather;
        }

    }
}
