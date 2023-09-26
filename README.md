# Open Weather Map Console Application

## Powered by [OpenWeatherMap.org](https://openweathermap.org/)
## Uses [Spectre Console](https://github.com/spectreconsole/spectre.console) for the display

## Directions for use

1. Clone app
2. Rename APIKEY-Copy.xml to AIPKEY.xml
3. Get your api key from [OpenWeatherMap.org](https://openweathermap.org/)
3. Build or publish the app in Visual Studio
4. Run the exe and enjoy -- it will ask for your api key and default location

## About
This is a simple app that displays current weather and 5 day 3hr forecast from Open Weather API
The three api endpoints in use are 

1. Current weather endpoint https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={APIkey}
2. 5 day forecast endpoint https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&units=imperial&appid={APIkey}
3. Location endpoint http://api.openweathermap.org/geo/1.0/direct?q={cityName},{stateCode},{countryCode}&limit={limit}&appid={APIkey}

The app will save your locations in SavedLocation.xml and your api key in APIKEY.xml
The default location is always the first in the list and is changeable.
Multiple locations can be added. 

Locations outside the US can be selected just follow the directions in the app.
Note: You may have to experiment a bit to get the correct location. Sometimes city names are not found.
For example, for some reason, the api wouldn't find Parker CO so I used Centennial CO -- a town or two north. 

Use [ISO 3166 country codes](https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes) and if in the US don't put periods or spaces between 
state abbreviations. For example Use NJ not N.J. Or for country use US not U.S. 

After each api call made to the current weather endpoint the location and current weather will be saved
in SavedCurrentLocation.txt and SavedCurrentWeather.txt -- data over written each time

After each api call made to the forecast endpoint the location and 5 day forcast will be saved 
in SavedForecastLocation.txt and SavedForecast.txt -- data over written each time

This is to help you limit the number of api calls made. You can check the most recent saved weather data or forecast data.

Overall the app is easy to use and a fast way to check the weather and forecast by you.

## Plans
Add SQL database to recored current weather. This way weekly average temperature and other metrics can be calculted.

Make an actual user interface maybe with [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)


