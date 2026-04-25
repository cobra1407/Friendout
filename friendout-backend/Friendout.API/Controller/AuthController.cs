using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly ITokenBlacklistService _blacklist;

    public AuthController(ITokenBlacklistService blacklist)
    {
        _blacklist = blacklist;
    }

    [HttpGet("auth/me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var role = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Role)?.Value;
        var avatarUrl = User.Claims.FirstOrDefault(c => c.Type == "avatar_url")?.Value;
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        return Ok(new
        {
            userId,
            User.Identity?.Name,
            Role = role,
            AvatarUrl = avatarUrl
        });
    }

    [HttpPost("auth/logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // Extract the Jti (unique token ID) from the current token's claims.
        // Adding it to the blacklist ensures the token is rejected on future requests,
        // even if the cookie is somehow still sent.
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
        if (jti != null)
        {
            // Parse the exp claim (Unix timestamp) to know when to auto-clean the blacklist entry.
            var expiry = expClaim != null && long.TryParse(expClaim, out var exp)
                ? DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime
                : DateTime.UtcNow.AddDays(7); // fallback: assume 7-day token
            _blacklist.Invalidate(jti, expiry);
        }
        
        var isHttps = HttpContext.Request.IsHttps ||
            string.Equals(
                HttpContext.Request.Headers["X-Forwarded-Proto"],
                "https",
                StringComparison.OrdinalIgnoreCase);

        Response.Cookies.Delete("auth_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax
        });

        return Ok();
    }
}
