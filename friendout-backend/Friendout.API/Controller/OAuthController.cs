using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

/// <summary>
/// Handles OAuth2 authentication.
/// </summary>
[ApiController]
[Route("auth")]
public class OAuthController : ControllerBase
{
    /// <summary>
    /// Initiates the OAuth2 login flow with Discord.
    /// </summary>
    /// <remarks>
    /// This endpoint triggers the Discord authentication process. The user will be redirected
    /// to Discord's login page, and after successful authentication, they will be redirected
    /// to the callback endpoint "/api/oauth/callback/discord".
    /// </remarks>
    /// <returns>
    /// An <see cref="IActionResult"/> that challenges the user with Discord's OAuth2 authentication scheme.
    /// </returns>
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
}