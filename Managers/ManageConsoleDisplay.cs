using OpenWeatherMap.Models;
using OpenWeatherMap.Utilities;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenWeatherMap.Managers
{
    internal static class ManageConsoleDisplay
    {
        public static void DisplayStatisticsError()
        {
            List<Markup> markup = new List<Markup>();
            markup.Add(new Markup(" "));
            markup.Add(new Markup("[bold red]Not enough weather data points to display an average[/]").Centered());
            markup.Add(new Markup(" "));
            Rows statRows = new Rows(markup);
            Panel statPanel = new Panel(statRows);
            statPanel.Header = new PanelHeader("Statistics:");
            statPanel.Width = 75;
            statPanel.Height = 38;
            AnsiConsole.Write(statPanel);
        }
        public static void DisplayStatistics(Dictionary<string,float> averages, Dictionary<string, float> maxMin, Dictionary<string, float> totals, int weatherRowCount)
        {
            List<Markup> markup = new List<Markup>();
            //row count
            markup.Add(new Markup(" "));
            markup.Add(new Markup($"[bold red]Statistics for the last {weatherRowCount} data points[/]").Centered());
            markup.Add(new Markup(" "));
            //average
            markup.Add(new Markup($"[bold green]Average Temperature: [/]{averages["Temperature"]} Degrees F"));
            markup.Add(new Markup($"[bold green]Average Pressure: [/]{averages["Pressure"]} hPa"));
            markup.Add(new Markup($"[bold green]Average Humidity: [/]{averages["Humidity"]} %"));
            markup.Add(new Markup($"[bold green]Average Wind Speed: [/]{averages["Wind Speed"]} miles/hr"));
            markup.Add(new Markup(" "));
            //max
            markup.Add(new Markup($"[bold green]Max Temperature: [/]{maxMin["Max Temperature"]} Degrees F"));
            markup.Add(new Markup($"[bold green]Max Pressure: [/]{maxMin["Max Pressure"]} hPa"));
            markup.Add(new Markup($"[bold green]Max Humidity: [/]{maxMin["Max Humidity"]} %"));
            markup.Add(new Markup($"[bold green]Max Wind Speed: [/]{maxMin["Max Wind Speed"]} miles/hour"));
            markup.Add(new Markup(" "));
            //min
            markup.Add(new Markup($"[bold green]Min Temperature: [/]{maxMin["Min Temperature"]} Degrees F"));
            markup.Add(new Markup($"[bold green]Min Pressure: [/]{maxMin["Min Pressure"]} hPa"));
            markup.Add(new Markup($"[bold green]Min Humidity: [/]{maxMin["Min Humidity"]} %"));
            markup.Add(new Markup($"[bold green]Min Wind Speed: [/]{maxMin["Min Wind Speed"]} miles/hour"));
            markup.Add(new Markup(" "));
            //totals
            foreach(var kvp in totals)
            {
                if(kvp.Key == "Rain")
                    markup.Add(new Markup($"[bold green]Total Rain: [/]{totals["Rain"]} inches"));
                if(kvp.Key == "Snow")
                    markup.Add(new Markup($"[bold green]Total Snow: [/]{totals["Snow"]} inches"));
            }
            
            
            markup.Add(new Markup(" "));

            Rows statRows = new Rows(markup);
            Panel statPanel = new Panel(statRows);
            statPanel.Header = new PanelHeader("Statistics:");
            statPanel.Width = 75;
            statPanel.Height = 38;
            AnsiConsole.Write(statPanel);
        }
        public static void DisplayHeader()
        {
            Grid headerGrid = new Grid();
            headerGrid.AddColumn();
            headerGrid.AddRow(new FigletText("Weather App").Centered().Color(Color.Green));
            headerGrid.AddRow(Align.Center(new Panel("[green bold]Powered by: [link]https://openweathermap.org[/][/]").NoBorder()));
            AnsiConsole.Write(headerGrid);
        }

        private static Panel LocationDisplayPanel(List<Location> location)
        {
            List<Markup> markup = new List<Markup>();
            //limit of 1 on api call so this List will always have length of 1
            foreach (Location loc in location)
            {
                markup.Add(new Markup($"[bold green]City: [/]{loc.Name}"));
                markup.Add(new Markup($"[bold green]Latitude: [/]{string.Format("{0:0.0000}", loc.Latitude)}"));
                markup.Add(new Markup($"[bold green]Longitude: [/]{string.Format("{0:0.0000}", loc.Longitude)}"));
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
                markup.Add(new Markup($"[bold green]Visibility: [/]{string.Format("{0:0.0000}", UnitConversions.MetersToMiles(currentWeather.Visibility))} miles"));
                markup.Add(new Markup($"[bold green]Wind Speed: [/]{currentWeather.Wind.speed} miles/hr"));
                markup.Add(new Markup("[blue]The first and the most important thing to remember: wind direction is always determined " +
                    "by where the wind is blowing FROM, not where it is blowing towards. " +
                    "Meteorological wind direction is defined as the direction from which it originates. " +
                    "For example, a northerly wind blows from the north to the south. Wind direction is measured in degrees clockwise from due north. " +
                    "Hence, a wind coming from the south has a wind direction of 180 degrees; one from the east is 90 degrees.[/]"));
                markup.Add(new Markup($"[bold green]Wind Direction: [/]{currentWeather.Wind.deg} degrees"));
                markup.Add(new Markup($"[bold green]Wind Cardinal Direction: [/]{UnitConversions.WindDegToDir(currentWeather.Wind.deg)}"));
                markup.Add(new Markup($"[bold green]Wind Gusts: [/]{currentWeather.Wind.gust} miles/hr"));
                markup.Add(new Markup($"[bold green]Cloud Cover: [/]{currentWeather.Clouds.all} %"));

                if (currentWeather.Rain != null)
                {
                    markup.Add(new Markup($"[bold green]Rain Volume Last Hour: [/]{string.Format("{0:0.0000}", UnitConversions.mmToInch(currentWeather.Rain.hr1))} inches"));
                    markup.Add(new Markup($"[bold green]Rain Volume Last 3 Hours: [/]{string.Format("{0:0.0000}", UnitConversions.mmToInch(currentWeather.Rain.hr3))} inches"));
                }
                if (currentWeather.Snow != null)
                {
                    markup.Add(new Markup($"[bold green]Snow Volume Last Hour: [/]{string.Format("{0:0.0000}", UnitConversions.mmToInch(currentWeather.Snow.hr1))} inches"));
                    markup.Add(new Markup($"[bold green]Snow Volume Last 3 Hours: [/]{string.Format("{0:0.0000}", UnitConversions.mmToInch(currentWeather.Snow.hr3))} inches"));
                }
                markup.Add(new Markup("[blue]Note: Time is converted to local time. So if you chosen location is far away from your location times will appear incorrectly.[/]"));
                markup.Add(new Markup($"[bold green]Time Of Data Calculation: [/]{UnitConversions.UnixTimeStampToDateTime(currentWeather.UnixTimeStamp, true)}"));
                markup.Add(new Markup($"[bold green]Sunrise: [/]{UnitConversions.UnixTimeStampToDateTime(currentWeather.SunRiseSetUnixStamp.sunrise, true)}"));
                markup.Add(new Markup($"[bold green]Sunset: [/]{UnitConversions.UnixTimeStampToDateTime(currentWeather.SunRiseSetUnixStamp.sunset, true)}"));
                markup.Add(new Markup($"[bold green]City: [/]{currentWeather.Name}"));
                markup.Add(new Markup($"[bold green]Latitude: [/]{string.Format("{0:0.0000}", currentWeather.Coord.lat)}"));
                markup.Add(new Markup($"[bold green]Longitude: [/]{string.Format("{0:0.0000}", currentWeather.Coord.lon)}"));
            }

            Rows currentWeatherRows = new Rows(markup);
            Panel currentWeatherPanel = new Panel(currentWeatherRows);
            currentWeatherPanel.Header = new PanelHeader("Current Weather:");
            currentWeatherPanel.Width = 120;
            currentWeatherPanel.Height = 38;

            return currentWeatherPanel;
        }

        public static void DisplayCurrentWeather(List<Location> location, CurrentWeather currentWeather)
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
            List<Panel> forecastPanels = new List<Panel>();

            List<ForecastRecord> forecastList = forecastWeather.Forecast;
            foreach (ForecastRecord forecastRecord in forecastList)
            {
                List<Markup> markup = new List<Markup>();
                List<WeatherRecord> wther = forecastRecord.Weather;
                markup.Add(new Markup($"[bold green]Time of Data Forecasted: [/][bold blue]{forecastRecord.TimeDataForecastedISO_UTC}[/]"));
                markup.Add(new Markup($"[bold green]Time Of Data Forecasted: [/][bold blue]{UnitConversions.UnixTimeStampToDateTime(forecastRecord.TimeDataForecastedUNIX_UTC, false)}[/]"));

                foreach (WeatherRecord weather in wther)
                {
                    markup.Add(new Markup($"[bold green]Conditions: [/][bold blue]{weather.description}[/]"));
                }
                markup.Add(new Markup($"[bold green]Probability of Precipitation: [/][bold blue]{forecastRecord.ProbabilityPrecipitation}[/]"));
                if (forecastRecord.Rain != null)
                {
                    markup.Add(new Markup($"[bold green]Rain Volume Last 3 Hours: [/][bold blue]{string.Format("{0:0.0000}", UnitConversions.mmToInch(forecastRecord.Rain.hr3))} inches[/]"));
                }
                if (forecastRecord.Snow != null)
                {
                    markup.Add(new Markup($"[bold green]Snow Volume Last 3 Hours: [/][bold blue]{string.Format("{0:0.0000}", UnitConversions.mmToInch(forecastRecord.Snow.hr3))} inches[/]"));
                }
                markup.Add(new Markup($"[bold green]Temperture: [/][bold blue]{forecastRecord.Main.temp} Degrees F[/]"));
                markup.Add(new Markup("--------------------------------------------------------------------------------------------"));

                markup.Add(new Markup($"[bold green]Pressure Sea Level hPa: [/]{forecastRecord.Main.pressure} hPa"));
                markup.Add(new Markup($"[bold green]Pressure Ground Level hPa: [/]{forecastRecord.Main.grnd_level} hPa"));
                markup.Add(new Markup($"[bold green]Humidity: [/]{forecastRecord.Main.humidity} %"));
                markup.Add(new Markup($"[bold green]Visibility: [/]{string.Format("{0:0.0000}", UnitConversions.MetersToMiles(forecastRecord.Visibility))} miles"));
                markup.Add(new Markup($"[bold green]Wind Speed: [/]{forecastRecord.Wind.speed} miles/hr"));
                markup.Add(new Markup($"[bold green]Wind Direction: [/]{forecastRecord.Wind.deg} degrees"));
                markup.Add(new Markup($"[bold green]Wind Cardinal Direction: [/]{UnitConversions.WindDegToDir(forecastRecord.Wind.deg)}"));
                markup.Add(new Markup($"[bold green]Wind Gusts: [/]{forecastRecord.Wind.gust} miles/hr"));
                markup.Add(new Markup($"[bold green]Cloud Cover: [/]{forecastRecord.Clouds.all} %"));


                markup.Add(new Markup($"[bold green]City: [/]{forecastWeather.City.name}"));
                markup.Add(new Markup($"[bold green]Population: [/]{string.Format("{0:n0}", forecastWeather.City.population)}"));
                markup.Add(new Markup($"[bold green]Sunrise: [/]{UnitConversions.UnixTimeStampToDateTime(forecastWeather.City.sunrise, true)}"));
                markup.Add(new Markup($"[bold green]Sunset: [/]{UnitConversions.UnixTimeStampToDateTime(forecastWeather.City.sunset, true)}"));

                Rows forecastRows = new Rows(markup);
                Panel forecastPanel = new Panel(forecastRows);
                forecastPanel.Header = new PanelHeader($"Forecast Weather: [bold blue]{UnitConversions.UnixTimeStampToDateTime(forecastRecord.TimeDataForecastedUNIX_UTC, false).DayOfWeek}[/]");
                forecastPanel.Width = 50;
                forecastPanels.Add(forecastPanel);
            }
            return forecastPanels;
        }

        public static void DisplayForecastWeather(List<Location> location, ForecastWeather forecastWeather)
        {
            Panel locationPanel = LocationDisplayPanel(location);
            AnsiConsole.Write(locationPanel);
            List<Panel> forecastPanels = ForecastWeatherDisplayPanels(forecastWeather);
            foreach (Panel panel in forecastPanels)
            {
                AnsiConsole.Write(panel);
            }
        }
        public static void GetAndDisplaySavedWeather()
        {
            List<Location> location = JsonSerializer.Deserialize<List<Location>>(ManageSavedWeatherText.GetCurrentLocationText()) ?? new();
            CurrentWeather? currentWeather = JsonSerializer.Deserialize<CurrentWeather>(ManageSavedWeatherText.GetCurrentWeatherText());
            DisplayCurrentWeather(location, currentWeather);
        }
        public static void GetAndDisplaySavedForecast()
        {
            List<Location> location = JsonSerializer.Deserialize<List<Location>>(ManageSavedWeatherText.GetForecastLocationText()) ?? new();
            ForecastWeather? forecastWeather = JsonSerializer.Deserialize<ForecastWeather>(ManageSavedWeatherText.GetForecastText());
            DisplayForecastWeather(location, forecastWeather);
        }
    }
}
