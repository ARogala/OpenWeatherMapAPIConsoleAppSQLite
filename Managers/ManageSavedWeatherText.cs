using Spectre.Console;

namespace OpenWeatherMap.Managers
{
    internal static class ManageSavedWeatherText
    {
        private static string appDirectory = Directory.GetCurrentDirectory();
        private static string dataDirectory = Directory.GetDirectories(appDirectory, "Data").First();
        public static void SaveCurrentWeatherText(string currentWeather)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter($"{GetPath("SavedCurrentWeather.txt")}", false))
                {
                    sw.Write(currentWeather);
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }

        }

        public static string GetCurrentWeatherText()
        {
            string weatherText = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader($"{GetPath("SavedCurrentWeather.txt")}"))
                {
                    weatherText = sr.ReadToEnd();
                    return weatherText;
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            return weatherText;

        }

        public static void SaveCurrentLocationText(string currentLocation)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter($"{GetPath("SavedCurrentLocation.txt")}", false))
                {
                    sw.Write(currentLocation);
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }

        }

        public static string GetCurrentLocationText()
        {
            string locationText = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader($"{GetPath("SavedCurrentLocation.txt")}"))
                {
                    locationText = sr.ReadToEnd();
                    return locationText;
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            return locationText;

        }

        public static void SaveForecastLocationText(string forecastLocation)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter($"{GetPath("SavedForecastLocation.txt")}", false))
                {
                    sw.Write(forecastLocation);
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
        }
        public static string GetForecastLocationText()
        {
            string locationText = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader($"{GetPath("SavedForecastLocation.txt")}"))
                {
                    locationText = sr.ReadToEnd();
                    return locationText;
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            return locationText;
        }
        public static void SaveForecastText(string forecast)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter($"{GetPath("SavedForecast.txt")}", false))
                {
                    sw.Write(forecast);
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
        }
        public static string GetForecastText()
        {
            string forecastText = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader($"{GetPath("SavedForecast.txt")}"))
                {
                    forecastText = sr.ReadToEnd();
                    return forecastText;
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            return forecastText;
        }

        private static string GetPath(string fileName)
        {
            return Directory.GetFiles(dataDirectory, $"{fileName}").First();
        }
    }
}
