using friendout_backend;
using Friendout.Domain.Context;
using Friendout.Infrastructure;
using friendout_backend.Controller;
using friendout_backend.Extensions;
using friendout_backend.Helpers;
using Microsoft.EntityFrameworkCore;

// -------------------------------------------------------
// Environment variables (.env / .env.local)
// -------------------------------------------------------
EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// Configuration
// Load order (last wins): appsettings.json → appsettings.{env}.json → env variables
// -------------------------------------------------------
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(prefix: null);

// Fail fast: ensure all required keys are present before starting.
var requiredKeys = new[]
{
    "Jwt:Key",
    "Authentication:Discord:ClientId",
    "Authentication:Discord:ClientSecret",
    "Authentication:Google:ClientId",
    "Authentication:Google:ClientSecret"
};

foreach (var key in requiredKeys)
{
    if (string.IsNullOrWhiteSpace(builder.Configuration[key]))
        throw new InvalidOperationException(
            $"Configuration missing: The key '{key}' is required. " +
            "Please check your .env (or .env.local) file, appsettings.json, or environment variables.");
}

// -------------------------------------------------------
// Services
// -------------------------------------------------------
var uploadsBasePath = builder.Environment.WebRootPath ?? builder.Environment.ContentRootPath;

var connectionString = builder.Configuration.GetConnectionString("FriendoutDatabase")
    ?? throw new InvalidOperationException(
        "The connection string 'ConnectionStrings:FriendoutDatabase' is missing. " +
        "Please check your .env file or appsettings.json.");

builder.Services.AddAppForwardedHeaders();
builder.Services.AddAppSwagger();
builder.Services.AddControllers(options => options.Conventions.Add(new RoutePrefixConvention("api")))
                .AddJsonOptions(opt => opt.JsonSerializerOptions.Converters
                    .Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddInfrastructure(uploadsBasePath);
builder.Services.AddDbContext<FriendoutDbContext>(opt =>
    opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddAppAuthentication(builder.Configuration);
builder.Services.AddHttpClient();

builder.Services.AddSingleton<Friendout.Infrastructure.Interfaces.ITokenBlacklistService,
                              Friendout.Infrastructure.Services.TokenBlacklistService>();
builder.Services.AddScoped<Friendout.Infrastructure.Interfaces.IRefreshTokenService,
                           Friendout.Infrastructure.Services.RefreshTokenService>();
builder.Services.AddHostedService<RefreshTokenCleanupService>();

builder.WebHost.ConfigureKestrel(opt => opt.Limits.MaxRequestBodySize = 30 * 1024 * 1024); // 30 MB

// -------------------------------------------------------
// Pipeline
// -------------------------------------------------------
var app = builder.Build();

await app.ApplyMigrationsAsync();
await app.SeedDatabaseAsync();

if (app.Environment.IsDevelopment())
    app.UseAppSwagger();

if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAppStaticFiles(uploadsBasePath);
app.UseForwardedHeaders();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
