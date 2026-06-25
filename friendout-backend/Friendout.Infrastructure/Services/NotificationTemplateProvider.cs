using System.IO;
using System.Threading.Tasks;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Reads HTML notification templates from the filesystem.
///
/// Convention: {type}.{locale}.html — e.g. activityreminder.fr.html
/// Fallback chain:
///   1. {type}.{locale}.html          — exact match (e.g. activityreminder.fr.html)
///   2. {type}.en.html                — English fallback
///   3. general.{locale}.html         — general template in user's locale
///   4. general.en.html               — general template in English
///   5. Inline minimal HTML           — last resort, never fails
///
/// To add a new language: create the corresponding .{locale}.html files.
/// No code changes required.
/// </summary>
public class NotificationTemplateProvider : INotificationTemplateProvider
{
    private readonly string _templatesPath;
    private readonly ILogger<NotificationTemplateProvider> _logger;

    public NotificationTemplateProvider(IHostEnvironment env, ILogger<NotificationTemplateProvider> logger)
    {
        _logger        = logger;
        _templatesPath = Path.Combine(env.ContentRootPath, "Templates", "Notifications");
    }

    public async Task<string> GetTemplateAsync(NotificationType type, string locale = "fr")
    {
        var typeName = type.ToString().ToLowerInvariant();

        // 1. Exact match: e.g. fr/activityreminder.fr.html
        var exactPath = Path.Combine(_templatesPath, locale, $"{typeName}.{locale}.html");
        if (File.Exists(exactPath))
            return await File.ReadAllTextAsync(exactPath);

        _logger.LogWarning("Template not found: {File}. Trying English fallback.", exactPath);

        // 2. English fallback: e.g. en/activityreminder.en.html
        var englishPath = Path.Combine(_templatesPath, "en", $"{typeName}.en.html");
        if (File.Exists(englishPath))
            return await File.ReadAllTextAsync(englishPath);

        _logger.LogWarning("English template not found: {File}. Falling back to general template.", englishPath);

        // 3. General template in user's locale: e.g. fr/general.fr.html
        if (type != NotificationType.General)
            return await GetTemplateAsync(NotificationType.General, locale);

        // 4. General template in English: en/general.en.html
        var generalEnglishPath = Path.Combine(_templatesPath, "en", "general.en.html");
        if (File.Exists(generalEnglishPath))
            return await File.ReadAllTextAsync(generalEnglishPath);

        // 5. Inline last resort — should never happen in production
        _logger.LogCritical("All notification templates missing (type={Type}, locale={Locale}). Falling back to inline HTML. Check Templates/Notifications deployment.", type, locale);
        return """
            <html>
            <body style="margin:0;padding:0;background-color:#f8fafc;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Helvetica,Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f8fafc;">
                <tr>
                  <td align="center" style="padding:40px 16px;">
                    <table width="520" cellpadding="0" cellspacing="0" border="0" style="max-width:520px;width:100%;background-color:#ffffff;border-radius:24px;overflow:hidden;">
                      <tr>
                        <td style="padding:32px 32px 8px;">
                          <p style="margin:0;font-size:17px;font-weight:700;color:#18181b;">Friendout</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:8px 32px 32px;">
                          <p style="margin:0;color:#3f3f46;font-size:15px;line-height:1.7;">{{ Message }}</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}
