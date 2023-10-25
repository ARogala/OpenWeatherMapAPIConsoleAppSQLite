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
                        SELECT location_id,city_name,latitude,longitude,country,state,is_default
                        FROM locations
                    ";

                    try
                    {
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
        public static void SaveLocation(string city, string stateCode, string countryCode, int isDefault, float? latitude = null, float? longitude = null)
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
                        new SqliteParameter("$latNull", latitude == null ? DBNull.Value : latitude),
                        new SqliteParameter("$lonNull", longitude == null ? DBNull.Value : longitude),
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
        public static void ChangeDefaultLocation(int? newDefaultLocationId)
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
                        AnsiConsole.WriteException(e);
                    }
                }
            }
        }

        public static void RemoveSavedLocation(int? removeLocationId)
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
                catch(Exception e)
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
                    AnsiConsole.WriteException(e);
                }
            }
            return rowCount;
        }

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
                    AnsiConsole.WriteException(e);
                }
            }
            return rowCount;
        }
    }
}
