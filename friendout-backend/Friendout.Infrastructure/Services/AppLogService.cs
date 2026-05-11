using System;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

public class AppLogService : IAppLogService
{
    private readonly FriendoutDbContext _db;
    private readonly ILogger<AppLogService> _logger;

    public AppLogService(FriendoutDbContext db, ILogger<AppLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public Task LogInfoAsync(string category, string message)
        => WriteAsync(AppLogLevel.Info, category, message, null);

    public Task LogWarningAsync(string category, string message)
        => WriteAsync(AppLogLevel.Warning, category, message, null);

    public Task LogErrorAsync(string category, string message, Exception? ex = null)
        => WriteAsync(AppLogLevel.Error, category, message, ex);

    private async Task WriteAsync(AppLogLevel level, string category, string message, Exception? ex)
    {
        // Forward to ASP.NET Core ILogger → visible in Docker stdout
        var logLevel = level switch
        {
            AppLogLevel.Warning => LogLevel.Warning,
            AppLogLevel.Error   => LogLevel.Error,
            _                   => LogLevel.Information
        };
        _logger.Log(logLevel, ex, "[{Category}] {Message}", category, message);

        // Persist in DB for the admin panel
        _db.AppLogs.Add(new AppLog
        {
            Level     = level,
            Category  = category,
            Message   = message,
            Exception = ex?.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception saveEx)
        {
            // Never let a log failure crash the app — just warn on stdout
            _logger.LogWarning(saveEx, "AppLogService: failed to persist log to database.");
        }
    }
}
