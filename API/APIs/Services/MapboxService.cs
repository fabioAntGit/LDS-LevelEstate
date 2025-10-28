using API.APIs.Exceptions;
using API.APIs.Interfaces;
using System.Text.Json;

namespace API.APIs.Services
{
    public class MapboxService : IMapboxService
    {
        private readonly HttpClient httpClient;
        private readonly string accessToken;

        public MapboxService(HttpClient httpClient, IConfiguration config)
        {
            this.httpClient = httpClient;
            this.accessToken = config["Mapbox:AccessToken"];
        }

        public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string address)
        {
            var url = $"https://api.mapbox.com/search/geocode/v6/forward?q={Uri.EscapeDataString(address)}&access_token={accessToken}";

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) throw new MapboxUnavailableException("Mapbox API request failed");

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("features", out var features) || features.GetArrayLength() == 0)
                throw new MapboxNotFoundException($"Could not find coordinates for address '{address}'.");

            foreach (var feature in features.EnumerateArray())
            {
                if (feature.TryGetProperty("properties", out var properties) &&
                    properties.TryGetProperty("context", out var context) &&
                    context.TryGetProperty("country", out var country) &&
                    country.GetProperty("name").GetString() == "Portugal" &&
                    country.GetProperty("country_code").GetString() == "PT")
                {
                    var coords = feature.GetProperty("geometry").GetProperty("coordinates");
                    double lon = coords[0].GetDouble();
                    double lat = coords[1].GetDouble();
                    return (lat, lon);
                }
            }

            throw new MapboxNotFoundException($"Address '{address}' was not found in Portugal.");
        }
    }
}
