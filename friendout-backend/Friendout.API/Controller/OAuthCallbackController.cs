using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

/// <summary>
/// Controller responsible for handling OAuth callbacks from external providers.
/// </summary>
[ApiController]
[Route("oauth/callback")]
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
        {
            return Redirect($"{frontendLoginUrl}?error=discord_unauthorized");
        }

        var claims = result.Principal.Claims.ToArray();

        var discordId = claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        var username = claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        var email = claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        var avatarUrl = claims
            .FirstOrDefault(c => c.Type == "urn:discord:avatar:url")
            ?.Value;


        if (string.IsNullOrEmpty(discordId))
        {
            return Redirect($"{frontendLoginUrl}?error=missing_id");
        }

        var userResult = await _userService.CreateUserFromOAuthAsync(
            ProviderEnum.Discord,
            discordId,
            username ?? "User",
            email,
            avatarUrl
        );

        if (!userResult.IsSuccess)
        {
            return Redirect($"{frontendLoginUrl}?error=user_creation_failed");
        }

        var user = userResult.Data!;

        var cleanClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),                // JWT standard
            new(ClaimTypes.NameIdentifier, user.Id),                  // ASP.NET standard
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("avatar_url", user.AvatarUrl ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = _jwt.GenerateJwt(cleanClaims);

        // Secure=true only if the request arrives over HTTPS.
        // CookieSecurePolicy.Always (or Secure=true) blocks cookies over HTTP on non-localhost.
        // Chrome has a localhost exception but rejects Secure cookies on plain HTTP for remote IPs.
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

        var rawRefreshToken = await _refreshTokenService.CreateAsync(user.Id);
        Response.Cookies.Append("refresh_token", rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return Redirect(frontendActivitiesUrl);
    }

}
