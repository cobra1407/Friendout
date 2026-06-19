namespace Friendout.Domain.Enums;

/// <summary>
/// Defines the types of notifications that can be triggered in the system.
/// This is a domain concept — it belongs in the Domain layer, not Infrastructure.
/// </summary>
public enum NotificationType
{
    /// <summary>Triggered when an activity is modified.</summary>
    ActivityModified,

    /// <summary>Triggered when an activity is canceled.</summary>
    ActivityCanceled,

    /// <summary>Triggered as a reminder for an upcoming activity.</summary>
    ActivityReminder,

    /// <summary>Triggered when a user is invited to an activity.</summary>
    InvitationReceived,

    /// <summary>Triggered when a user account is deleted.</summary>
    AccountDeleted,

    /// <summary>Triggered when an access request is approved by an admin.</summary>
    AccessRequestApproved,

    /// <summary>Triggered when an access request is denied by an admin.</summary>
    AccessRequestDenied,

    /// <summary>General-purpose fallback notification.</summary>
    General
}
