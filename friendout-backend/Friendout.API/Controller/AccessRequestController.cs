using Friendout.Domain.DTOs.Admin;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace friendout_backend.Controller;

/// <summary>
/// Public endpoint for unauthenticated users to submit an access request.
/// </summary>
[ApiController]
[EnableRateLimiting("auth")]
public class AccessRequestController : ControllerBase
{
    
    private readonly IAdminService _adminService;

    public AccessRequestController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Submits a new access request.
    /// Always returns 201 on a well-formed request, even if the email already has a pending
    /// request or is already approved — the response is intentionally identical in every case
    /// so this public endpoint cannot be used to enumerate which emails are known to the system.
    /// </summary>
    [HttpPost("access-requests")]
    [ProducesResponseType(201)]
    public async Task<IActionResult> Submit([FromBody] SubmitAccessRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { error = "email_required" });

        // Basic email format check — rejects values like "666" or "notanemail".
        if (!System.Net.Mail.MailAddress.TryCreate(dto.Email, out _))
            return BadRequest(new { error = "email_invalid" });

        var result = await _adminService.SubmitAccessRequestAsync(dto);

        if (!result.IsSuccess)
        {
            return result.ErrorMessage switch
            {
            "message_too_long" => BadRequest(new { error = "message_too_long" }),
            "too_many_pending" => StatusCode(503, new { error = "too_many_pending" }),
                _                  => StatusCode(500, new { error = result.ErrorMessage })
            };
        }

        return Created(string.Empty, null);
    }
}
