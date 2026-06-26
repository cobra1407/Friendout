using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Friendout.Domain.Context;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace friendout_backend.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAppAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection  = configuration.GetSection("Jwt");
        var jwtKey      = jwtSection["Key"]!;
        var jwtIssuer   = jwtSection.GetValue<string>("Issuer")   ?? "friendout-backend";
        var jwtAudience = jwtSection.GetValue<string>("Audience") ?? "friendout-frontend";

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme       = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => ConfigureJwtBearer(options, jwtKey, jwtIssuer, jwtAudience))
            .AddCookie(ConfigureCookie)
            .AddGoogle(options  => ConfigureGoogle(options,  configuration))
            .AddDiscord(options => ConfigureDiscord(options, configuration));

        return services;
    }

    // -------------------------------------------------------
    // JWT Bearer
    // -------------------------------------------------------

    private static void ConfigureJwtBearer(
        JwtBearerOptions options,
        string jwtKey,
        string jwtIssuer,
        string jwtAudience)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType            = ClaimTypes.Role,
            NameClaimType            = JwtRegisteredClaimNames.Name
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                // Check if this token has been blacklisted (i.e. the user already logged out).
                // We use the Jti claim (unique token ID) as the blacklist key.
                var blacklist = context.HttpContext.RequestServices
                    .GetRequiredService<ITokenBlacklistService>();

                var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
                if (jti != null && blacklist.IsBlacklisted(jti))
                    context.Fail("Token has been invalidated.");

                return Task.CompletedTask;
            },

            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token) &&
                    context.Request.Cookies.TryGetValue("auth_token", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    error   = "Unauthorized",
                    message = "You must be authenticated to access this resource."
                });

                return context.Response.WriteAsync(result);
            }
        };
    }

    // -------------------------------------------------------
    // Cookie (temporary OAuth session)
    // -------------------------------------------------------

    private static void ConfigureCookie(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = ".AspNetCore.OAuth.Temp";
        options.Cookie.HttpOnly = true;
        // SameAsRequest: Secure flag is derived from the actual request scheme.
        // CookieSecurePolicy.Always would block cookies over HTTP (Docker without TLS).
        // When HTTPS is enabled, Secure=true is applied automatically.
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite     = SameSiteMode.Lax;
    }

    // -------------------------------------------------------
    // Google OAuth
    // -------------------------------------------------------

    private static void ConfigureGoogle(
        Microsoft.AspNetCore.Authentication.Google.GoogleOptions options,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("Authentication:Google");

        options.ClientId     = section["ClientId"]!;
        options.ClientSecret = section["ClientSecret"]!;
        options.CallbackPath = section["CallbackPath"] ?? "/signin-google";
        options.SaveTokens   = true;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.CorrelationCookie.SameSite     = SameSiteMode.Lax;

        // Map the profile picture claim. The `picture` field from Google's userinfo
        // response is a plain string URL, so a simple JSON key mapping is enough.
        options.ClaimActions.MapJsonKey("urn:google:picture", "picture");

        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.Events.OnRemoteFailure = context =>
        {
            var loginUrl = configuration["Frontend:LoginUrl"] ?? "/login";
            context.HandleResponse();
            context.Response.Redirect($"{loginUrl}?error_code=google_access_denied");
            return Task.CompletedTask;
        };

        options.Events.OnTicketReceived = async context =>
        {
            var loginUrl = configuration["Frontend:LoginUrl"] ?? "/login";
            var email    = context.Principal?.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();

            var settingsService = context.HttpContext.RequestServices.GetRequiredService<ISettingsService>();
            var settings        = await settingsService.GetAccessSettingsAsync();

            // Restriction disabled → open mode, everyone is allowed.
            if (!settings.GoogleRestricted)
            {
                context.Success();
                return;
            }

            // Restriction enabled → email must be in the whitelist.
            var db        = context.HttpContext.RequestServices.GetRequiredService<FriendoutDbContext>();
            var appLog    = context.HttpContext.RequestServices.GetRequiredService<IAppLogService>();
            var allowedEmails = await db.AllowedEmails.Select(e => e.Email).ToListAsync();

            if (string.IsNullOrEmpty(email) || !allowedEmails.Contains(email))
            {
                await appLog.LogWarningAsync("Auth", $"Google login refused — email not in whitelist: {email}");
                var encodedEmail = Uri.EscapeDataString(email ?? "");
                context.Response.Redirect($"{loginUrl}?error_code=google_access_denied&email={encodedEmail}");
                context.HandleResponse();
                return;
            }

            context.Success();
        };
    }

    // -------------------------------------------------------
    // Discord OAuth
    // -------------------------------------------------------

    private static void ConfigureDiscord(
        AspNet.Security.OAuth.Discord.DiscordAuthenticationOptions options,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("Authentication:Discord");

        options.ClientId     = section["ClientId"]!;
        options.ClientSecret = section["ClientSecret"]!;
        options.CallbackPath = section["CallbackPath"] ?? "/signin-discord";
        options.SaveTokens   = true;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // The OAuth correlation cookie must follow the actual request scheme.
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.CorrelationCookie.SameSite     = SameSiteMode.Lax;

        // Avatar URL custom claim.
        options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
        {
            if (!user.TryGetProperty("id",     out var idProp)     ||
                !user.TryGetProperty("avatar", out var avatarProp))
                return null;

            var id     = idProp.GetString();
            var avatar = avatarProp.GetString();
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(avatar)) return null;

            var ext = avatar.StartsWith("a_") ? "gif" : "png";
            return $"https://cdn.discordapp.com/avatars/{id}/{avatar}.{ext}?size=512";
        });

        options.Scope.Add("identify");
        options.Scope.Add("email");
        options.Scope.Add("guilds");
        options.ClaimActions.MapJsonKey("urn:discord:guilds", "guilds");

        options.Events.OnTicketReceived = async context =>
        {
            // Read the login URL from config to generate absolute redirects.
            // A relative redirect (/login) would also work, but an absolute URL
            // is safer behind a reverse proxy.
            var loginUrl    = configuration["Frontend:LoginUrl"] ?? "/login";
            var accessToken = context.Properties?.GetTokenValue("access_token");

            if (string.IsNullOrEmpty(accessToken))
            {
                // error_code: query parameter read by the React frontend (loginpage.tsx)
                context.Response.Redirect($"{loginUrl}?error_code=no_token");
                context.HandleResponse();
                return;
            }

            var httpClientFactory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://discord.com/api/users/@me/guilds");
            if (!response.IsSuccessStatusCode)
            {
                context.Response.Redirect($"{loginUrl}?error_code=discord_access_denied");
                context.HandleResponse();
                return;
            }

            var guildsJson = await response.Content.ReadAsStringAsync();
            var guilds     = JsonSerializer.Deserialize<List<DiscordGuild>>(guildsJson) ?? new();

            var settingsService = context.HttpContext.RequestServices.GetRequiredService<ISettingsService>();
            var settings        = await settingsService.GetAccessSettingsAsync();

            // Restriction disabled → skip guild check entirely.
            if (!settings.DiscordRestricted)
            {
                context.Success();
                return;
            }

            var db             = context.HttpContext.RequestServices.GetRequiredService<FriendoutDbContext>();
            var allowedGuildIds = await db.AllowedGuilds.Select(g => g.GuildId).ToListAsync();

            if (allowedGuildIds.Count > 0 && !guilds.Any(g => allowedGuildIds.Contains(g.Id)))
            {
                var appLog = context.HttpContext.RequestServices.GetRequiredService<IAppLogService>();
                await appLog.LogWarningAsync("Auth", "Login refused — no matching allowed guild.");
                context.Response.Redirect($"{loginUrl}?error_code=discord_access_denied");
                context.HandleResponse();
                return;
            }

            context.Success();
        };

        options.Events.OnRemoteFailure = context =>
        {
            // Read the login URL from config to generate absolute redirects.
            var loginUrl = configuration["Frontend:LoginUrl"] ?? "/login";
            context.HandleResponse();
            context.Response.Redirect($"{loginUrl}?error_code=discord_access_denied");
            return Task.CompletedTask;
        };
    }
}
