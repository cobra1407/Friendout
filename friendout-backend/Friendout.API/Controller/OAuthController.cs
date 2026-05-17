using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace friendout_backend.Controller;

/// <summary>
/// Handles OAuth2 authentication.
/// </summary>
[ApiController]
[Route("auth")]
[EnableRateLimiting("auth")]
public class OAuthController : ControllerBase
{
    /// <summary>
    /// Initiates the OAuth2 login flow with Discord.
    /// </summary>
    [HttpGet("discord")]
    public IActionResult LoginWithDiscord()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/oauth/callback/discord",
            IsPersistent = true
        };
        return Challenge(properties, DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Initiates the OAuth2 login flow with Google.
    /// </summary>
    [HttpGet("google")]
    public IActionResult LoginWithGoogle()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/oauth/callback/google",
            IsPersistent = true
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
}
