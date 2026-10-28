using System;
using System.Linq;

namespace Friendout.Infrastructure.Utils;

/// <summary>
/// Server-side validation of user-supplied URLs (stored XSS / phishing protection).
/// Anything that ends up in an href or window.open() on the client must pass through here.
/// </summary>
public static class UrlSafety
{
    // Voice / video rooms only (no streaming or social platforms). A host matches when it is
    // exactly one of these domains or a subdomain of it (e.g. us02web.zoom.us).
    private static readonly string[] TrustedVirtualDomains =
    {
        // Discord
        "discord.gg", "discord.com", "discordapp.com", "discord.me",
        // TeamSpeak
        "teamspeak.com", "ts3server.info", "ts3server.com",
        // Google Meet
        "meet.google.com",
        // Zoom
        "zoom.us", "zoom.com",
        // Microsoft Teams / Skype
        "teams.microsoft.com", "teams.live.com", "join.skype.com",
        // Others
        "webex.com",
        "whereby.com",
        "meet.jit.si",
        "gotomeet.me", "gotomeeting.com",
        "app.slack.com",
        "signal.group",
        "mumble.info",
        "revolt.chat", "stoat.chat",
        "guilded.gg"
    };

    /// <summary>
    /// True when the value is empty or an absolute http(s) URL.
    /// Rejects javascript:, data:, vbscript:, file:, relative URLs, URLs with credentials
    /// and anything containing control characters.
    /// </summary>
    public static bool IsHttpUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        return TryParseHttpUrl(url, out _);
    }

    /// <summary>
    /// True when the value is empty or an http(s) URL whose host is one of the trusted
    /// virtual-meeting platforms (or a subdomain of one).
    /// </summary>
    public static bool IsTrustedVirtualUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        if (!TryParseHttpUrl(url, out var uri)) return false;

        var host = uri.Host.ToLowerInvariant();
        return TrustedVirtualDomains.Any(domain => host == domain || host.EndsWith("." + domain));
    }

    private static bool TryParseHttpUrl(string url, out Uri uri)
    {
        uri = null!;
        var trimmed = url.Trim();

        if (trimmed.Any(char.IsControl)) return false;
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var parsed)) return false;
        if (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps) return false;
        if (!string.IsNullOrEmpty(parsed.UserInfo)) return false;

        uri = parsed;
        return true;
    }
}
