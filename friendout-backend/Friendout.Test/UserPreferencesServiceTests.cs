using FluentAssertions;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Preferences;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Services;

namespace Friendout.Test;

public class UserPreferencesServiceTests
{
    // -------------------------
    // Helpers
    // -------------------------

    private static UserPreferencesService CreateService(FriendoutDbContext db) => new(db);

    private static async Task<string> SeedUserAsync(FriendoutDbContext db, string userId = "user-1")
    {
        db.Users.Add(new User { Id = userId, Name = "Thomas" });
        await db.SaveChangesAsync();
        return userId;
    }

    // -------------------------
    // GetMyPreferencesAsync — locale
    // -------------------------

    [Test]
    public async Task GetMyPreferences_ReturnsDefaultLocale_WhenNoRowStored()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetMyPreferences_ReturnsDefaultLocale_WhenNoRowStored));
        var userId = await SeedUserAsync(db);
        var service = CreateService(db);

        var result = await service.GetMyPreferencesAsync(userId);

        result.Locale.Should().Be("en");
    }

    [Test]
    public async Task GetMyPreferences_ReturnsStoredLocale()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetMyPreferences_ReturnsStoredLocale));
        var userId = await SeedUserAsync(db);
        db.UserPreferences.Add(new UserPreferences { UserId = userId, Locale = "fr" });
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.GetMyPreferencesAsync(userId);

        result.Locale.Should().Be("fr");
    }

    // -------------------------
    // UpdateUserPreferencesAsync — locale validation
    // -------------------------

    [TestCase("fr")]
    [TestCase("en")]
    public async Task UpdateUserPreferences_AcceptsSupportedLocale(string locale)
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext($"{nameof(UpdateUserPreferences_AcceptsSupportedLocale)}_{locale}");
        var userId = await SeedUserAsync(db);
        var service = CreateService(db);

        var result = await service.UpdateUserPreferencesAsync(
            userId,
            new UpdateUserPreferencesDto(locale, true, true, "default", false));

        result.IsSuccess.Should().BeTrue();
        result.Data!.Locale.Should().Be(locale);
    }

    [Test]
    public async Task UpdateUserPreferences_RefusesUnsupportedLocale()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserPreferences_RefusesUnsupportedLocale));
        var userId = await SeedUserAsync(db);
        var service = CreateService(db);

        var result = await service.UpdateUserPreferencesAsync(
            userId,
            new UpdateUserPreferencesDto("de", true, true, "default", false));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("de");
    }

    [Test]
    public async Task UpdateUserPreferences_RefusesUnsupportedLocale_DoesNotPersistAnything()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserPreferences_RefusesUnsupportedLocale_DoesNotPersistAnything));
        var userId = await SeedUserAsync(db);
        var service = CreateService(db);

        await service.UpdateUserPreferencesAsync(
            userId,
            new UpdateUserPreferencesDto("de", true, true, "default", false));

        var stored = await service.GetMyPreferencesAsync(userId);
        stored.Locale.Should().Be("en"); // still the default, nothing was written
    }

    [Test]
    public async Task UpdateUserPreferences_CreatesRow_WhenNoneExistedYet()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserPreferences_CreatesRow_WhenNoneExistedYet));
        var userId = await SeedUserAsync(db);
        var service = CreateService(db);

        await service.UpdateUserPreferencesAsync(
            userId,
            new UpdateUserPreferencesDto("fr", true, true, "default", false));

        var stored = await db.UserPreferences.FindAsync(userId);
        stored.Should().NotBeNull();
        stored!.Locale.Should().Be("fr");
    }

    [Test]
    public async Task UpdateUserPreferences_UpdatesExistingRow_InsteadOfDuplicating()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserPreferences_UpdatesExistingRow_InsteadOfDuplicating));
        var userId = await SeedUserAsync(db);
        db.UserPreferences.Add(new UserPreferences { UserId = userId, Locale = "en" });
        await db.SaveChangesAsync();
        var service = CreateService(db);

        await service.UpdateUserPreferencesAsync(
            userId,
            new UpdateUserPreferencesDto("fr", true, true, "default", false));

        db.UserPreferences.Count(p => p.UserId == userId).Should().Be(1);
        (await db.UserPreferences.FindAsync(userId))!.Locale.Should().Be("fr");
    }

    // -------------------------
    // GetMyPreferencesAsync — notification channels
    // -------------------------

    [Test]
    public async Task GetMyPreferences_ReturnsDefaultNotificationPrefs_WhenNoRowStored()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetMyPreferences_ReturnsDefaultNotificationPrefs_WhenNoRowStored));
        var userId = await SeedUserAsync(db);
        var service = CreateService(db);

        var result = await service.GetMyPreferencesAsync(userId);

        result.EmailEnabled.Should().BeTrue();
        result.InAppEnabled.Should().BeTrue();
        result.NotificationSound.Should().Be("default");
        result.AccessRequestAlertsEnabled.Should().BeFalse();
    }

    [Test]
    public async Task GetMyPreferences_ReturnsStoredNotificationPrefs()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetMyPreferences_ReturnsStoredNotificationPrefs));
        var userId = await SeedUserAsync(db);
        db.UserNotificationPreferences.Add(new UserNotificationPreferences
        {
            UserId = userId,
            EmailEnabled = false,
            InAppEnabled = true,
            NotificationSound = "chime",
            AccessRequestAlertsEnabled = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.GetMyPreferencesAsync(userId);

        result.EmailEnabled.Should().BeFalse();
        result.InAppEnabled.Should().BeTrue();
        result.NotificationSound.Should().Be("chime");
        result.AccessRequestAlertsEnabled.Should().BeTrue();
    }

    // -------------------------
    // UpdateUserPreferencesAsync — notification channels
    // -------------------------

    [Test]
    public async Task UpdateUserPreferences_PersistsNotificationChannels()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserPreferences_PersistsNotificationChannels));
        var userId = await SeedUserAsync(db);
        var service = CreateService(db);

        var result = await service.UpdateUserPreferencesAsync(
            userId,
            new UpdateUserPreferencesDto("en", false, true, "chime", true));

        result.IsSuccess.Should().BeTrue();
        result.Data!.EmailEnabled.Should().BeFalse();
        result.Data!.InAppEnabled.Should().BeTrue();
        result.Data!.NotificationSound.Should().Be("chime");
        result.Data!.AccessRequestAlertsEnabled.Should().BeTrue();

        var stored = await db.UserNotificationPreferences.FindAsync(userId);
        stored!.EmailEnabled.Should().BeFalse();
        stored.InAppEnabled.Should().BeTrue();
        stored.NotificationSound.Should().Be("chime");
        stored.AccessRequestAlertsEnabled.Should().BeTrue();
    }

    [Test]
    public async Task UpdateUserPreferences_UpdatesExistingNotificationRow_InsteadOfDuplicating()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserPreferences_UpdatesExistingNotificationRow_InsteadOfDuplicating));
        var userId = await SeedUserAsync(db);
        db.UserNotificationPreferences.Add(new UserNotificationPreferences { UserId = userId, EmailEnabled = true, InAppEnabled = true });
        await db.SaveChangesAsync();
        var service = CreateService(db);

        await service.UpdateUserPreferencesAsync(
            userId,
            new UpdateUserPreferencesDto("en", false, false, "default", false));

        db.UserNotificationPreferences.Count(p => p.UserId == userId).Should().Be(1);
        (await db.UserNotificationPreferences.FindAsync(userId))!.EmailEnabled.Should().BeFalse();
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public async Task UpdateUserPreferences_FallsBackToDefaultSound_WhenSoundIsBlankOrNull(string? blankSound)
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext($"{nameof(UpdateUserPreferences_FallsBackToDefaultSound_WhenSoundIsBlankOrNull)}_{blankSound ?? "null"}");
        var userId = await SeedUserAsync(db);
        var service = CreateService(db);

        var result = await service.UpdateUserPreferencesAsync(
            userId,
            new UpdateUserPreferencesDto("en", true, true, blankSound!, false));

        result.Data!.NotificationSound.Should().Be("default");
    }

    [Test]
    public async Task UpdateUserPreferences_KeepsCustomSound_WhenNotBlank()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserPreferences_KeepsCustomSound_WhenNotBlank));
        var userId = await SeedUserAsync(db);
        var service = CreateService(db);

        var result = await service.UpdateUserPreferencesAsync(
            userId,
            new UpdateUserPreferencesDto("en", true, true, "chime", false));

        result.Data!.NotificationSound.Should().Be("chime");
    }

    [Test]
    public async Task UpdateUserPreferences_AllowsOptingIntoAccessRequestAlerts()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserPreferences_AllowsOptingIntoAccessRequestAlerts));
        var userId = await SeedUserAsync(db);
        var service = CreateService(db);

        var result = await service.UpdateUserPreferencesAsync(
            userId,
            new UpdateUserPreferencesDto("en", true, true, "default", true));

        result.Data!.AccessRequestAlertsEnabled.Should().BeTrue();
    }
}
