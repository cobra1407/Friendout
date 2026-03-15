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
    private readonly IWebHostEnvironment _env;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    
    /// <summary>
    /// Initializes a new instance of <see cref="OAuthCallbackController"/>.
    /// </summary>
    /// <param name="jwt">Service responsible for generating JWT tokens.</param>
    /// <param name="env">Provides information about the hosting environment.</param>
    /// <param name="userService">Service responsible for user operations.</param>
    /// <param name="configuration">Application configuration.</param>
    public OAuthCallbackController(
        JwtService jwt,
        IWebHostEnvironment env,
        IUserService userService,
        IConfiguration configuration)
    {
        _jwt = jwt;
        _env = env;
        _userService = userService;
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
        var frontendLoginUrl = _configuration["Frontend:LoginUrl"] ?? "http://localhost:5173/login";
        var frontendActivitiesUrl = _configuration["Frontend:ActivitiesUrl"] ?? "http://localhost:5173/activities";

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

        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return Redirect(frontendActivitiesUrl);
    }

}
