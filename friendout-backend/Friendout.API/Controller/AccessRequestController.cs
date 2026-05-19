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
    /// Returns 201 on success, 409 if a pending request or approved email already exists.
    /// </summary>
    [HttpPost("access-requests")]
    [ProducesResponseType(201)]
    [ProducesResponseType(409)]
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
            "already_pending"  => Conflict(new { error = "already_pending" }),
            "already_approved" => Conflict(new { error = "already_approved" }),
            "too_many_pending" => StatusCode(503, new { error = "too_many_pending" }),
                _                  => StatusCode(500, new { error = result.ErrorMessage })
            };
        }

        return Created(string.Empty, null);
    }
}
