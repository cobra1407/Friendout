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
using friendout_backend.Controller;

LoadEnvFiles();

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────────────────────
// Configuration – Load order (the last one wins)
// 1. appsettings.json          (default values)
// 2. appsettings.{Environment}.json  (environment-specific)
// 3. Environment variables / .env     (secrets & overrides)
// ────────────────────────────────────────────────
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
            $"Configuration manquante : la clé '{key}' est obligatoire.\n" +
            "Vérifiez .env (ou .env.local), appsettings.json, ou les variables d'environnement.");
    }
}

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]!;
var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? "friendout-backend";
var jwtAudience = jwtSection.GetValue<string>("Audience") ?? "friendout-frontend";

// ────────────────────────────────────────────────
// Services
// ────────────────────────────────────────────────
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
});

var uploadsBasePath = builder.Environment.WebRootPath ?? builder.Environment.ContentRootPath;
builder.Services.AddInfrastructure(uploadsBasePath);

var connectionString = builder.Configuration.GetConnectionString("FriendoutDatabase");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "La chaîne de connexion 'ConnectionStrings:FriendoutDatabase' est manquante. Vérifiez .env ou appsettings.json.");

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

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 30 * 1024 * 1024; // 30 MB
});

// ────────────────────────────────────────────────
// Authentification
// ────────────────────────────────────────────────
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
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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

    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
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
        var accessToken = context.Properties?.GetTokenValue("access_token");
        if (string.IsNullOrEmpty(accessToken))
        {
            context.Response.Redirect("/login?error=no_token");
            context.HandleResponse();
            return;
        }

        var httpClientFactory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("https://discord.com/api/users/@me/guilds");
        if (!response.IsSuccessStatusCode)
        {
            context.HandleResponse();
            return;
        }

        var guildsJson = await response.Content.ReadAsStringAsync();
        var guilds = JsonSerializer.Deserialize<List<DiscordGuild>>(guildsJson) ?? new();

        var allowedGuildIds = builder.Configuration.GetSection("Discord:AllowedGuildIds").Get<string[]>()
            ?? GetCommaSeparatedConfig(builder.Configuration, "Discord:AllowedGuildIds")
            ?? Array.Empty<string>();

        if (allowedGuildIds.Length > 0 && !guilds.Any(g => allowedGuildIds.Contains(g.Id)))
        {
            context.Response.Redirect("/login?error=discord_access_denied");
            context.HandleResponse();
            return;
        }

        context.Success();
    };

    options.Events.OnRemoteFailure = context =>
    {
        context.HandleResponse();
        context.Response.Redirect("/login?error=discord_access_denied");
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// ────────────────────────────────────────────────
// Pipeline
// ────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Seed only if enable
    if (builder.Configuration.GetValue<bool>("Seed:Enabled", false))
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();
        await DatabaseSeeder.SeedAsync(db);
    }
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

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// ────────────────────────────────────────────────
// Helpers : .env et configuration
// ────────────────────────────────────────────────

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

// Lit une clé qui peut être une chaîne "a,b,c" (ex. variable d'env) et la retourne en tableau.
static string[]? GetCommaSeparatedConfig(IConfiguration configuration, string key)
{
    var value = configuration[key];
    if (string.IsNullOrWhiteSpace(value)) return null;
    return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}