using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

[ApiController]
public class AuthController : ControllerBase
{
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
        // Doit correspondre exactement aux attributs utilisés lors de la création du cookie
        // dans OAuthCallbackController, sinon le browser ignore la suppression.
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