using Microsoft.Data.Sqlite;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

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
                        SELECT location_id,city_name,state,country
                        FROM locations
                    ";

                    try
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string locationId = reader.GetString(0);
                                string cityName = reader.GetString(1);
                                string state = reader.GetString(2);
                                string country = reader.GetString(3);
                              
                                locationsList.Add(new SavedLocations(cityName, state, country, locationId));
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

        //Set up as a Transaction -- it all happens or nothing happens
        //1 update old default location's is_default column to 0 
        //2 select choosen new default location and update its is_default column to 1
        //3 commit txn
        public static void ChangeDefaultLocation(SavedLocations newDefaultLocation)
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
                            WHERE city_name = $city AND state = $stateCode AND country = $countryCode;
                        ";
                        command.Parameters.AddRange(new[] {
                            new SqliteParameter("$city", newDefaultLocation.City),
                            new SqliteParameter("$stateCode", newDefaultLocation.StateCode),
                            new SqliteParameter("$countryCode", newDefaultLocation.CountryCode),
                        });
                        command.ExecuteNonQuery();
                        txn.Commit();
                    }
                    catch (Exception e)
                    {
                        txn.Rollback();
                        AnsiConsole.WriteException(e);
                    }
                }
            }
        }

        public static void RemoveSavedLocation(SavedLocations removeLocation)
        {
            using (connection)
            {
                try
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        DELETE FROM locations WHERE city_name = $city AND state = $stateCode AND country = $countryCode;
                    ";
                    command.Parameters.AddRange(new[] {
                        new SqliteParameter("$city", removeLocation.City),
                        new SqliteParameter("$stateCode", removeLocation.StateCode),
                        new SqliteParameter("$countryCode", removeLocation.CountryCode)  
                    });

                    command.ExecuteNonQuery();
                }
                catch(Exception e)
                {
                    AnsiConsole.WriteLine("Failed to remove a saved location");
                    AnsiConsole.WriteException(e);
                }
            }
        }
    }
}
