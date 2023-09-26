using System.Text.Json.Serialization;

//https://openweathermap.org/api/geocoding-api
namespace OpenWeatherMap
{
    internal record class Location(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("lat")] double Latitude,
        [property: JsonPropertyName("lon")] double Longitude,
        [property: JsonPropertyName("country")] string Country,
        [property: JsonPropertyName("state")] string State);
}
