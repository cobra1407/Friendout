using Friendout.Domain.Context;
using Friendout.Domain.Seeds;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

namespace friendout_backend.Extensions;

public static class ApplicationExtensions
{
    /// <summary>
    /// Configures forwarded headers so ASP.NET Core reads X-Forwarded-Proto
    /// correctly when running behind a reverse proxy (Docker / nginx).
    /// Must be registered before AddAuthentication.
    /// </summary>
    public static IServiceCollection AddAppForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            // Clear default restrictions so all proxies in the Docker network are trusted.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    /// <summary>
    /// Applies database migrations and optionally seeds the database.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();
        await db.Database.MigrateAsync();
    }

    /// <summary>
    /// Seeds the database when Seed:Enabled is true in configuration.
    /// Useful for first-time Docker setup.
    /// </summary>
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Seed:Enabled", false)) return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();
        await DatabaseSeeder.SeedAsync(db);
    }

    /// <summary>
    /// Configures static file serving for uploaded files.
    /// </summary>
    public static IApplicationBuilder UseAppStaticFiles(
        this IApplicationBuilder app,
        string basePath)
    {
        var uploadsPath = Path.Combine(basePath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
            RequestPath  = "/uploads"
        });

        return app;
    }
}
