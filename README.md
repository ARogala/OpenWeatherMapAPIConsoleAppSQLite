# OpenWeather Map Console Application with SQLite

## Powered by [OpenWeatherMap.org](https://openweathermap.org/)
## [Spectre Console](https://github.com/spectreconsole/spectre.console) for the display
## [Coordinat Sharp](https://coordinatesharp.com/) for the celestial data calculations
## [SQLite](https://www.sqlite.org/) for the database

## App screen shot

![App screen shot](AppScreenShot.png "App screen shot")

## Build Directions

1. Clone app
2. Rename APIKEY-Copy.xml to AIPKEY.xml
3. Get your API key from [OpenWeatherMap.org](https://openweathermap.org/)
4. Build or publish the app in Visual Studio
5. Run the exe and enjoy -- it will ask for your API key and default location
6. Or for Ubuntu run this command in terminal --> dotnet publish -c release -r ubuntu.22.04-x64 --self-contained
7. Running on Windows 11 -- go to System -> For Developers -> Terminal and set to Windows Console Host 
    * this allows the app to resize the console to the correct width and height

## Build/Publish Profiles -- Raspberry Pi, Windows, and Linux-x64

In Visual Studio Publish there is a 32 and 64 bit linux arm build that is set up for the Raspberry Pi.
The Pi 3 is a 64 bit machine but the standard OS is 32 bit so in that case use the 32 bit build option.
Running this on a single board PC is the best most efficient bet for building up a weather database.
There is also a standard linux x64 build that can be used for Ubuntu and of course Windows as well.
C# is awesome!

## About

This is a simple app that displays current weather and 5 day 3 hour forecast from OpenWeather API.
The app will save locations and weather data for those locations in a SQLite database. 
The API key is saved in an xml file.
The default location is always the first one entered when the app starts and is changeable.
Multiple locations can be added and entered locations can be edited. 
Locations can be deleted but note that deleting a location will drop all 
weather points saved in the database for that location.

Locations outside the US can be selected just follow the directions in the app.<br/>
Note: You may have to experiment a bit to get the correct location. Sometimes city names are not found.
For example, for some reason, the API wouldn't find Parker CO so I used Centennial CO -- a town or two north. 

Use [ISO 3166 country codes](https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes) 
and if in the US don't put periods or spaces between 
state abbreviations. For example Use NJ not N.J. For country use US not U.S. 

By default, on start up, the app will update the weather for the default location hourly and automatically save 
the current weather for that default location to the database.

Weather statistics and celestial data for the default location will also automatically be displayed every 14 minutes. 
Statistics and celestial data is not saved to database. 
This statistics feature needs at least two weather data points in the database and will just diplay
"Not enough weather data points to display an average" while the data is being collected.
The statistics feature is designed to calculate average and min/max values for the last 8 hours 
but will start calulating once 2 data points are found in the database within an 8 hour range.
If the user clicks "Update weather" repeatedly to get new weather points from the API this will throw off the 
8 hour average. This is becuse there will now be more data points for that time of day skewing the average temperature to 
the temperature of the time when the update button was repeatedly pressed. 
If the user would like to collect data and see 8 hour averages it is best 
to let the app run and collect the data automatically.

This hourly update feature can be stopped with an option in the extended menu and settings.
This would be useful for desktop use cases when the user just wants to check the weather 
and forecast occassionally throughout the day but is not looking to build a database of values. 

Both the hourly weather update and the statistics/celestial data features make use of C#'s multithreading 
capabilities to run tasks on recurring inervals.

There is a settings option to let the user
* Toggle the app from asking to display saved weather on start. Option "Display Saved Weather"
* Suppress or show the header. Option "Suppress Header"
* Prevent the recurring update from starting. Option "Recurring Update"
    * Note: Setting Recurring Update from false to true requires reboot 
* Display short or long menu by default. Option "Extended Menu"

The three OpenWeather API endpoints in use are 

1. [Current weather endpoint](https://openweathermap.org/current) 
    * https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={APIkey}
2. [5 day forecast endpoint](https://openweathermap.org/forecast5) 
    * https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&units=imperial&appid={APIkey}
3. [Geocoding endpoint](https://openweathermap.org/api/geocoding-api) 
    * http://api.openweathermap.org/geo/1.0/direct?q={cityName},{stateCode},{countryCode}&limit={limit}&appid={APIkey}

Overall the app is easy to use and a fast way to check the weather and forecast by you. 
Hope you enjoy it. I am hoping to have the time to make a similar mobile app.

## Motivation for building this app

* To continue learing software development and engineering. Some core concepts demonstrated are:
	* Multithreading with C# 
    	* An intersting problem was displaying and saving the updated weather hourly
        and then also showing statistics/celestial data every 14 minutes 
        (see ThreadDisplayCalculations.xlsx for solution)
	* Asynchronous programming and consuming data from an API
	* Basic SQL operations with C#
* Improve my code organization and documentation
* In addition to the educatational benifits I also just enjoy building pratical and useful applications
* A fast and efficient way to get weather and forecast data without 
  all the FULL PAGE ADVERTISEMENTS one encounters on many of the leading websites.
* Build a weather database for my local area to view historical weather
* Make use of a Rasbperry Pi I had collecting dust

## Data saved in text files

After each API call made to the current weather endpoint the location and current weather
will be saved in SavedCurrentLocation.txt and SavedCurrentWeather.txt this data is overwritten each time.<br/>
(options "Update weather" and "Get weather from a saved location")

This is to save the application state so API calls can be limited. 
The option "Display saved weather" pulls the data from the text files not SQL.

After each API call made to the forecast endpoint the location and 5 day forcast 
will be saved in SavedForecastLocation.txt and SavedForecast.txt this data is overwritten each time.<br/>
(options "Get 5 day forecast" and "Get 5 day forecast from a saved location") 

This is to save the application state so API calls can be limited. 
The option "Display saved forecast" pulls the data from the text files not SQL.

Originally, this app was written without a database and I chose to keep this text based 
application state feature from the original version of the app. This was 
partially due to the fact I didn't think saving the forecast to the database was useful or necessary, 
but still wanted to provide a way to view the last forecast whithout making another API call.
I could change option "Display saved weather" to pull data from the database but felt since forecast is not being
saved to the database it was best to leave this application state feature intact from the original version.

## Data saved to SQL database

Each location entered is saved to the database and is the parent table of weather data points.

By default, on start up, the app will update the weather for the default location hourly 
and automatically save the current weather for that default location to the database.

After each API call made to the current weather endpoint the current weather
for that location will be saved in the database.<br/>
(options "Update weather" and "Get weather from a saved location")

Note: If you delete a location ALL saved weather points will be deleted with it.

Forcast, statistics, and celestial data is not saved to the database.

## Statistics

The current time interval (14 minute) recurring weather statistics are.

* 8 hour average temperature, pressure, humidity, and wind speed.
* 8 hour max/min temperature, pressure, humidity, and wind speed.
* 8 hour rain/snow totals.

The extended menu displays options for 8 hour, 12 hour, and 24 hour statistics.

## Celestial Data

The Celestial data displayed include.

* Solar: sunrise, sunset, solar noon, civil dawn, civil dusk, hours of day, and hours of night
* Lunar: moonrise, moonset, moon phase, moon fraction, moon distance from Earth
* Last and next solar eclipse data
* Last and next lunar eclipse data
* Lunar perigee and apogee data
* Equinox and solistice dates

## Documentation

For an application of this size I do not feel extensive documention to be necessary.
I did take time to name variables and methods thoughtfully as well as adding in code comments 
to explain things throughout the code base.

#### Main Method flow chart

There is also a **MainMethodFlowChart.drawio** file that can be opened in [Draw Io](https://www.drawio.com/).
This chart shows the logical flow of the main method. Unfortunately, this chart is a bit out of date compared 
to the applications current state. It was made some time after the recurring weather update feature but 
before I added statistics and clestial data. I don't yet know the best way to document software. 
Flow charts are useful but are rather time consuming and hard to keep up to date with app changes.
This was my first try at a flow chart so be kind. I likely did not used the official symbols and way of doing 
things but it was a useful experience. Although it's a bit out of date it does still present a good visual on 
the logic in the main method and will go a long way in getting started in the code base.

#### Recurring weather and statistics/celestial upate

You will also find an Excel file **ThreadDisplayCalculaton.xlsx** 
This file serves as an explanation to the odd if statement you will see in the RecurringStatsAndCelestial method.
```
 if (((count * 14) / 60.00) % 1 != 0)
```
The recurring functionality is designed work like this. 
* Every hour get new weather from OpenWeather API save that weather do the data base and display it on the console.
* Every 14 minutes calculate and display statistics and celestial data.
* Every 7 minutes display saved weather from the text file. 
    * This is so that statistics and celestial data isn't displayed for the remainder 
    of the hour and the console output shuffles between statistics and weather.

The problem was I needed to figure out the logic to ensure that both or all three of these tasks didn't 
try to display to the consoel at the same time. To ensure display statistics and display saved weather 
don't overlap was easy. Only display saved weather at odd intervals. The pattern between 7 and 14 is obvious.
It was much trickier to mathematically determine when all three tasks may or may not overlap. 
To solve this I modeled the scenario in Excel and found out every 420 minutes all three tasks will operlap.
To prevent this my solution is as follows. If minute/60 is a whole number do NOT display statistics/celestial data. 
See Excel model if you are curious. A picture is worth 1,000 words.

I also chose to use a ushort count variable in RecurringStatsAndCelestial and RecurringDisplaySavedWeather 
and reset that variable at 60,000. About 6 days of continuious app run time -- calculations in Excel. 
I could have just used a uint variable but having a variable that can count for 1,167 continuous years seemed wrong to me.
That was the reasoning behind that decision. Easy enough to change at next refactor if necessary.

#### ManageAPICalls Class

The **ManageAPICalls.cs** class is commented well but is complex enough a high level overview is useful.
OpenWeather API endpoints for current weather, forecast, and air pollution require latitude and longitude to fetch the data.
Therefore, after the user enters a location the app needes to query the Geocoding endpoint to get the latitude and longitude for the
entered location before weather, forecast, or air pollution data can be fetched.

ManageAPICalls has three public methods: GetLocation, GetCurrentWeather, and GetForecast

GetLocation is designed to return a location that has latitude and longitude populated.<br/>
It does this by:
1. gets the specified location (default or at an id) from the database.
2. if that location has latitude and longitude populated 
    * save that location to the appropriate text file -- for app state feature
    * return that location
3. if that location does not have latitude and longitude populated
    * use Geocoding endpoint to get latitude and longitude
    * save that location to the appropriate text file
    * update the location in the database with latitude and longitude
    * return that location

GetCurrentWeather gets the weather from the weather endpoint, saves the weather to the database, and returns the weather.

GetForecast gets and returns the forecast. Forecast is not saved to the database

There is a public enum variable named GetLocationFor that is used to toggle between saving weather or forecast text.
This was added in because I needed to get location for the celestial data without saving anything to the text for application state

See in code comments for other documentation

#### How application code is organized

The Data folder contains the text files for the saved weather and forecast data, the xml for the API key, 
the SQLite Weather.db file, and a sql script file showing how the tables are structured.
I have been using [DB Browser for SQLite](https://sqlitebrowser.org/) to view, query, edit, and create the table 
for Weather.db.

The Managers folder contains static classes with methods needed for app functionality. 
This was a good way to factor out and organize the methods needed for app functionality into classes. 
Their names are self descriptive. However the **ManageSavedWeatherText.cs** class manages the text files for both
weather and forecast.

The Models folder contains classes that represent app data used to map JSON and SQL data to C# objects for use in the app.
* CurrentWeather.cs is to map the current weather JSON to C#
* ForecastWeather.cs is to map the forcast JSON to C#
* Location.cs is to map the location JSON to C#
* Savedlocations.cs is to map SQL locations table data to C#

The Utilities folder contains one class to convert units; meters to miles etc. within the app.

## Plans

Add an "On this day last year" feature that will display the average temperature and weather conditions for this day last year.
Could also add "On this day last month"

Improve the forecast output by adding a second panel to the right of the main output panel.
This second panel will highlight the day, time, and probability of precipitation thus making it 
easier to get a quick view of the forecast.

Possibly add a recurring forecast output.

Add alerts -- just red text on weather and or statistics ouput
* pressure dropping
* solar/lunar eclipse today
* Equinox/solistice today
* Bad weather/rain alert 
    * may end up saving some forecast data to database to do this. Then remove text base app state feature. 

Add the air pollution end point https://openweathermap.org/api/air-pollution 

Make an actual user interface maybe with [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui), 
[.NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui?view=net-maui-8.0) or both.

Once user mobile MAUI interface is constructed include weather maps end point https://openweathermap.org/api/weathermaps 
