using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Friendout.Domain.Context;
using Friendout.Domain.Models;
using Friendout.Domain.Seeds;
using Friendout.Infrastructure;
using Friendout.Infrastructure.Interfaces;
using friendout_backend.Controller;

LoadEnvFiles();

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Configuration – Load order (the last one wins)
// 1. appsettings.json          (default values)
// 2. appsettings.{Environment}.json  (environment-specific)
// 3. Environment variables / .env     (secrets & overrides)
// -----------------------------
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(prefix: null);

// Required configuration keys validation (appsettings / .env)
var requiredKeys = new[]
{
    "Jwt:Key",
    "Authentication:Discord:ClientId",
    "Authentication:Discord:ClientSecret"
};

foreach (var key in requiredKeys)
{
    if (string.IsNullOrWhiteSpace(builder.Configuration[key]))
    {
        throw new InvalidOperationException(
            $"Configuration missing: the key� '{key}' est obligatoire.\n" +
            "Vérifiez .env (ou .env.local), appsettings.json, ou les variables d'environnement.");
    }
}

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]!;
var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? "friendout-backend";
var jwtAudience = jwtSection.GetValue<string>("Audience") ?? "friendout-frontend";

// -----------------------------
// Services
// -----------------------------
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Friendout API", Version = "v1" });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        opt.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RoutePrefixConvention("api"));
})
.AddJsonOptions(options =>
{
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

var uploadsBasePath = builder.Environment.WebRootPath ?? builder.Environment.ContentRootPath;
builder.Services.AddInfrastructure(uploadsBasePath);

var connectionString = builder.Configuration.GetConnectionString("FriendoutDatabase");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "La chaîne de connexion 'ConnectionStrings:FriendoutDatabase' is missing. Vérifiez .env ou appsettings.json.");

builder.Services.AddDbContext<FriendoutDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? GetCommaSeparatedConfig(builder.Configuration, "Cors:AllowedOrigins")
            ?? new[] { "http://localhost:5173", "http://localhost:5122" };

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddHttpClient();

// Singleton: one instance shared across all requests.
// The blacklist must persist between requests — a scoped or transient service would lose state.
builder.Services.AddSingleton<Friendout.Infrastructure.Interfaces.ITokenBlacklistService,
    Friendout.Infrastructure.Services.TokenBlacklistService>();

builder.Services.AddScoped<Friendout.Infrastructure.Interfaces.IRefreshTokenService,
    Friendout.Infrastructure.Services.RefreshTokenService>();

// Runs once per day to delete expired and revoked refresh tokens.
builder.Services.AddHostedService<friendout_backend.RefreshTokenCleanupService>();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 30 * 1024 * 1024; // 30 MB
});

// ------------------------------------------------
// Authentification
// ------------------------------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = JwtRegisteredClaimNames.Name
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Check if this token has been blacklisted (i.e. the user already logged out).
            // We use the Jti claim (unique token ID) as the blacklist key.
            var blacklist = context.HttpContext.RequestServices
                .GetRequiredService<Friendout.Infrastructure.Interfaces.ITokenBlacklistService>();

            var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (jti != null && blacklist.IsBlacklisted(jti))
            {
                context.Fail("Token has been invalidated.");
            }

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
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new
            {
                error = "Unauthorized",
                message = "You must be authenticated to access this resource."
            });

            return context.Response.WriteAsync(result);
        }
    };
})
.AddCookie(options =>
{
    options.Cookie.Name = ".AspNetCore.OAuth.Temp";
    options.Cookie.HttpOnly = true;
    // SameAsRequest: Secure flag is derived from the actual request scheme.
    // CookieSecurePolicy.Always would block cookies over HTTP (Docker without TLS).
    // When HTTPS is enabled, Secure=true is applied automatically.
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
})
.AddDiscord(options =>
{
    var discordSection = builder.Configuration.GetSection("Authentication:Discord");

    options.ClientId = discordSection["ClientId"]!;
    options.ClientSecret = discordSection["ClientSecret"]!;
    options.CallbackPath = discordSection["CallbackPath"] ?? "/signin-discord";
    options.SaveTokens = true;
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    // Same reason: the OAuth correlation cookie must follow the actual request scheme.
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;

    // Avatar URL custom claim
    options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
    {
        if (!user.TryGetProperty("id", out var idProp) || !user.TryGetProperty("avatar", out var avatarProp))
            return null;

        var id = idProp.GetString();
        var avatar = avatarProp.GetString();
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(avatar))
            return null;

        var ext = avatar.StartsWith("a_") ? "gif" : "png";
        return $"https://cdn.discordapp.com/avatars/{id}/{avatar}.{ext}?size=512";
    });

    options.Scope.Add("identify");
    options.Scope.Add("email");
    options.Scope.Add("guilds");

    options.ClaimActions.MapJsonKey("urn:discord:guilds", "guilds");

    options.Events.OnTicketReceived = async context =>
    {
        // Lire l'URL de login depuis la config pour générer des redirects absolus.
        // Un redirect relatif (/login) fonctionnerait aussi, mais une URL absolue
        // est plus sûre derrière un reverse proxy.
        var loginUrl = builder.Configuration["Frontend:LoginUrl"] ?? "/login";

        var accessToken = context.Properties?.GetTokenValue("access_token");
        if (string.IsNullOrEmpty(accessToken))
        {
            // error_code : paramètre lu par le frontend React (loginpage.tsx)
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
        var guilds = JsonSerializer.Deserialize<List<DiscordGuild>>(guildsJson) ?? new();

        // Check if the user has access to at least one of the allowed guilds (if any are configured).
        var db = context.HttpContext.RequestServices.GetRequiredService<FriendoutDbContext>();
        var allowedGuildIds = await db.AllowedGuilds
            .Select(g => g.GuildId)
            .ToListAsync();

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
        var loginUrl = builder.Configuration["Frontend:LoginUrl"] ?? "/login";
        context.HandleResponse();
        context.Response.Redirect($"{loginUrl}?error_code=discord_access_denied");
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// -- Migrations --
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();
    await db.Database.MigrateAsync();
}

// -----------------------------
// Pipeline
//-----------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed runs in any environment when explicitly enabled — useful for first-time Docker setup.
if (builder.Configuration.GetValue<bool>("Seed:Enabled", false))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();
    await DatabaseSeeder.SeedAsync(db);
}

if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

var uploadsPath = Path.Combine(uploadsBasePath, "uploads");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// Forwarded headers (X-Forwarded-For, X-Forwarded-Proto) for correct client IP and scheme when behind a reverse proxy.
app.UseForwardedHeaders();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// -----------------------------
// Helpers : .env and configuration
// -----------------------------

static void LoadEnvFiles()
{
    var dirs = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
    foreach (var dir in dirs)
    {
        var envPath = Path.Combine(dir, ".env");
        if (File.Exists(envPath))
        {
            DotNetEnv.Env.Load(envPath);
            break;
        }
    }
    foreach (var dir in dirs)
    {
        var localPath = Path.Combine(dir, ".env.local");
        if (File.Exists(localPath))
        {
            DotNetEnv.Env.Load(localPath);
            break;
        }
    }
}

// Helper to parse comma-separated config values (e.g. CORS origins) from either JSON array or comma-separated string.
static string[]? GetCommaSeparatedConfig(IConfiguration configuration, string key)
{
    var value = configuration[key];
    if (string.IsNullOrWhiteSpace(value)) return null;
    return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
