namespace Friendout.Domain.DTOs.Preferences;

/// <summary>
/// Aggregated view of a user's preferences (general + notification channels),
/// returned by GET /preferences/me for the settings screen.
/// </summary>
public record UserPreferencesDto(
    string Locale,
    bool EmailEnabled,
    bool InAppEnabled
);

/// <summary>Payload to update a user's own preferences.</summary>
public record UpdateUserPreferencesDto(
    string Locale,
    bool EmailEnabled,
    bool InAppEnabled
);
