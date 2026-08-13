namespace Friendout.Domain.DTOs.Preferences;

/// <summary>
/// Aggregated view of a user's preferences (general + notification channels),
/// returned by GET /preferences/me for the settings screen.
/// </summary>
public record UserPreferencesDto(
    string Locale,
    bool EmailEnabled,
    bool InAppEnabled,
    string NotificationSound,
    bool AccessRequestAlertsEnabled
);

/// <summary>Payload to update a user's own preferences.</summary>
public record UpdateUserPreferencesDto(
    string Locale,
    bool EmailEnabled,
    bool InAppEnabled,
    string NotificationSound,
    bool AccessRequestAlertsEnabled
);
