drop table if exists locations;
drop table if exists weather;
drop table if exists settings;
/*
 0 false 1 true
 SQLite does not have a separate Boolean storage class. Instead, Boolean values are stored as integers 0 (false) and 1 (true).

 set this table to have a max of 1 row -- as its only a settings table there should only be 1 setting row. 
 https://stackoverflow.com/questions/33104101/ensure-sqlite-table-only-has-one-row
*/
CREATE TABLE settings(
	settings_id INTEGER PRIMARY KEY CHECK (settings_id = 0),
	display_saved_weather INTEGER NOT NULL,
	suppress_header INTEGER NOT NULL,
	recurring_update INTEGER NOT NULL,
	extended_menu INTEGER NOT NULL
);

INSERT INTO settings (settings_id, display_saved_weather, suppress_header, recurring_update, extended_menu) VALUES(0,0,1,1,0)

CREATE TABLE locations(
	location_id INTEGER PRIMARY KEY,
	city_name TEXT NOT NULL,
	latitude REAL,
	longitude REAL,
	country TEXT NOT NULL,
	state TEXT NOT NULL,
	is_default INTEGER NOT NULL
);

CREATE TABLE weather(
	weather_id INTEGER PRIMARY KEY,
	location_id INTEGER NOT NULL,
	latitude REAL NOT NULL,
	longitude REAL NOT NULL,
	city_name TEXT NOT NULL,
	weather_description TEXT NOT NULL,
	temperature_fahrenheit REAL NOT NULL,
	pressure_sea_level_hPa INTEGER NOT NULL,
	humidity INTEGER NOT NULL,
	visibility_miles REAL NOT NULL,
	wind_speed_miles_hr REAL NOT NULL,
	wind_direction_degrees REAL NOT NULL,
	wind_direction_cardinal TEXT NOT NULL,
	wind_gust_miles_hr REAL NOT NULL,
	cloudiness_percent INTEGER NOT NULL,
	rain_volume_last_1hr_inch REAL,
	rain_volume_last_3hr_inch REAL,
	snow_volume_last_1hr_inch REAL,
	snow_volume_last_3hr_inch REAL,
	time_weather_data_calculated_unix_utc INTEGER NOT NULL,
	date_record_saved_utc TEXT DEFAULT CURRENT_TIMESTAMP,
	FOREIGN KEY (location_id)
	REFERENCES locations(location_id)
		ON UPDATE CASCADE
		ON DELETE CASCADE	
);

SELECT * FROM weather WHERE date_record_saved_utc BETWEEN '2024-04-02 15:59:59' AND '2024-04-02 23:59:59' AND location_id == 2;

SELECT * FROM weather WHERE date_record_saved_utc BETWEEN '2024-03-16 13:00:00' AND '2024-03-16 21:00:00' AND location_id == 2;

SELECT COUNT(*) FROM weather WHERE date_record_saved_utc BETWEEN '2024-03-16 13:00:00' AND '2024-03-16 21:00:00' AND location_id == 2;

SELECT * FROM weather WHERE time_weather_data_calculated_unix_utc BETWEEN '1712073489' AND '1712102289' AND location_id == 2;

SELECT avg(wind_speed_miles_hr), avg(temperature_fahrenheit) FROM weather WHERE time_weather_data_calculated_unix_utc BETWEEN '1712073489' AND '1712102289' AND location_id == 2;

SELECT avg(temperature_fahrenheit), avg(pressure_sea_level_hPa), avg(humidity), avg(wind_speed_miles_hr) FROM weather WHERE time_weather_data_calculated_unix_utc BETWEEN '1712073489' AND '1712102289' AND location_id == 2;

SELECT max(temperature_fahrenheit), max(pressure_sea_level_hPa), max(humidity), max(wind_speed_miles_hr), min(temperature_fahrenheit), min(pressure_sea_level_hPa), min(humidity), min(wind_speed_miles_hr) FROM weather WHERE time_weather_data_calculated_unix_utc BETWEEN '1712073489' AND '1712102289' AND location_id == 2;

SELECT min(temperature_fahrenheit), min(pressure_sea_level_hPa), min(humidity), min(wind_speed_miles_hr) FROM weather WHERE time_weather_data_calculated_unix_utc BETWEEN '1712073489' AND '1712102289' AND location_id == 2;

SELECT sum(rain_volume_last_1hr_inch), sum(snow_volume_last_1hr_inch) FROM weather WHERE time_weather_data_calculated_unix_utc BETWEEN '1712073489' AND '1712102289' AND location_id == 2;
/*
SELECT avg(temperature_fahrenheit) FROM weather WHERE date_record_saved_utc BETWEEN '2024-04-02 15:59:59' AND '2024-04-02 23:59:59';

SELECT max(temperature_fahrenheit) FROM weather WHERE date_record_saved_utc BETWEEN '2024-04-02 15:59:59' AND '2024-04-02 23:59:59';

SELECT min(temperature_fahrenheit) FROM weather WHERE date_record_saved_utc BETWEEN '2024-04-02 15:59:59' AND '2024-04-02 23:59:59';*/