# Open Weather Map Console Application with SQLite

## Powered by [OpenWeatherMap.org](https://openweathermap.org/)
## Uses [Spectre Console](https://github.com/spectreconsole/spectre.console) for the display
## Uses [Coordinat Sharp](https://coordinatesharp.com/) for the celestial data calculations

## App screen shot

![App screen shot](AppScreenShot.png "App screen shot")

## Directions for use

1. Clone app
2. Rename APIKEY-Copy.xml to AIPKEY.xml
3. Get your api key from [OpenWeatherMap.org](https://openweathermap.org/)
3. Build or publish the app in Visual Studio
4. Run the exe and enjoy -- it will ask for your api key and default location
5. Or for Ubuntu run this command in terminal --> dotnet publish -c release -r ubuntu.22.04-x64 --self-contained
6. Running on Windows 11 -- go to System -> For Developers -> Terminal and set to Windows Console Host (this allows the app to resize the console to the correct width and height)

## Build/Publish Profiles -- Raspberry Pi, Windows, and Linux-x64
In Visual Studio Publish there is a 32 and 64 bit linux arm build that is set up for the Raspberry Pi.
The Pi 3 is a 64 bit machine but the standard OS is 32 bit so in that case use the 32 bit build option.
Running this on a single board PC is the best most efficient bet for building up a weather database.
There is also a standard linux x64 build that can be used for Ubuntu and of course Windows as well.
C# is awesome!

## About
This is a simple app that displays current weather and 5 day 3hr forecast from Open Weather API.
By default the app also updates the weather for the default location hourly -- this feature can be stopped with an option in the extended menu.
Basic weather statistics and celestial data will also be displayed incrementally.
Both the hourly weather update and the statistics/celestial data features make use of C#'s multithreading capabilities to run tasks on recurring inervals.

The three Open Weather API endpoints in use are 

1. Current weather endpoint https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={APIkey}
2. 5 day forecast endpoint https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&units=imperial&appid={APIkey}
3. Location endpoint http://api.openweathermap.org/geo/1.0/direct?q={cityName},{stateCode},{countryCode}&limit={limit}&appid={APIkey}

The app will save your locations in a SQLite database and your api key in APIKEY.xml
By default, on start up, the app will update the weather for the default location hourly and automatically save the current weather for that default location to the database.
Weather statistics and celestial data for the default location will also be displayed every 14 minutes  -- this data is not saved to database. 
This statistics feature needs at least two weather data points in the database and will just diplay "Not enough weather data points to display an average" while the data is being collected.
The default location is always the first one entered when the app starts and is changeable.
Multiple locations can be added. 

Locations outside the US can be selected just follow the directions in the app.
Note: You may have to experiment a bit to get the correct location. Sometimes city names are not found.
For example, for some reason, the api wouldn't find Parker CO so I used Centennial CO -- a town or two north. 

Use [ISO 3166 country codes](https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes) and if in the US don't put periods or spaces between 
state abbreviations. For example Use NJ not N.J. Or for country use US not U.S. 

Overall the app is easy to use and a fast way to check the weather and forecast by you. Hope you enjoy it. I am hoping to have the time to make a similar mobile app.

## Motivation for building this app

* To continue learing software development and engineering. Some core concepts demonstrated are:
	* Multithreading with C# -- an intersting problem was displaying and saving the updated weather hourly and then also showing statistics/celestial data every 14 minutes (see ThreadDisplayCalculations.xlsx for solution)
	* Asynchronous programming and consuming data from an API
	* Basic SQL operations with C#
	* Writing clean documented code
* In addition to the educatational benifits I also just enjoy building pratical and useful applications
* A fast and efficient way to get weather and forecast data without all the FULL PAGE ADVERTISEMENTS one encounters on many of the leading websites.
* Build a weather database for my local area to view historical weather
* Make use of a Rasbperry Pi PC I had collecting dust

## Data saved in text files
After each api call made to the current weather endpoint (options "Update weather" and "Get weather from a saved location") 
the location and current weather will be saved in SavedCurrentLocation.txt and SavedCurrentWeather.txt -- data is overwritten each time

This is to save the application state so api calls can be limited. The option "Display saved weather" pulls the data from the text files not SQL

After each api call made to the forecast endpoint (options "Get 5 day forecast" and "Get 5 day forecast from a saved location") 
the location and 5 day forcast will be saved in SavedForecastLocation.txt and SavedForecast.txt -- data is overwritten each time

This is to save the application state so api calls can be limited. The option "Display saved forecast" pulls the data from the text files not SQL

## Data saved to SQL
Each location entered is saved to the database and is the parent table of weather data points

By default, on start up, the app will update the weather for the default location hourly and automatically save the current weather for that default location to the database.

After each api call made to the current weather endpoint (options "Update weather" and "Get weather from a saved location") the current weather for that location will be saved in the database.

Note: If you delete a location ALL saved SQL weather points will be deleted with it.

No forcast data is saved to SQL

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
I did take time to name variables and methods thoughtfully as well as adding in code comments to explain things throughout the code base.

There is also a MainMethodFlowChart.drawio file that can be opened in [Draw Io](https://www.drawio.com/) which shows the logical flow of the main method. 
Unfortunately, this chart is a bit out of date compared to the applications current state. It was made some time after the recurring weather update feature but before I added statistics and clestial data.
I don't yet know the best way to document software and I suspect many in the industy don't as well. Flow charts are useful but are rather time consuming and hard to keep up to date with app changes.
This was my first try at a flow chart so be kind. I may have not used the official symbols and way of doing things but it was a useful experience.
Although it's a bit out of data it does still present a good visual on the logic in the main method and will go a long way in getting you started in the code base.

You will also find and Excel file ThreadDisplayCalculaton.xlsx This file serves as an explination to the odd if statement you will see in the RecurringStatsAndCelestial method.
```
 if (((count * 14) / 60.00) % 1 != 0)
```
The recurring functionality is set to work like this. 
* Every hour get new weather from Open Weather api save that weather do the data base and display it on the console
* Every 14 minutes calculate and display statistics and celestial data
* Every 7 minutes display saved weather from the text file. This is so that statistics and celestial data is displayed for the remainder of the hour.

The problem was I needed to figure out the logic to ensure that both or all three of these tasks didn't try to display to the consoel at the same time.
To ensure display statistics and display saved weather don't overlap was easy. Only display saved weather at odd intervals. The pattern between 7 and 14 is obvious.
It was much trickier to mathematically determine when all three tasks may or may not overlap. For this I just modeled the scenario in Excel and turns out every 420 minutes they will.
To prevent this my solution is as follows. If minute/60 is a whole number do NOT display statistics/celestial data. See Excel model if you are curious. A picture is worth 1,000 words.

In the rest of this section I will expain the high level view of how the application is structured.

The Data folder contains the text files for the saved weather and forecast data, the xml for the API key, the SQLite Weather.db file, and a sql script file showing how the tables are structured.

The Managers folder contains static classes with methods needed for app functionality . 
This was a good way to factor out and organize the methods needed for app functionality into classes. Their names are self descriptive.

The Models folder contains classes that represent app data used to map JSON and SQL data to C# objects for use in the app.
* CurrentWeather.cs is to map the current weather JSON to C#
* ForecastWeather.cs is to map the forcast JSON to C#
* Location.cs is to map the location JSON to C#
* Savedlocations.cs is to map SQL locations table data to C#

The Utilities folder contains one class to convert units meeters to miles ect. within the app.

## Plans

Add an "On this day last year" feature that will display the average temperature and weather conditions for this day last year.
Could also add "On this day last month"

Add settings options to let the user
* Prevent the app from aking to display saved weather everytime and just automatically update on start
* Suppress or show the header
* Prevent the recurring update from starting
* Display short or long menu by default

Add the air polution end point https://openweathermap.org/api/air-pollution 

Make an actual user interface maybe with [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui), [.NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui?view=net-maui-8.0) or both.

Once user mobile MAUI interface is constructed include weather maps end point https://openweathermap.org/api/weathermaps 
