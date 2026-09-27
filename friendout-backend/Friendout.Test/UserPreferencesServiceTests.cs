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
}
