using friendout_backend.Helpers;

namespace friendout_backend.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddAppCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? EnvLoader.GetCommaSeparated(configuration, "Cors:AllowedOrigins")
                    ?? new[] { "http://localhost:5173", "http://localhost:5122" };

                policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
