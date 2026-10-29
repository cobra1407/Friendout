using Friendout.Infrastructure.Services;
using Friendout.Infrastructure.Utils;
using FluentAssertions;
using NUnit.Framework;

namespace Friendout.Test;

public class UrlSafetyTests
{
    [TestCase("javascript:alert(1)")]
    [TestCase("  javascript:alert(document.cookie)")]
    [TestCase("JaVaScRiPt:alert(1)")]
    [TestCase("data:text/html,<script>alert(1)</script>")]
    [TestCase("vbscript:msgbox(1)")]
    [TestCase("file:///etc/passwd")]
    [TestCase("//evil.com")]
    [TestCase("/relative/path")]
    [TestCase("https://user:pass@discord.com/x")]
    [TestCase("https://discord.com/\u0001x")]
    public void IsHttpUrl_RejectsUnsafeValues(string url)
    {
        UrlSafety.IsHttpUrl(url).Should().BeFalse();
        UrlSafety.IsTrustedVirtualUrl(url).Should().BeFalse();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("https://www.google.com/maps/place/Paris")]
    [TestCase("http://example.com")]
    public void IsHttpUrl_AcceptsEmptyAndHttpUrls(string? url)
    {
        UrlSafety.IsHttpUrl(url).Should().BeTrue();
    }

    [TestCase("https://discord.gg/abc")]
    [TestCase("https://meet.google.com/abc-defg-hij")]
    [TestCase("https://us02web.zoom.us/j/123")]
    public void IsTrustedVirtualUrl_AcceptsKnownPlatforms(string url)
    {
        UrlSafety.IsTrustedVirtualUrl(url).Should().BeTrue();
    }

    [TestCase("https://evil.com")]
    [TestCase("https://discord.com.evil.com")]
    [TestCase("https://evildiscord.com")]
    [TestCase("https://evil.com/?u=discord.com")]
    public void IsTrustedVirtualUrl_RejectsLookalikeHosts(string url)
    {
        UrlSafety.IsTrustedVirtualUrl(url).Should().BeFalse();
    }

    [Test]
    public void TemplateEngine_HtmlEncodesUserValues()
    {
        var engine = new SimpleTemplateEngine();

        var result = engine.Render("<p>{{ ActivityName }}</p>",
            new Dictionary<string, string> { ["ActivityName"] = "<script>alert(1)</script>" });

        result.Should().NotContain("<script>");
        result.Should().Contain("&lt;script&gt;");
    }
}
