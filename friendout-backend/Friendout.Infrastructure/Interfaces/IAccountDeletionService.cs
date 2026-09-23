using System.Threading.Tasks;
using Friendout.Domain.DTOs.User;
using Friendout.Infrastructure.Services;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Handles the out-of-band (no-login-required) account deletion flow.
///
/// This exists for users who can no longer authenticate — e.g. an admin revoked the
/// Discord guild that used to grant them access — but who still need a way to exercise
/// their right to erasure. The account row itself is always hard-deleted; activities the
/// user created are either orphaned (CreatedBy set to null, so other participants keep
/// their content) or deleted entirely, depending on the user's choice and whether anyone
/// else is involved. See IAccountDeletionService.DeleteUserDataAsync for the exact rules.
/// </summary>
public interface IAccountDeletionService
{
    /// <summary>
    /// Starts a deletion request for the given email, if it belongs to an existing account.
    /// Always returns success regardless of whether the email exists, to avoid leaking
    /// which addresses have an account (same reasoning as access requests). The
    /// deleteCreatedActivities choice is stored on the request and applied at confirmation.
    /// </summary>
    Task<ServiceResult<bool>> RequestDeletionAsync(string email, bool deleteCreatedActivities);

    /// <summary>
    /// Confirms a deletion request using the token from the confirmation email, applying
    /// the deleteCreatedActivities choice captured when the request was made.
    /// </summary>
    Task<ServiceResult<bool>> ConfirmDeletionAsync(string token);

    /// <summary>
    /// Shared cleanup logic used by both the self-service deletion flow above and the
    /// admin "delete user" action.
    ///
    /// Always removes the user row entirely (no anonymized "ghost" accounts). Before that:
    /// - Purely-personal records (OAuth links, tokens, preferences, notifications,
    ///   equipment lists) are deleted outright.
    /// - The user's own participations in other people's activities are deleted outright
    ///   (only affects their own row, no one else's data).
    /// - Comments authored by the user are deleted outright (authorship can't be
    ///   reassigned without misattributing what someone else wrote).
    /// - Activities the user created are handled per deleteCreatedActivities:
    ///   - If true: deleted entirely, regardless of other participants (explicit,
    ///     consented choice — the person was warned this affects others too).
    ///   - If false (default): orphaned (CreatedBy set to null) when other participants
    ///     are involved, so the activity stays intact for them; deleted outright when
    ///     the user was the only one involved, since nothing is left to preserve.
    /// </summary>
    Task<ServiceResult<bool>> DeleteUserDataAsync(string userId, bool deleteCreatedActivities);
}
