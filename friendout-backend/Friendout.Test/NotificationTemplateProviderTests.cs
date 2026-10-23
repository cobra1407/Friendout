using System.IO;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Friendout.Test;

public class NotificationTemplateProviderTests
{
    private string _root = null!;
    private NotificationTemplateProvider _provider = null!;

    [SetUp]
    public void Setup()
    {
        _root = Path.Combine(Path.GetTempPath(), "friendout-tests-" + Guid.NewGuid().ToString("N"));
        var templatesDir = Path.Combine(_root, "Templates");
        var notificationsDir = Path.Combine(templatesDir, "Notifications");

        WriteTemplate(Path.Combine(notificationsDir, "en", "activityreminder.en.html"), "EN");
        WriteTemplate(Path.Combine(notificationsDir, "fr", "activityreminder.fr.html"), "FR");
        WriteTemplate(Path.Combine(notificationsDir, "pt-BR", "activityreminder.pt-BR.html"), "PT");

        // Files that an unvalidated locale would reach (file name is {type}.{locale}.html):
        // locale ""   -> Templates/Notifications/activityreminder..html
        // locale "."  -> Templates/Notifications/activityreminder...html
        // locale ".." -> Templates/activityreminder....html
        WriteTemplate(Path.Combine(notificationsDir, "activityreminder..html"), "TRAPPED");
        WriteTemplate(Path.Combine(notificationsDir, "activityreminder...html"), "TRAPPED");
        WriteTemplate(Path.Combine(templatesDir, "activityreminder....html"), "TRAPPED");

        _provider = new NotificationTemplateProvider(
            new FakeHostEnvironment(_root),
            TestLogger<NotificationTemplateProvider>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    [TestCase("en", "EN")]
    [TestCase("fr", "FR")]
    [TestCase("pt-BR", "PT")]
    public async Task GetTemplateAsync_WithValidLocale_ReturnsLocalizedTemplate(string locale, string expected)
    {
        var result = await _provider.GetTemplateAsync(NotificationType.ActivityReminder, locale);

        result.Should().Be(expected);
    }

    [Test]
    public async Task GetTemplateAsync_WithValidButUnknownLocale_FallsBackToEnglish()
    {
        var result = await _provider.GetTemplateAsync(NotificationType.ActivityReminder, "de");

        result.Should().Be("EN");
    }

    [TestCase(".")]
    [TestCase("..")]
    [TestCase("../etc/passwd")]
    [TestCase("fr/../en")]
    [TestCase("fr\n")]
    [TestCase("FR")]
    [TestCase("")]
    public async Task GetTemplateAsync_WithInvalidLocale_FallsBackToEnglish(string locale)
    {
        var result = await _provider.GetTemplateAsync(NotificationType.ActivityReminder, locale);

        result.Should().Be("EN");
    }

    private static void WriteTemplate(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public FakeHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
        }

        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "Friendout.Test";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
