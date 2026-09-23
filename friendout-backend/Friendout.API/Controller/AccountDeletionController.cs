using Friendout.Domain.DTOs.User;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace friendout_backend.Controller;

/// <summary>
/// Public, unauthenticated endpoints for the out-of-band account deletion flow.
///
/// This exists for users who can no longer log in (e.g. their Discord guild access
/// was revoked by an admin) but still need a way to have their personal data removed.
/// Verification happens via a token sent to the account's email address, not via login.
/// </summary>
[ApiController]
[EnableRateLimiting("auth")]
public class AccountDeletionController : ControllerBase
{
    private readonly IAccountDeletionService _accountDeletionService;

    public AccountDeletionController(IAccountDeletionService accountDeletionService)
    {
        _accountDeletionService = accountDeletionService;
    }

    /// <summary>
    /// Starts a deletion request for the given email. Always returns 202, whether or not
    /// the email belongs to an account, to avoid leaking which addresses are registered.
    /// </summary>
    [HttpPost("account-deletion/request")]
    [ProducesResponseType(202)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RequestDeletion([FromBody] RequestAccountDeletionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { error = "email_required" });

        if (!System.Net.Mail.MailAddress.TryCreate(dto.Email, out _))
            return BadRequest(new { error = "email_invalid" });

        await _accountDeletionService.RequestDeletionAsync(dto.Email, dto.DeleteCreatedActivities);

        // Always 202, regardless of whether the email matched an account.
        return Accepted();
    }

    /// <summary>
    /// Confirms deletion using the token from the confirmation email.
    /// </summary>
    [HttpPost("account-deletion/confirm")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ConfirmDeletion([FromBody] ConfirmAccountDeletionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest(new { error = "token_required" });

        var result = await _accountDeletionService.ConfirmDeletionAsync(dto.Token);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok();
    }
}
