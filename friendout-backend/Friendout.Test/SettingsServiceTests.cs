using FluentAssertions;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Admin;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace Friendout.Test;

public class SettingsServiceTests
{
    // -------------------------
    // Helpers
    // -------------------------

    private sealed class LogSpy : IAppLogService
    {
        public List<string> Infos { get; } = new();
        public List<string> Warnings { get; } = new();
        public Task LogInfoAsync(string category, string message) { Infos.Add(message); return Task.CompletedTask; }
        public Task LogWarningAsync(string category, string message) { Warnings.Add(message); return Task.CompletedTask; }
        public Task LogErrorAsync(string category, string message, Exception? ex = null) => Task.CompletedTask;
    }

    private static SettingsService CreateService(FriendoutDbContext db, LogSpy logSpy, string? actorId = null, string? actorName = null)
    {
        var httpContext = new DefaultHttpContext();
        if (actorId != null)
            httpContext.Items["UserId"] = actorId;

        if (actorId != null && actorName != null)
        {
            db.Users.Add(new User { Id = actorId, Name = actorName });
            db.SaveChanges();
        }

        return new SettingsService(db, logSpy, new HttpContextAccessor { HttpContext = httpContext });
    }

    // -------------------------
    // GetAccessSettingsAsync
    // -------------------------

