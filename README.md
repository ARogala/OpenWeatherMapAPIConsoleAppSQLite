# Open Weather Map Console Application with SQLite
## Currently in Dev -- but works

## For the version of the app without the SQLite database go here: https://github.com/ARogala/OpenWeatherMapAPIConsoleApp
## Powered by [OpenWeatherMap.org](https://openweathermap.org/)
## Uses [Spectre Console](https://github.com/spectreconsole/spectre.console) for the display

## Directions for use

1. Clone app
2. Rename APIKEY-Copy.xml to AIPKEY.xml
3. Get your api key from [OpenWeatherMap.org](https://openweathermap.org/)
3. Build or publish the app in Visual Studio
4. Run the exe and enjoy -- it will ask for your api key and default location
5. Or for Ubuntu run this command in terminal --> dotnet publish -c release -r ubuntu.22.04-x64 --self-contained

## About
This is a simple app that displays current weather and 5 day 3hr forecast from Open Weather API
The three api endpoints in use are 

1. Current weather endpoint https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={APIkey}
2. 5 day forecast endpoint https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&units=imperial&appid={APIkey}
3. Location endpoint http://api.openweathermap.org/geo/1.0/direct?q={cityName},{stateCode},{countryCode}&limit={limit}&appid={APIkey}

The app will save your locations in a SQLite database and your api key in APIKEY.xml
You will also be able to save current weather points to the database and pull some metrics -- to be determined
The default location is always the first one entered when the app starts and is changeable.
Multiple locations can be added. 

Locations outside the US can be selected just follow the directions in the app.
Note: You may have to experiment a bit to get the correct location. Sometimes city names are not found.
For example, for some reason, the api wouldn't find Parker CO so I used Centennial CO -- a town or two north. 

Use [ISO 3166 country codes](https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes) and if in the US don't put periods or spaces between 
state abbreviations. For example Use NJ not N.J. Or for country use US not U.S. 

### Data saved in text files
After each api call made to the current weather endpoint the location and current weather will be saved
in SavedCurrentLocation.txt and SavedCurrentWeather.txt -- data over written each time
This is to save the application state so api calls can be limited. The option "Display saved weather" pulls the data from the text files not SQL
SQL options will be labeled as such.

After each api call made to the forecast endpoint the location and 5 day forcast will be saved 
in SavedForecastLocation.txt and SavedForecast.txt -- data over written each time
This is to save the application state so api calls can be limited. The option "Display saved forecast" pulls the data from the text files not SQL
SQL options will be labeled as such.

### Data saved to SQL
Each location entered is saved to the database and is the parent table of weather data points

After each api call made to the current weather endpoint the current weather will be saved in SQL -- this is for metric calculation and historical data
Note: If you delete a location ALL saved SQL weather points will be deleted with it.

No forcast data is saved to SQL

Overall the app is easy to use and a fast way to check the weather and forecast by you.

## Plans
cli options that will add weather data points automatically to the database every hour. Run on raspberry pi to build up your weather database

Make an actual user interface maybe with [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)


