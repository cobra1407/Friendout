using friendout_backend;
using friendout_backend.Jobs;
using Friendout.Domain.Context;
using Friendout.Infrastructure;
using Friendout.Infrastructure.WebSocketHub;
using friendout_backend.Controller;
using friendout_backend.Extensions;
using friendout_backend.Helpers;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using friendout_backend.Converters;
using Microsoft.EntityFrameworkCore;
using Quartz;

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
builder.Services.AddAppHealthChecks();
builder.Services.AddControllers(options => options.Conventions.Add(new RoutePrefixConvention("api")))
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                    opt.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
                });

// AddInfrastructure registers AppOptions + SmtpOptions with ValidateOnStart —
// the app will fail to start if any required key is missing.
builder.Services.AddInfrastructure(uploadsBasePath, builder.Configuration);
builder.Services.AddDbContext<FriendoutDbContext>(opt =>
    // Pinned version (matches mysql:8.4 in docker-compose.yml) instead of
    // ServerVersion.AutoDetect(): AutoDetect hits the DB just to construct the
    // DbContext, which crashes before our own error handling (e.g. health check) runs.
    opt.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))));

builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddAppAuthentication(builder.Configuration);
builder.Services.AddHttpClient();

builder.Services.AddSingleton<Friendout.Infrastructure.Interfaces.ITokenBlacklistService,
                              Friendout.Infrastructure.Services.TokenBlacklistService>();
builder.Services.AddScoped<Friendout.Infrastructure.Interfaces.IRefreshTokenService,
                           Friendout.Infrastructure.Services.RefreshTokenService>();
builder.Services.AddHostedService<RefreshTokenCleanupService>();
builder.Services.AddAppQuartz();

builder.WebHost.ConfigureKestrel(opt => opt.Limits.MaxRequestBodySize = 30 * 1024 * 1024);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
            }));

    options.AddSlidingWindowLimiter("auth", policy =>
    {
        policy.PermitLimit = 10;
        policy.Window = TimeSpan.FromMinutes(1);
        policy.SegmentsPerWindow = 6;
    });
});

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
app.UseRateLimiter();
app.MapControllers();
app.MapAppHealthChecks();
app.MapHub<ActivitiesHub>("/hubs/activities").RequireAuthorization();
app.Run();