    [Test]
    public async Task GetAccessSettings_ReturnsBothFalse_WhenNoSettingsStored()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessSettings_ReturnsBothFalse_WhenNoSettingsStored));
        var service = CreateService(db, new LogSpy());

        var result = await service.GetAccessSettingsAsync();

        result.DiscordRestricted.Should().BeFalse();
        result.GoogleRestricted.Should().BeFalse();
    }

    [Test]
    public async Task GetAccessSettings_ReturnsStoredValues()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessSettings_ReturnsStoredValues));
        db.AppSettings.AddRange(
            new AppSetting { Key = "discord_restricted", Value = "true" },
            new AppSetting { Key = "google_restricted", Value = "false" }
        );
        await db.SaveChangesAsync();
        var service = CreateService(db, new LogSpy());

        var result = await service.GetAccessSettingsAsync();

        result.DiscordRestricted.Should().BeTrue();
        result.GoogleRestricted.Should().BeFalse();
    }

    // -------------------------
    // UpdateAccessSettingsAsync — persistence
    // -------------------------

    [Test]
    public async Task UpdateAccessSettings_PersistsBothValues()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_PersistsBothValues));
        db.AllowedGuilds.Add(new AllowedGuild { GuildId = "123" });
        db.AllowedEmails.Add(new AllowedEmail { Email = "thomas@gmail.com" });
        await db.SaveChangesAsync();
        var service = CreateService(db, new LogSpy(), "admin-1", "Thomas");

        var result = await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(true, true));

        result.IsSuccess.Should().BeTrue();
        var stored = await service.GetAccessSettingsAsync();
        stored.DiscordRestricted.Should().BeTrue();
        stored.GoogleRestricted.Should().BeTrue();
    }

    // -------------------------
    // UpdateAccessSettingsAsync — no_login_method_left guard
    // -------------------------

    [Test]
    public async Task UpdateAccessSettings_AllowsEnablingDiscordAlone_EvenWithEmptyGuildAllowlist()
    {
        // Disabling Discord as a login method while Google stays open is a valid admin
        // choice on its own — only leaving *both* providers unusable should be refused.
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_AllowsEnablingDiscordAlone_EvenWithEmptyGuildAllowlist));
        var service = CreateService(db, new LogSpy(), "admin-1", "Thomas");

        var result = await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(true, false));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task UpdateAccessSettings_RefusesUpdate_WhenItWouldLeaveNoLoginMethod()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_RefusesUpdate_WhenItWouldLeaveNoLoginMethod));
        var service = CreateService(db, new LogSpy(), "admin-1", "Thomas");

        var result = await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(true, true));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("no_login_method_left");
    }

    [Test]
    public async Task UpdateAccessSettings_RefusesUpdate_DoesNotPersistAnything()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_RefusesUpdate_DoesNotPersistAnything));
        var service = CreateService(db, new LogSpy(), "admin-1", "Thomas");

        await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(true, true));

        var stored = await service.GetAccessSettingsAsync();
        stored.DiscordRestricted.Should().BeFalse();
        stored.GoogleRestricted.Should().BeFalse();
    }

    [Test]
    public async Task UpdateAccessSettings_AllowsBothRestricted_WhenBothAllowlistsHaveEntries()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_AllowsBothRestricted_WhenBothAllowlistsHaveEntries));
        db.AllowedGuilds.Add(new AllowedGuild { GuildId = "123" });
        db.AllowedEmails.Add(new AllowedEmail { Email = "thomas@gmail.com" });
        await db.SaveChangesAsync();
        var service = CreateService(db, new LogSpy(), "admin-1", "Thomas");

        var result = await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(true, true));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------
    // UpdateAccessSettingsAsync — Discord logging
    // -------------------------

    [Test]
    public async Task UpdateAccessSettings_LogsInfo_WithActorName_WhenDiscordRestrictionEnabled()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_LogsInfo_WithActorName_WhenDiscordRestrictionEnabled));
        var logSpy = new LogSpy();
        var service = CreateService(db, logSpy, "admin-1", "Thomas");

        await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(true, false));

        logSpy.Infos.Should().ContainSingle()
            .Which.Should().Contain("Thomas").And.Contain("admin-1").And.Contain("Discord");
        logSpy.Warnings.Should().BeEmpty();
    }

    [Test]
    public async Task UpdateAccessSettings_LogsWarning_WithActorName_WhenDiscordRestrictionDisabled()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_LogsWarning_WithActorName_WhenDiscordRestrictionDisabled));
        db.AppSettings.Add(new AppSetting { Key = "discord_restricted", Value = "true" });
        await db.SaveChangesAsync();
        var logSpy = new LogSpy();
        var service = CreateService(db, logSpy, "admin-2", "Alice");

        await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(false, false));

        logSpy.Warnings.Should().ContainSingle()
            .Which.Should().Contain("Alice").And.Contain("admin-2").And.Contain("Discord");
        logSpy.Infos.Should().BeEmpty();
    }

    // -------------------------
    // UpdateAccessSettingsAsync — Google logging
    // -------------------------

    [Test]
    public async Task UpdateAccessSettings_LogsInfo_WithActorName_WhenGoogleRestrictionEnabled()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_LogsInfo_WithActorName_WhenGoogleRestrictionEnabled));
        var logSpy = new LogSpy();
        var service = CreateService(db, logSpy, "admin-1", "Thomas");

        await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(false, true));

        logSpy.Infos.Should().ContainSingle()
            .Which.Should().Contain("Thomas").And.Contain("admin-1").And.Contain("Google");
        logSpy.Warnings.Should().BeEmpty();
    }

    [Test]
    public async Task UpdateAccessSettings_LogsWarning_WithActorName_WhenGoogleRestrictionDisabled()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_LogsWarning_WithActorName_WhenGoogleRestrictionDisabled));
        db.AppSettings.Add(new AppSetting { Key = "google_restricted", Value = "true" });
        await db.SaveChangesAsync();
        var logSpy = new LogSpy();
        var service = CreateService(db, logSpy, "admin-2", "Alice");

        await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(false, false));

        logSpy.Warnings.Should().ContainSingle()
            .Which.Should().Contain("Alice").And.Contain("admin-2").And.Contain("Google");
        logSpy.Infos.Should().BeEmpty();
    }

    // -------------------------
    // UpdateAccessSettingsAsync — no-op / unknown actor
    // -------------------------

    [Test]
    public async Task UpdateAccessSettings_LogsNothing_WhenValuesUnchanged()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_LogsNothing_WhenValuesUnchanged));
        db.AppSettings.AddRange(
            new AppSetting { Key = "discord_restricted", Value = "true" },
            new AppSetting { Key = "google_restricted", Value = "false" }
        );
        await db.SaveChangesAsync();
        var logSpy = new LogSpy();
        var service = CreateService(db, logSpy, "admin-1", "Thomas");

        await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(true, false));

        logSpy.Infos.Should().BeEmpty();
        logSpy.Warnings.Should().BeEmpty();
    }

    [Test]
    public async Task UpdateAccessSettings_LogsBoth_WhenBothValuesChangeInOneCall()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_LogsBoth_WhenBothValuesChangeInOneCall));
        db.AllowedGuilds.Add(new AllowedGuild { GuildId = "123" });
        db.AllowedEmails.Add(new AllowedEmail { Email = "thomas@gmail.com" });
        await db.SaveChangesAsync();
        var logSpy = new LogSpy();
        var service = CreateService(db, logSpy, "admin-1", "Thomas");

        await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(true, true));

        logSpy.Infos.Should().HaveCount(2);
        logSpy.Infos.Should().Contain(m => m.Contains("Discord"));
        logSpy.Infos.Should().Contain(m => m.Contains("Google"));
    }

    [Test]
    public async Task UpdateAccessSettings_LogsUnknownActor_WhenNoHttpContextUser()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateAccessSettings_LogsUnknownActor_WhenNoHttpContextUser));
        var logSpy = new LogSpy();
        var service = CreateService(db, logSpy); // no actorId provided

        await service.UpdateAccessSettingsAsync(new UpdateAccessSettingsDto(true, false));

        logSpy.Infos.Should().ContainSingle()
            .Which.Should().Contain("unknown");
    }
}
