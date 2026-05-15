using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Friendout.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="webRootPath">The root web path where files will be stored (usually from IWebHostEnvironment.WebRootPath or ContentRootPath).</param>
    public static void AddInfrastructure(this IServiceCollection services, string webRootPath)
    {
        // Add services 
        services.AddHttpContextAccessor();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IAppLogService, AppLogService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IParticipantService, ParticipantService>();
        services.AddScoped<ICommentService, CommentService>();
        
        // File validation service (must be registered before FileService)
        services.AddScoped<IFileValidationService, FileValidationService>();
        
        // Configure FileService with the base path
        // The uploads folder will be created in the API project
        services.AddScoped<IFileService>(provider => 
        {
            var validationService = provider.GetRequiredService<IFileValidationService>();
            return new FileService(webRootPath, validationService);
        });
        
        services.AddScoped<JwtService>();
        
        // Websocket
    }
}