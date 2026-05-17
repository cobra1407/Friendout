using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace friendout_backend.Controller;

/// <summary>
/// Controller responsible for handling OAuth callbacks from external providers.
/// </summary>
[ApiController]
[Route("oauth/callback")]
[EnableRateLimiting("auth")]
public class OAuthCallbackController : ControllerBase
{
    private readonly JwtService _jwt;
    private readonly IUserService _userService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Controller responsible for handling OAuth callbacks from external providers.
    /// </summary>
    /// <param name="jwt">The JWT service used to generate access tokens.</param>
    /// <param name="userService">The user service used to retrieve user information.</param>
    /// <param name="refreshTokenService">The refresh token service used to manage refresh tokens.</param>
    /// <param name="configuration">The application configuration used to retrieve JWT settings.</param>
    public OAuthCallbackController(
        JwtService jwt,
        IUserService userService,
        IRefreshTokenService refreshTokenService,
        IConfiguration configuration)
    {
        _jwt = jwt;
        _userService = userService;
        _refreshTokenService = refreshTokenService;
        _configuration = configuration;
    }

    /// <summary>
    /// OAuth callback endpoint for Discord authentication.
    /// This endpoint is called by Discord after the user successfully authenticates.
    /// </summary>
    /// <returns>
    /// Redirects the user to the frontend login page if authentication fails,
    /// otherwise issues a JWT and redirects to the protected area.
    /// </returns>
    [HttpGet("discord")]
    public async Task<IActionResult> DiscordCallback()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var frontendLoginUrl = _configuration["Frontend:LoginUrl"] ?? $"{baseUrl}/login";
        var frontendActivitiesUrl = _configuration["Frontend:ActivitiesUrl"] ?? $"{baseUrl}/activities";

        var result = await HttpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        if (!result.Succeeded || result.Principal == null)
            return Redirect($"{frontendLoginUrl}?error=discord_unauthorized");

        var claims = result.Principal.Claims.ToArray();

        var discordId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var avatarUrl = claims.FirstOrDefault(c => c.Type == "urn:discord:avatar:url")?.Value;

        if (string.IsNullOrEmpty(discordId))
            return Redirect($"{frontendLoginUrl}?error=missing_id");

        var userResult = await _userService.CreateUserFromOAuthAsync(
            ProviderEnum.Discord, discordId, username ?? "User", email, avatarUrl);

        if (!userResult.IsSuccess)
            return Redirect($"{frontendLoginUrl}?error=user_creation_failed");

        return await IssueTokensAndRedirect(userResult.Data!, frontendActivitiesUrl);
    }

    /// <summary>
    /// OAuth callback endpoint for Google authentication.
    /// This endpoint is called by Google after the user successfully authenticates.
    /// </summary>
    /// <returns>
    /// Redirects the user to the frontend login page if authentication fails,
    /// otherwise issues a JWT and redirects to the protected area.
    /// </returns>
    [HttpGet("google")]
    public async Task<IActionResult> GoogleCallback()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var frontendLoginUrl = _configuration["Frontend:LoginUrl"] ?? $"{baseUrl}/login";
        var frontendActivitiesUrl = _configuration["Frontend:ActivitiesUrl"] ?? $"{baseUrl}/activities";

        var result = await HttpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        if (!result.Succeeded || result.Principal == null)
            return Redirect($"{frontendLoginUrl}?error=google_unauthorized");

        var claims = result.Principal.Claims.ToArray();

        var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var avatarUrl = claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value;

        if (string.IsNullOrEmpty(googleId))
            return Redirect($"{frontendLoginUrl}?error=missing_id");

        var userResult = await _userService.CreateUserFromOAuthAsync(
            ProviderEnum.Google, googleId, username ?? "User", email, avatarUrl);

        if (!userResult.IsSuccess)
            return Redirect($"{frontendLoginUrl}?error=user_creation_failed");

        return await IssueTokensAndRedirect(userResult.Data!, frontendActivitiesUrl);
    }

    /// <summary>
    /// Issues a JWT access token and a refresh token as HttpOnly cookies,
    /// then redirects the user to the given URL.
    /// Any previously active refresh tokens for this user are revoked beforehand.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="redirectUrl">The frontend URL to redirect to after login.</param>
    private async Task<IActionResult> IssueTokensAndRedirect(
        Friendout.Domain.Models.User user,
        string redirectUrl)
    {
        var cleanClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("avatar_url", user.AvatarUrl ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = _jwt.GenerateJwt(cleanClaims);

        var isHttps = HttpContext.Request.IsHttps ||
            string.Equals(
                HttpContext.Request.Headers["X-Forwarded-Proto"],
                "https",
                StringComparison.OrdinalIgnoreCase);

        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        await _refreshTokenService.RevokeAllAsync(user.Id);
        var rawRefreshToken = await _refreshTokenService.CreateAsync(user.Id);

        Response.Cookies.Append("refresh_token", rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Redirect(redirectUrl);
    }
}
