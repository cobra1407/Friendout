using System.Threading;
using System.Threading.Tasks;

namespace Friendout.Infrastructure.Interfaces;

public interface IGeocodingService
{
    /// <summary>
    /// Attempts to resolve a human-readable place name (city/town) from coordinates.
    /// Returns null on failure — callers must fall back to a generic label, never
    /// let a geocoding failure block the calling operation.
    /// </summary>
    Task<string?> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
}
