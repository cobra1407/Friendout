using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Friendout.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        services.AddScoped<IAppLogService, AppLogService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IParticipantService, ParticipantService>();
        services.AddScoped<ICommentService, CommentService>();

        // ---- Notification system ----
        services.AddScoped<INotificationTemplateProvider, NotificationTemplateProvider>();
        services.AddScoped<ITemplateEngine, SimpleTemplateEngine>();
        services.AddScoped<INotificationStrategy, EmailNotificationStrategy>();  // ← email channel
        services.AddScoped<INotificationStrategy, InAppNotificationStrategy>();  // ← in-app channel
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddScoped<IInAppNotificationService, InAppNotificationService>();

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
