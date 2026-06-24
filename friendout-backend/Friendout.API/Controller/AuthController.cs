using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace friendout_backend.Controller;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly ITokenBlacklistService _blacklist;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtService _jwtService;
    private readonly IUserService _userService;

    /// <summary>
    /// Represents the AuthController class.
    /// The AuthController class is responsible for handling authentication-related requests.
    /// </summary>
    /// <param name="blacklist">The blacklist service used to validate and invalidate tokens.</param>
    /// <param name="refreshTokenService">The refresh token service used to manage refresh tokens.</param>
    /// <param name="jwtService">The JWT service used to generate and validate JWT tokens.</param>
    /// <param name="userService">The user service used to read up-to-date profile data from the database.</param>
    public AuthController(
        ITokenBlacklistService blacklist,
        IRefreshTokenService refreshTokenService,
        JwtService jwtService,
        IUserService userService)
    {
        _blacklist = blacklist;
        _refreshTokenService = refreshTokenService;
        _jwtService = jwtService;
        _userService = userService;
    }

    /// <summary>
    /// Returns the authenticated user's current identity.
    ///
    /// Name and avatar are read live from the database rather than from the JWT claims:
    /// the JWT is only refreshed every 15 minutes (or on login), so if it were the source
    /// of truth here, a name/avatar change made on the preferences page wouldn't be reflected
    /// until the next token rotation. Role and userId are still taken from the token, since
    /// they're stable for the token's lifetime and avoid an extra round trip for those fields.
    /// </summary>
    [HttpGet("auth/me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var role = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Role)?.Value;
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var profileResult = await _userService.GetUserProfileAsync(userId);
        if (!profileResult.IsSuccess)
            return Unauthorized();

        var profile = profileResult.Data!;

        return Ok(new
        {
            userId,
            Name = profile.Name,
            Role = role,
            AvatarUrl = profile.AvatarUrl ?? string.Empty
        });
    }

    /// <summary>
    /// Issues a new access token using a valid refresh token.
    ///
    /// Flow:
    /// 1. Client sends the refresh_token cookie (automatically by the browser).
    /// 2. We validate it against the database.
    /// 3. If valid, we rotate it (revoke old, create new) and issue a new JWT.
    /// 4. Both new cookies are set on the response.
    /// </summary>
    [HttpPost("auth/refresh")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh()
    {
        var rawToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(rawToken))
            return Unauthorized(new { error = "refresh_token_missing" });

        var refreshToken = await _refreshTokenService.ValidateAsync(rawToken);
        if (refreshToken is null)
            return Unauthorized(new { error = "refresh_token_invalid" });

        var user = refreshToken.User;

        // Build the same claims as during login.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("avatar_url", user.AvatarUrl ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var newAccessToken = _jwtService.GenerateJwt(claims);

        // Rotate the refresh token: the old one is revoked, a new one is created.
        // This means a stolen refresh token can only be used once before it becomes invalid.
        var newRawRefreshToken = await _refreshTokenService.RotateAsync(rawToken, user.Id);

        var isHttps = HttpContext.Request.IsHttps ||
            string.Equals(
                HttpContext.Request.Headers["X-Forwarded-Proto"],
                "https",
                StringComparison.OrdinalIgnoreCase);

        Response.Cookies.Append("auth_token", newAccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("refresh_token", newRawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        return Ok();
    }

    [HttpPost("auth/logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Blacklist the current access token so it cannot be used even before it expires.
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
        if (jti != null)
        {
            var expiry = expClaim != null && long.TryParse(expClaim, out var exp)
                ? DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime
                : DateTime.UtcNow.AddMinutes(15);
            _blacklist.Invalidate(jti, expiry);
        }

        // Revoke the refresh token so it cannot be used to obtain a new access token.
        // The browser sends the refresh_token cookie here because Path = "/api/auth"
        // covers both /api/auth/refresh and /api/auth/logout.
        var rawRefreshToken = Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(rawRefreshToken))
            await _refreshTokenService.RevokeAsync(rawRefreshToken);

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

        Response.Cookies.Delete("refresh_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth"
        });

        return Ok();
    }
}
