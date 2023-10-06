using Microsoft.Data.Sqlite;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenWeatherMap
{
    internal static class ManageSQL
    {
        private static string connectionString = "Data Source=Weather.db";
        private static SqliteConnection connection = new SqliteConnection(connectionString);
        public static List<SavedLocations> GetSavedLocations()
        {
            List<SavedLocations> locationsList = new List<SavedLocations>();
            try
            {
                using (connection)
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT city_name,state,country
                        FROM locations
                    ";

                    try
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string city_name = reader.GetString(0);
                                string state = reader.GetString(1);
                                string country = reader.GetString(2);
                              
                                locationsList.Add(new SavedLocations(city_name, state, country));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        AnsiConsole.WriteException(e);
                    }
                }
            }
            catch(Exception e)
            {
                AnsiConsole.WriteLine("Failed to get location from sqlite");
                AnsiConsole.WriteException(e);
            }
            return locationsList;
        }

        //isDefault 0 false 1 true
        public static void SaveLocation(string city, string stateCode, string countryCode, int isDefault)
        {
            try
            {
                using (connection)
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO locations(city_name, latitude, longitude, country, state, is_default) VALUES($city, $latNull, $lonNull, $countryCode, $stateCode, $isDefault);
                    ";
                    command.Parameters.AddRange(new[] { 
                        new SqliteParameter("$city", city),
                        new SqliteParameter("$latNull", DBNull.Value),
                        new SqliteParameter("$lonNull", DBNull.Value),
                        new SqliteParameter("$countryCode", countryCode),
                        new SqliteParameter("$stateCode", stateCode),
                        new SqliteParameter("$isDefault", isDefault)
                    });

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine("Failed to set location from sqlite");
                AnsiConsole.WriteException(e);
            }
        }
    }
}
