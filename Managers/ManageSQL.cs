using Microsoft.Data.Sqlite;
using OpenWeatherMap.Models;
using OpenWeatherMap.Utilities;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace OpenWeatherMap.Managers
{
    internal static class ManageSQL
    {
        private static string connectionString = $"Data Source={ManageFilePath.GetPath("Weather.db")}";
        private static SqliteConnection connection = new SqliteConnection(connectionString);

        //settings table set up to have a max of 1 row, thus id always 0
        //Will only ever UPDATE settings table. Never INSERT a new ROW
        public static Dictionary<string, bool> GetSettings()
        {
            int id = 0;
            Dictionary<string, bool> settings = new Dictionary<string, bool>();   
            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT display_saved_weather, suppress_header, recurring_update, extended_menu
                        FROM settings WHERE settings_id = $id
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$id",id)
                    });
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            settings.Add("Display Saved Weather", reader.GetInt32(0) == 1 ? true : false);
                            settings.Add("Suppress Header", reader.GetInt32(1) == 1 ? true : false);
                            settings.Add("Recurring Update", reader.GetInt32(2) == 1 ? true : false);
                            settings.Add("Extended Menu", reader.GetInt32(3) == 1 ? true : false); 
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to get settings");
                    AnsiConsole.WriteException(e);
                }
            }

            return settings;
        }

        public static void UpdateSettings(int displaySavedWeather, int suppressHeader, int recurringUpdate, int updateMenu)
        {
            int id = 0;
            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE settings SET display_saved_weather = $displaySavedWeather, suppress_header = $suppressHeader, 
                        recurring_update = $recurringUpdate, extended_menu = $updateMenu
                        WHERE settings_id = $id
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$displaySavedWeather",displaySavedWeather),
                        new SqliteParameter("$suppressHeader",suppressHeader),
                        new SqliteParameter("$recurringUpdate",recurringUpdate),
                        new SqliteParameter("$updateMenu",updateMenu),
                        new SqliteParameter("$id",id)
                    });
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to update settings");
                    AnsiConsole.WriteException(e);
                }
            }
        }
        public static List<SavedLocations> GetSavedLocations()
        {
            List<SavedLocations> locationsList = new List<SavedLocations>();

            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT location_id,city_name,latitude,longitude,country,state,is_default
                        FROM locations
                    ";

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int locationId = reader.GetInt32(0);
                            string cityName = reader.GetString(1);
                            float? latitude = reader.IsDBNull(2) ? null : reader.GetFloat(2);
                            float? longitude = reader.IsDBNull(3) ? null : reader.GetFloat(3);
                            string country = reader.GetString(4);
                            string state = reader.GetString(5);
                            int isDefault = reader.GetInt32(6);

                            locationsList.Add(new SavedLocations(cityName, state, country, locationId, isDefault, latitude, longitude));
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to get locations");
                    AnsiConsole.WriteException(e);
                }
            }

            return locationsList;
        }

        public static SavedLocations GetLocationAtId(int getLocationId)
        {
           
            SavedLocations location = new SavedLocations();
            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT location_id,city_name,latitude,longitude,country,state,is_default 
                        FROM locations WHERE location_id = $getLocationId;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$getLocationId", getLocationId)
                    });

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int locationId = reader.GetInt32(0);
                            string cityName = reader.GetString(1);
                            float? latitude = reader.IsDBNull(2) ? null : reader.GetFloat(2);
                            float? longitude = reader.IsDBNull(3) ? null : reader.GetFloat(3);
                            string country = reader.GetString(4);
                            string state = reader.GetString(5);
                            int isDefault = reader.GetInt32(6);

                            location = new SavedLocations(cityName,state,country,locationId,isDefault,latitude,longitude);
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine($"Failed to get location at id {getLocationId}");
                    AnsiConsole.WriteException(e);
                }
            }
            return location;
        }

        public static SavedLocations GetDefaultLocation()
        {
            SavedLocations location = new SavedLocations();
            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT location_id,city_name,latitude,longitude,country,state,is_default 
                        FROM locations WHERE is_default = $default;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$default", 1)
                    });

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int locationId = reader.GetInt32(0);
                            string cityName = reader.GetString(1);
                            float? latitude = reader.IsDBNull(2) ? null : reader.GetFloat(2);
                            float? longitude = reader.IsDBNull(3) ? null : reader.GetFloat(3);
                            string country = reader.GetString(4);
                            string state = reader.GetString(5);
                            int isDefault = reader.GetInt32(6);

                            location = new SavedLocations(cityName, state, country, locationId, isDefault, latitude, longitude);
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine($"Failed to get default location");
                    AnsiConsole.WriteException(e);
                }
            }
            return location;
        }
        //isDefault 0 false 1 true
        public static void SaveLocation(string city, string stateCode, string countryCode, int isDefault, float? latitude = null, float? longitude = null)
        {

            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO locations(city_name, latitude, longitude, country, state, is_default) VALUES($city, $latNull, $lonNull, $countryCode, $stateCode, $isDefault);
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$city", city),
                        new SqliteParameter("$latNull", latitude == null ? DBNull.Value : latitude),
                        new SqliteParameter("$lonNull", longitude == null ? DBNull.Value : longitude),
                        new SqliteParameter("$countryCode", countryCode),
                        new SqliteParameter("$stateCode", stateCode),
                        new SqliteParameter("$isDefault", isDefault)
                    });

                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to set location to sqlite");
                    AnsiConsole.WriteException(e);
                }
            }
        }

        public static void AddLatLonToLocation(float latitude, float longitude, int locationId)
        {
            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE locations SET latitude = $lat, longitude = $lon WHERE location_id = $locationId;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$lat", latitude),
                        new SqliteParameter("$lon", longitude),
                        new SqliteParameter("$locationId", locationId)
                    });

                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to update lat lon in locations table");
                    AnsiConsole.WriteException(e);
                }
            }
        }

        public static void SaveCurrentWeather(CurrentWeather currentWeather, int locationId)
        {
            string weatherDescription = string.Empty;
            List<WeatherRecord> wther = currentWeather.Weather;
            foreach (WeatherRecord weather in wther)
            {
                weatherDescription = weatherDescription + " " + weather.description;
            }
            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO weather(
                        location_id, 
                        latitude, 
                        longitude, 
                        city_name,
                        weather_description,
                        temperature_fahrenheit,
                        pressure_sea_level_hPa,
                        humidity,
                        visibility_miles,
                        wind_speed_miles_hr,
                        wind_direction_degrees,
                        wind_direction_cardinal,
                        wind_gust_miles_hr,
                        cloudiness_percent,
                        rain_volume_last_1hr_inch,
                        rain_volume_last_3hr_inch,
                        snow_volume_last_1hr_inch,
                        snow_volume_last_3hr_inch,
                        time_weather_data_calculated_unix_utc) 
                        VALUES(
                        $locationId, 
                        $lat, 
                        $lon, 
                        $city, 
                        $weatherDescription, 
                        $tempF,
                        $pressure,
                        $humidity,
                        $visibilityMiles,
                        $windSpeedMH,
                        $windDirDeg,
                        $windDirCard,
                        $windGustMH,
                        $cloudCover,
                        $rain1hr,
                        $rain3hr,
                        $snow1hr,
                        $snow3hr,
                        $timeWthCalcUNIXUTC);
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$locationId", locationId),
                        new SqliteParameter("$lat", currentWeather.Coord.lat),
                        new SqliteParameter("$lon", currentWeather.Coord.lon),
                        new SqliteParameter("$city", currentWeather.Name),
                        //description is in an array
                        new SqliteParameter("$weatherDescription", weatherDescription),
                        new SqliteParameter("$tempF", currentWeather.Main.temp),
                        new SqliteParameter("$pressure", currentWeather.Main.pressure),
                        new SqliteParameter("$humidity", currentWeather.Main.humidity),
                        new SqliteParameter("$visibilityMiles", UnitConversions.MetersToMiles(currentWeather.Visibility)),
                        new SqliteParameter("$windSpeedMH", currentWeather.Wind.speed),
                        new SqliteParameter("$windDirDeg", currentWeather.Wind.deg),
                        new SqliteParameter("$windDirCard", UnitConversions.WindDegToDir(currentWeather.Wind.deg)),
                        new SqliteParameter("$windGustMH", currentWeather.Wind.gust),
                        new SqliteParameter("$cloudCover", currentWeather.Clouds.all),
                        new SqliteParameter("$rain1hr", currentWeather.Rain == null ? DBNull.Value : UnitConversions.mmToInch(currentWeather.Rain.hr1)),
                        new SqliteParameter("$rain3hr", currentWeather.Rain == null ? DBNull.Value : UnitConversions.mmToInch(currentWeather.Rain.hr3)),
                        new SqliteParameter("$snow1hr", currentWeather.Snow == null ? DBNull.Value : UnitConversions.mmToInch(currentWeather.Snow.hr1)),
                        new SqliteParameter("$snow3hr", currentWeather.Snow == null ? DBNull.Value : UnitConversions.mmToInch(currentWeather.Snow.hr3)),
                        new SqliteParameter("$timeWthCalcUNIXUTC", currentWeather.UnixTimeStamp)
                    });

                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to save current weather to DB");
                    AnsiConsole.WriteException(e);
                }
            }
        }
        //Set up as a Transaction -- it all happens or nothing happens
        //1 update old default location's is_default column to 0 
        //2 select choosen new default location and update its is_default column to 1
        //3 commit txn
        public static void ChangeDefaultLocation(int newDefaultLocationId)
        {
            using (connection)
            {
                connection.Open();
                using (SqliteTransaction txn = connection.BeginTransaction())
                {
                    try
                    {
                        connection.Open();
                        SqliteCommand command = connection.CreateCommand();
                        command.CommandText =
                        @"
                            UPDATE locations
                            SET is_default = 0
                            WHERE is_default = 1;
                        ";
                        command.ExecuteNonQuery();
                        command.CommandText =
                        @"
                            UPDATE locations
                            SET is_default = 1
                            WHERE location_id = $newDefaultLocationId;
                        ";
                        command.Parameters.AddRange(new[] {
                            new SqliteParameter("$newDefaultLocationId", newDefaultLocationId)
                        });
                        command.ExecuteNonQuery();
                        txn.Commit();
                    }
                    catch (Exception e)
                    {
                        txn.Rollback();
                        AnsiConsole.WriteLine("Failed to change default location");
                        AnsiConsole.WriteException(e);
                    }
                }
            }
        }

        //may want to delete weather points for location after edit -- if location changes old weather no longer accurate
        public static void EditLocation(string city, string stateCode, string countryCode, int editLocationId)
        {
            using (connection)
            {
                try
                {
                    connection.Open();
                    //make sure lat and lon is null when updating -- need to get new lat lon 
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE locations SET city_name = $city, latitude = $lat, longitude = $lon, country = $countryCode, state = $stateCode WHERE location_id = $locationId;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$city", city),
                        new SqliteParameter("$lat", DBNull.Value),
                        new SqliteParameter("$lon", DBNull.Value),
                        new SqliteParameter("$countryCode", countryCode),
                        new SqliteParameter("$stateCode", stateCode),
                        new SqliteParameter("$locationId", editLocationId)
                    });

                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to update/edit location");
                    AnsiConsole.WriteException(e);
                }
            }
        }
        public static void RemoveSavedLocation(int removeLocationId)
        {
            using (connection)
            {
                try
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        DELETE FROM locations WHERE location_id = $removeLocationId;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$removeLocationId", removeLocationId)
                    });

                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to remove a saved location");
                    AnsiConsole.WriteException(e);
                }
            }
        }
        //return null or row count
        public static int? GetLocationRowCount()
        {
            int? rowCount = null;
            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT COUNT(*) FROM locations;
                    ";

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rowCount = reader.GetInt32(0);
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to get location row count");
                    AnsiConsole.WriteException(e);
                }
            }
            return rowCount;
        }
        //retruns null(if fails) 0 or 1
        public static int? HasDefaultLocation()
        {
            int? rowCount = null;
            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT COUNT(*) FROM locations WHERE is_default = $isDefault;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$isDefault", 1)
                    });
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rowCount = reader.GetInt32(0);
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to check for default location");
                    AnsiConsole.WriteException(e);
                }
            }
            return rowCount;
        }
        //returns default location id or null if operation fails
        public static int? GetDefaultLocationId()
        {
            int? defaultLocationId = null;
            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT location_id FROM locations WHERE is_default = $isDefault;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$isDefault", 1)
                    });
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            defaultLocationId = reader.GetInt32(0);
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to get default location id");
                    AnsiConsole.WriteException(e);
                }
            }
            return defaultLocationId;

        }

        public static int? GetWeatherRowCountInTimeRange(int hours, int locationId)
        {
            long unixStampNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long unixStampNowMinusHours = SubtractHoursUnixTimeStamp(unixStampNow, hours);
            int? rowCount = null;
            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT COUNT(*) FROM weather WHERE time_weather_data_calculated_unix_utc BETWEEN $startTime AND $endTime AND location_id == $locationId;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$startTime", unixStampNowMinusHours),
                        new SqliteParameter("$endTime", unixStampNow),
                        new SqliteParameter("$locationId", locationId)
                    });
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rowCount = reader.GetInt32(0);
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to check for default location");
                    AnsiConsole.WriteException(e);
                }
            }
            return rowCount;
        }

        //get the avg stats for the last X hours at a given location
        public static Dictionary<string, float> GetAverageValuesInTimeRange(int hours, int locationId)
        {
            Dictionary<string, float> averageStats = new Dictionary<string, float>();
            long unixStampNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long unixStampNowMinusHours = SubtractHoursUnixTimeStamp(unixStampNow, hours);
            
            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT avg(temperature_fahrenheit), avg(pressure_sea_level_hPa), avg(humidity), avg(wind_speed_miles_hr) FROM weather WHERE time_weather_data_calculated_unix_utc BETWEEN $startTime AND $endTime AND location_id == $locationId;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$startTime", unixStampNowMinusHours),
                        new SqliteParameter("$endTime", unixStampNow),
                        new SqliteParameter("$locationId", locationId)
                    });
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            averageStats.Add("Temperature", reader.GetFloat(0));
                            averageStats.Add("Pressure", reader.GetFloat(1));
                            averageStats.Add("Humidity", reader.GetFloat(2));
                            averageStats.Add("Wind Speed", reader.GetFloat(3));
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to get average stats");
                    AnsiConsole.WriteException(e);
                }
            }
            return averageStats;
        }

        //get max and min values
        public static Dictionary<string, float> GetMaxMinValuesInTimeRange(int hours, int locationId)
        {
            Dictionary<string, float> maxMinValues = new Dictionary<string, float>();
            long unixStampNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long unixStampNowMinusHours = SubtractHoursUnixTimeStamp(unixStampNow, hours);

            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT max(temperature_fahrenheit), max(pressure_sea_level_hPa), max(humidity), max(wind_speed_miles_hr), min(temperature_fahrenheit), min(pressure_sea_level_hPa), min(humidity), min(wind_speed_miles_hr) FROM weather WHERE time_weather_data_calculated_unix_utc BETWEEN $startTime AND $endTime AND location_id == $locationId;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$startTime", unixStampNowMinusHours),
                        new SqliteParameter("$endTime", unixStampNow),
                        new SqliteParameter("$locationId", locationId)
                    });
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            maxMinValues.Add("Max Temperature", reader.GetFloat(0));
                            maxMinValues.Add("Max Pressure", reader.GetFloat(1));
                            maxMinValues.Add("Max Humidity", reader.GetFloat(2));
                            maxMinValues.Add("Max Wind Speed", reader.GetFloat(3));

                            maxMinValues.Add("Min Temperature", reader.GetFloat(4));
                            maxMinValues.Add("Min Pressure", reader.GetFloat(5));
                            maxMinValues.Add("Min Humidity", reader.GetFloat(6));
                            maxMinValues.Add("Min Wind Speed", reader.GetFloat(7));
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to get max min stats");
                    AnsiConsole.WriteException(e);
                }
            }
            return maxMinValues;
        }

        //get sum rain snow totals
        public static Dictionary<string, float> GetTotalValuesInTimeRange(int hours, int locationId)
        {
            Dictionary<string, float> sumValues = new Dictionary<string, float>();
            long unixStampNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long unixStampNowMinusHours = SubtractHoursUnixTimeStamp(unixStampNow, hours);

            using (connection)
            {
                try
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT sum(rain_volume_last_1hr_inch), sum(snow_volume_last_1hr_inch) FROM weather WHERE time_weather_data_calculated_unix_utc BETWEEN $startTime AND $endTime AND location_id == $locationId;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$startTime", unixStampNowMinusHours),
                        new SqliteParameter("$endTime", unixStampNow),
                        new SqliteParameter("$locationId", locationId)
                    });
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                     
                        while (reader.Read())
                        {
                            if (reader.IsDBNull(0) == false)
                                sumValues.Add("Rain", reader.GetFloat(0));
                            if(reader.IsDBNull(1) == false)
                                sumValues.Add("Snow", reader.GetFloat(1));
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine("Failed to get sum values");
                    AnsiConsole.WriteException(e);
                }
            }
            return sumValues;
        }
        private static long SubtractHoursUnixTimeStamp(long unixTimeStamp, int hours)
        {
            return unixTimeStamp - (hours * 60 * 60);
        }
    }
}
