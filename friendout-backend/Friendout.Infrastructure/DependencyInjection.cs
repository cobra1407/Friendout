using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Friendout.Infrastructure.Services;
using Friendout.Infrastructure.WebSocketHub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;

namespace Friendout.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="webRootPath">The root web path where files will be stored.</param>
    /// <param name="configuration">The application configuration.</param>
    public static void AddInfrastructure(this IServiceCollection services, string webRootPath, IConfiguration configuration)
    {
        // ---- Options (validated at startup — app fails fast if any required key is missing) ----
        services.AddOptions<AppOptions>()
            .Bind(configuration.GetSection(AppOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // ---- Services ----
        services.AddHttpContextAccessor();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        services.AddScoped<IAppLogService, AppLogService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IEquipmentListService, EquipmentListService>();
        services.AddScoped<IParticipantService, ParticipantService>();
        services.AddScoped<ICommentService, CommentService>();

        // ---- Geocoding (reverse geocoding for Maps links with raw coordinates) ----
        services.AddHttpClient<IGeocodingService, NominatimGeocodingService>();

        // ---- Notification system ----
        services.AddScoped<INotificationTemplateProvider, NotificationTemplateProvider>();
        services.AddScoped<ITemplateEngine, SimpleTemplateEngine>();
        services.AddScoped<INotificationStrategy, EmailNotificationStrategy>();  // ← email channel
        services.AddScoped<INotificationStrategy, InAppNotificationStrategy>();  // ← in-app channel
        services.AddScoped<INotificationStrategy, WebSocketNotificationStrategy>(); // ← live push channel
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddScoped<IInAppNotificationService, InAppNotificationService>();

        // ---- Real-time (SignalR) ----
        // SignalR uses its own JSON serializer, entirely separate from the MVC controllers'
        // AddJsonOptions (Program.cs) — without this, enums like ParticipationStatus are sent
        // as raw numbers over the hub (0, 1, 2…) instead of the strings ("Participating", …)
        services.AddSignalR(options =>
        {
            // The global HTTP rate limiter (Program.cs) only covers the initial negotiate
            // request — once a WebSocket is upgraded, hub method calls bypass it entirely.
            // This filter closes that gap (see HubRateLimitFilter for details).
            options.AddFilter<HubRateLimitFilter>();
        }).AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddSingleton<IActivitiesHubNotifier, ActivitiesHubNotifier>();
        services.AddSingleton<HubRateLimitFilter>();

        // ---- File services ----
        services.AddScoped<IFileValidationService, FileValidationService>();
        services.AddScoped<IFileService>(provider =>
        {
            var validationService = provider.GetRequiredService<IFileValidationService>();
            var appOptions = provider.GetRequiredService<IOptions<AppOptions>>();
            return new FileService(webRootPath, validationService, appOptions);
        });

        services.AddScoped<JwtService>();
    }
}
