using System;
using System.Globalization;

namespace Friendout.Infrastructure.Utils;

public static class LocaleHelper
{
    /// <summary>
    /// Maps a stored user locale (e.g. "fr", "en") to its CultureInfo.
    /// Falls back to English for unknown/unsupported locales.
    /// </summary>
    public static CultureInfo ResolveCulture(string? locale)
    {
        return locale switch
        {
            "fr" => CultureInfo.GetCultureInfo("fr-FR"),
            "en" => CultureInfo.GetCultureInfo("en-US"),
            _ => CultureInfo.GetCultureInfo("en-US")
        };
    }

    /// <summary>
    /// Formats a date using the given locale instead of the server's default culture.
    /// Without this, DateTime.ToString(format) silently uses CultureInfo.CurrentCulture,
    /// which depends on server configuration, not the recipient's preference — meaning
    /// every email shows the date in the same language regardless of who it's sent to.
    /// </summary>
    public static string FormatDate(DateTime date, string format, string? locale)
    {
        return date.ToString(format, ResolveCulture(locale));
    }
}
