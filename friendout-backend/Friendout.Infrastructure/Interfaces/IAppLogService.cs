using System;
using System.Threading.Tasks;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Logs meaningful application events both to stdout (Docker) and to the database (admin panel).
/// Not intended for high-frequency/debug logging — use ILogger directly for that.
/// </summary>
public interface IAppLogService
{
    Task LogInfoAsync(string category, string message);
    Task LogWarningAsync(string category, string message);
    Task LogErrorAsync(string category, string message, Exception? ex = null);
}
