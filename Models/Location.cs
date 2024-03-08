using System.Text.Json.Serialization;

//https://openweathermap.org/api/geocoding-api
namespace OpenWeatherMap.Models
{
    internal record class Location(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("lat")] float Latitude,
        [property: JsonPropertyName("lon")] float Longitude,
        [property: JsonPropertyName("country")] string Country,
        [property: JsonPropertyName("state")] string State);
}
