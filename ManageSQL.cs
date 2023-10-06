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

        public static List<SavedLocations> GetSavedLocations()
        {
            List<SavedLocations> locationsList = new List<SavedLocations>();
            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
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
                                //int location_id = reader.GetInt32(0);
                                string city_name = reader.GetString(1);
                                //float latitude = reader.GetFloat(2);
                                //float longitude = reader.GetFloat(3);
                                string country = reader.GetString(4);
                                string state = reader.GetString(5);
                                //0 false 1 true
                                //short is_default = reader.GetInt16(6);

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
    }
}
