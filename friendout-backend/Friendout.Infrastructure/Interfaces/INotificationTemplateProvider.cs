using System.Threading.Tasks;
using Friendout.Domain.Enums;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Provides the raw HTML content of notification templates.
///
/// Templates follow the naming convention: {type}.{locale}.html
/// Example: activityreminder.fr.html, accessrequestapproved.en.html
///
/// Fallback chain when a template is missing:
///   1. {type}.{locale}.html
///   2. {type}.en.html
///   3. general.{locale}.html
///   4. general.en.html
///   5. Inline minimal HTML
/// </summary>
public interface INotificationTemplateProvider
{
    /// <summary>
    /// Returns the raw HTML template for the given notification type and locale.
    /// </summary>
    /// <param name="type">The notification type (determines the file name).</param>
    /// <param name="locale">The user's locale code (e.g. "fr", "en"). Defaults to "fr".</param>
    Task<string> GetTemplateAsync(NotificationType type, string locale = "en");
}
