using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Friendout.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Reverse geocoding via the free Nominatim (OpenStreetMap) public API.
///
/// Used only when a shared Google Maps link contains raw coordinates instead of a
/// named place (e.g. a dropped pin) — so we can still show a readable city/town
/// name instead of something like "50°22'51.2"N 4°51'58.7"E".
///
/// This is called at most once per activity/sub-activity, at creation or update
/// time, and the result is persisted (Localisation.DisplayName) — never called
/// again afterwards for the same location. That keeps volume well within
/// Nominatim's usage policy (https://operations.osmfoundation.org/policies/nominatim/):
/// max 1 request/second, an identifiable User-Agent, no bulk/automated geocoding.
/// </summary>
public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NominatimGeocodingService> _logger;

    public NominatimGeocodingService(HttpClient httpClient, ILogger<NominatimGeocodingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        if (_httpClient.BaseAddress is null)
            _httpClient.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");

        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Friendout/1.0 (self-hosted activity planner; contact via GitHub)");
    }

    public async Task<string?> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        try
        {
            var lat = latitude.ToString(CultureInfo.InvariantCulture);
            var lon = longitude.ToString(CultureInfo.InvariantCulture);
            var requestUri = $"reverse?format=jsonv2&lat={lat}&lon={lon}&zoom=14&accept-language=fr";

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await _httpClient.GetFromJsonAsync<NominatimReverseResponse>(requestUri, cts.Token);

            var address = response?.Address;
            if (address is null)
                return null;

            var name = address.City ?? address.Town ?? address.Village ?? address.Municipality ?? address.County;
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }
        catch (Exception ex)
        {
            // Never let a geocoding failure (timeout, rate limit, service down) block
            // activity creation/update — the caller falls back to a generic label.
            _logger.LogWarning(ex, "Reverse geocoding failed for {Lat},{Lng}", latitude, longitude);
            return null;
        }
    }

    private class NominatimReverseResponse
    {
        [JsonPropertyName("address")]
        public NominatimAddress? Address { get; set; }
    }

    private class NominatimAddress
    {
        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("town")]
        public string? Town { get; set; }

        [JsonPropertyName("village")]
        public string? Village { get; set; }

        [JsonPropertyName("municipality")]
        public string? Municipality { get; set; }

        [JsonPropertyName("county")]
        public string? County { get; set; }
    }
}
