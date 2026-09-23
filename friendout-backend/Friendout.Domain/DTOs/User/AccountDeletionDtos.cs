namespace Friendout.Domain.DTOs.User;

/// <summary>Request body to start the out-of-band account deletion flow.</summary>
/// <param name="DeleteCreatedActivities">
/// If true, every activity the user created is deleted outright — even ones other people
/// are participating in. If false (default), those activities are orphaned instead
/// (kept intact for other participants, with no owner) rather than deleted.
/// </param>
public record RequestAccountDeletionDto(string Email, bool DeleteCreatedActivities = false);

/// <summary>Request body to confirm account deletion using the token from the confirmation email.</summary>
public record ConfirmAccountDeletionDto(string Token);
