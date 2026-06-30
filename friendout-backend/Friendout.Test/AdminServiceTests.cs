using FluentAssertions;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Admin;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Friendout.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Friendout.Test;

public class AdminServiceTests
{
    // -------------------------
    // Helpers
    // -------------------------

    private sealed class NullAppLogService : IAppLogService
    {
        public static readonly NullAppLogService Instance = new();
        public Task LogInfoAsync(string category, string message) => Task.CompletedTask;
        public Task LogWarningAsync(string category, string message) => Task.CompletedTask;
        public Task LogErrorAsync(string category, string message, Exception? ex = null) => Task.CompletedTask;
    }

    /// <summary>No-op INotificationDispatcher for tests — dispatches nothing.</summary>
    private sealed class NullNotificationDispatcher : INotificationDispatcher
    {
        public static readonly NullNotificationDispatcher Instance = new();
        public Task DispatchNotificationAsync(Guid userId, Friendout.Domain.Enums.NotificationType type, Dictionary<string, string> data)
            => Task.CompletedTask;
    }

    private sealed class LogSpy : IAppLogService
    {
        public List<string> Warnings { get; } = new();
        public Task LogInfoAsync(string category, string message) => Task.CompletedTask;
        public Task LogWarningAsync(string category, string message) { Warnings.Add(message); return Task.CompletedTask; }
        public Task LogErrorAsync(string category, string message, Exception? ex = null) => Task.CompletedTask;
    }

    /// <summary>Fake ISettingsService for tests — both restrictions disabled by default.</summary>
    private sealed class FakeSettingsService : ISettingsService
    {
        public static readonly FakeSettingsService Instance = new();
        public bool DiscordRestricted { get; set; } = false;
        public bool GoogleRestricted { get; set; } = false;
        public Task<AccessSettingsDto> GetAccessSettingsAsync()
            => Task.FromResult(new AccessSettingsDto(DiscordRestricted, GoogleRestricted));
        public Task<ServiceResult<AccessSettingsDto>> UpdateAccessSettingsAsync(UpdateAccessSettingsDto dto)
            => Task.FromResult(ServiceResult<AccessSettingsDto>.Success(new AccessSettingsDto(dto.DiscordRestricted, dto.GoogleRestricted)));
    }

    /// <summary>Fake IRefreshTokenService for tests — records which user ids had RevokeAllAsync called.</summary>
    private sealed class FakeRefreshTokenService : IRefreshTokenService
    {
        public static readonly FakeRefreshTokenService Instance = new();
        public List<string> RevokedAllForUserIds { get; } = new();
        public Task<string> CreateAsync(string userId) => Task.FromResult("fake-token");
        public Task<RefreshToken?> ValidateAsync(string token) => Task.FromResult<RefreshToken?>(null);
        public Task RevokeAsync(string token) => Task.CompletedTask;
        public Task<string> RotateAsync(string oldToken, string userId) => Task.FromResult("fake-token");
        public Task RevokeAllAsync(string userId) { RevokedAllForUserIds.Add(userId); return Task.CompletedTask; }
    }

    /// <summary>No-op IAppSettings for tests — returns a placeholder URL.</summary>
    private sealed class NullAppSettings
    {
        public static readonly NullAppSettings Instance = new();
        public string AppUrl => "https://localhost";
    }

    private static AdminService CreateService(FriendoutDbContext db)
        => new(db, NullAppLogService.Instance, new HttpContextAccessor(), NullNotificationDispatcher.Instance, FakeSettingsService.Instance, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

    // -------------------------
    // GetLogsAsync
    // -------------------------

    [Test]
    public async Task GetLogs_ReturnsEmpty_WhenNoLogsExist()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetLogs_ReturnsEmpty_WhenNoLogsExist));

        var result = await CreateService(db).GetLogsAsync(null, 100);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetLogs_ReturnsDescendingOrder()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetLogs_ReturnsDescendingOrder));
        db.AppLogs.AddRange(
            new AppLog { Level = AppLogLevel.Info,    Category = "Test", Message = "old", CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
            new AppLog { Level = AppLogLevel.Warning, Category = "Test", Message = "new", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetLogsAsync(null, 100);

        result.Should().HaveCount(2);
        result[0].Message.Should().Be("new");
        result[1].Message.Should().Be("old");
    }

    [Test]
    public async Task GetLogs_FiltersBy_Level()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetLogs_FiltersBy_Level));
        db.AppLogs.AddRange(
            new AppLog { Level = AppLogLevel.Info,  Category = "Test", Message = "info log" },
            new AppLog { Level = AppLogLevel.Error, Category = "Test", Message = "error log" }
        );
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetLogsAsync("Error", 100);

        result.Should().HaveCount(1);
        result[0].Level.Should().Be("Error");
    }

    [Test]
    public async Task GetLogs_RespectsLimit()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetLogs_RespectsLimit));
        for (var i = 0; i < 10; i++)
            db.AppLogs.Add(new AppLog { Level = AppLogLevel.Info, Category = "Test", Message = $"log {i}" });
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetLogsAsync(null, 3);

        result.Should().HaveCount(3);
    }

    [Test]
    public async Task GetLogs_RespectsSkip()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetLogs_RespectsSkip));
        for (var i = 0; i < 6; i++)
            db.AppLogs.Add(new AppLog { Level = AppLogLevel.Info, Category = "Test", Message = $"log {i}", CreatedAt = DateTime.UtcNow.AddMinutes(i) });
        await db.SaveChangesAsync();

        var page1 = await CreateService(db).GetLogsAsync(null, 3, 0);
        var page2 = await CreateService(db).GetLogsAsync(null, 3, 3);

        page1.Should().HaveCount(3);
        page2.Should().HaveCount(3);
        page1.Select(l => l.Id).Should().NotIntersectWith(page2.Select(l => l.Id));
    }

    // -------------------------
    // GetAllowedGuildsAsync
    // -------------------------

    [Test]
    public async Task GetAllowedGuilds_ReturnsEmpty_WhenNoGuildsExist()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAllowedGuilds_ReturnsEmpty_WhenNoGuildsExist));

        var result = await CreateService(db).GetAllowedGuildsAsync();

        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllowedGuilds_ReturnsGuilds_OrderedByCreatedAt()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAllowedGuilds_ReturnsGuilds_OrderedByCreatedAt));
        db.AllowedGuilds.AddRange(
            new AllowedGuild { GuildId = "111", Label = "First",  CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
            new AllowedGuild { GuildId = "222", Label = "Second", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetAllowedGuildsAsync();

        result.Should().HaveCount(2);
        result[0].GuildId.Should().Be("111");
        result[1].GuildId.Should().Be("222");
    }

    // -------------------------
    // AddAllowedGuildAsync
    // -------------------------

    [Test]
    public async Task AddAllowedGuild_ReturnsSuccess_WhenGuildIsNew()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(AddAllowedGuild_ReturnsSuccess_WhenGuildIsNew));

        var result = await CreateService(db).AddAllowedGuildAsync(new AddGuildDto("123456789", "Mon serveur"));

        result.IsSuccess.Should().BeTrue();
        result.Data!.GuildId.Should().Be("123456789");
        result.Data.Label.Should().Be("Mon serveur");
        db.AllowedGuilds.Should().HaveCount(1);
    }

    [Test]
    public async Task AddAllowedGuild_ReturnsFailure_WhenGuildAlreadyExists()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(AddAllowedGuild_ReturnsFailure_WhenGuildAlreadyExists));
        db.AllowedGuilds.Add(new AllowedGuild { GuildId = "123456789" });
        await db.SaveChangesAsync();

        var result = await CreateService(db).AddAllowedGuildAsync(new AddGuildDto("123456789", null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("guild_already_exists");
        db.AllowedGuilds.Should().HaveCount(1);
    }

    // -------------------------
    // DeleteAllowedGuildAsync
    // -------------------------

    [Test]
    public async Task DeleteAllowedGuild_ReturnsSuccess_WhenGuildExists()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteAllowedGuild_ReturnsSuccess_WhenGuildExists));
        db.AllowedGuilds.Add(new AllowedGuild { GuildId = "999" });
        await db.SaveChangesAsync();
        var id = db.AllowedGuilds.First().Id;

        var result = await CreateService(db).DeleteAllowedGuildAsync(id);

        result.IsSuccess.Should().BeTrue();
        db.AllowedGuilds.Should().BeEmpty();
    }

    [Test]
    public async Task DeleteAllowedGuild_ReturnsFailure_WhenGuildNotFound()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteAllowedGuild_ReturnsFailure_WhenGuildNotFound));

        var result = await CreateService(db).DeleteAllowedGuildAsync(999);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("not_found");
    }

    // -------------------------
    // GetAllowedEmailsAsync
    // -------------------------

    [Test]
    public async Task GetAllowedEmails_ReturnsEmpty_WhenNoEmailsExist()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAllowedEmails_ReturnsEmpty_WhenNoEmailsExist));

        var result = await CreateService(db).GetAllowedEmailsAsync();

        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllowedEmails_ReturnsEmails_OrderedByCreatedAt()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAllowedEmails_ReturnsEmails_OrderedByCreatedAt));
        db.AllowedEmails.AddRange(
            new AllowedEmail { Email = "alpha@gmail.com", CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new AllowedEmail { Email = "beta@gmail.com",  CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetAllowedEmailsAsync();

        result.Should().HaveCount(2);
        result[0].Email.Should().Be("alpha@gmail.com");
        result[1].Email.Should().Be("beta@gmail.com");
    }

    // -------------------------
    // AddAllowedEmailAsync
    // -------------------------

    [Test]
    public async Task AddAllowedEmail_ReturnsSuccess_WhenEmailIsNew()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(AddAllowedEmail_ReturnsSuccess_WhenEmailIsNew));

        var result = await CreateService(db).AddAllowedEmailAsync(new AddEmailDto("Thomas@Gmail.com"));

        result.IsSuccess.Should().BeTrue();
        db.AllowedEmails.Should().HaveCount(1);
    }

    [Test]
    public async Task AddAllowedEmail_NormalizesEmailToLowercase()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(AddAllowedEmail_NormalizesEmailToLowercase));

        await CreateService(db).AddAllowedEmailAsync(new AddEmailDto("Thomas@Gmail.com"));

        db.AllowedEmails.First().Email.Should().Be("thomas@gmail.com");
    }

    [Test]
    public async Task AddAllowedEmail_ReturnsFailure_WhenEmailAlreadyExists()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(AddAllowedEmail_ReturnsFailure_WhenEmailAlreadyExists));
        db.AllowedEmails.Add(new AllowedEmail { Email = "thomas@gmail.com" });
        await db.SaveChangesAsync();

        var result = await CreateService(db).AddAllowedEmailAsync(new AddEmailDto("thomas@gmail.com"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("email_already_exists");
        db.AllowedEmails.Should().HaveCount(1);
    }

    // -------------------------
    // DeleteAllowedEmailAsync
    // -------------------------

    [Test]
    public async Task DeleteAllowedEmail_ReturnsSuccess_WhenEmailExists()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteAllowedEmail_ReturnsSuccess_WhenEmailExists));
        db.AllowedEmails.Add(new AllowedEmail { Email = "delete@gmail.com" });
        await db.SaveChangesAsync();
        var id = db.AllowedEmails.First().Id;

        var result = await CreateService(db).DeleteAllowedEmailAsync(id);

        result.IsSuccess.Should().BeTrue();
        db.AllowedEmails.Should().BeEmpty();
    }

    [Test]
    public async Task DeleteAllowedEmail_ReturnsFailure_WhenEmailNotFound()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteAllowedEmail_ReturnsFailure_WhenEmailNotFound));

        var result = await CreateService(db).DeleteAllowedEmailAsync(999);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("not_found");
    }

    // -------------------------
    // GetAccessRequestsAsync
    // -------------------------

    [Test]
    public async Task GetAccessRequests_ReturnsAll_WhenNoStatusFilter()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessRequests_ReturnsAll_WhenNoStatusFilter));
        db.AccessRequests.AddRange(
            new AccessRequest { Email = "a@x.com", Status = AccessRequestStatus.Pending },
            new AccessRequest { Email = "b@x.com", Status = AccessRequestStatus.Approved },
            new AccessRequest { Email = "c@x.com", Status = AccessRequestStatus.Denied }
        );
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetAccessRequestsAsync(null);

        result.Should().HaveCount(3);
    }

    [Test]
    public async Task GetAccessRequests_ReturnsFiltered_WhenStatusProvided()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessRequests_ReturnsFiltered_WhenStatusProvided));
        db.AccessRequests.AddRange(
            new AccessRequest { Email = "a@x.com", Status = AccessRequestStatus.Pending },
            new AccessRequest { Email = "b@x.com", Status = AccessRequestStatus.Approved }
        );
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetAccessRequestsAsync("Pending");

        result.Should().HaveCount(1);
        result[0].Email.Should().Be("a@x.com");
    }

    [Test]
    public async Task GetAccessRequests_ReturnsDescendingOrder()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessRequests_ReturnsDescendingOrder));
        db.AccessRequests.AddRange(
            new AccessRequest { Email = "old@x.com", Status = AccessRequestStatus.Pending, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new AccessRequest { Email = "new@x.com", Status = AccessRequestStatus.Pending, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetAccessRequestsAsync(null);

        result[0].Email.Should().Be("new@x.com");
        result[1].Email.Should().Be("old@x.com");
    }

    // -------------------------
    // ResolveAccessRequestAsync
    // -------------------------

    [Test]
    public async Task ResolveAccessRequest_ReturnsFailure_WhenRequestNotFound()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(ResolveAccessRequest_ReturnsFailure_WhenRequestNotFound));

        var result = await CreateService(db).ResolveAccessRequestAsync(999, new ResolveAccessRequestDto(AccessRequestStatus.Approved));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("not_found");
    }

    [Test]
    public async Task ResolveAccessRequest_ReturnsFailure_WhenRequestAlreadyResolved()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(ResolveAccessRequest_ReturnsFailure_WhenRequestAlreadyResolved));
        db.AccessRequests.Add(new AccessRequest { Email = "done@x.com", Status = AccessRequestStatus.Approved });
        await db.SaveChangesAsync();
        var id = db.AccessRequests.First().Id;

        var result = await CreateService(db).ResolveAccessRequestAsync(id, new ResolveAccessRequestDto(AccessRequestStatus.Denied));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("request_already_resolved");
    }

    [Test]
    public async Task ResolveAccessRequest_ApprovesRequest_AndAddsEmailToWhitelist()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(ResolveAccessRequest_ApprovesRequest_AndAddsEmailToWhitelist));
        db.AccessRequests.Add(new AccessRequest { Email = "new@gmail.com", Status = AccessRequestStatus.Pending });
        await db.SaveChangesAsync();
        var id = db.AccessRequests.First().Id;

        var result = await CreateService(db).ResolveAccessRequestAsync(id, new ResolveAccessRequestDto(AccessRequestStatus.Approved));

        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(AccessRequestStatus.Approved);
        result.Data.ResolvedAt.Should().NotBeNull();
        db.AllowedEmails.Should().HaveCount(1);
        db.AllowedEmails.First().Email.Should().Be("new@gmail.com");
    }

    [Test]
    public async Task ResolveAccessRequest_DeniesRequest_WithoutAddingEmail()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(ResolveAccessRequest_DeniesRequest_WithoutAddingEmail));
        db.AccessRequests.Add(new AccessRequest { Email = "denied@gmail.com", Status = AccessRequestStatus.Pending });
        await db.SaveChangesAsync();
        var id = db.AccessRequests.First().Id;

        var result = await CreateService(db).ResolveAccessRequestAsync(id, new ResolveAccessRequestDto(AccessRequestStatus.Denied));

        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(AccessRequestStatus.Denied);
        db.AllowedEmails.Should().BeEmpty();
    }

    [Test]
    public async Task ResolveAccessRequest_DoesNotDuplicateEmail_WhenAlreadyInWhitelist()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(ResolveAccessRequest_DoesNotDuplicateEmail_WhenAlreadyInWhitelist));
        db.AllowedEmails.Add(new AllowedEmail { Email = "existing@gmail.com" });
        db.AccessRequests.Add(new AccessRequest { Email = "existing@gmail.com", Status = AccessRequestStatus.Pending });
        await db.SaveChangesAsync();
        var id = db.AccessRequests.First().Id;

        var result = await CreateService(db).ResolveAccessRequestAsync(id, new ResolveAccessRequestDto(AccessRequestStatus.Approved));

        result.IsSuccess.Should().BeTrue();
        db.AllowedEmails.Should().HaveCount(1);
    }

    // -------------------------
    // GetUsersAsync
    // -------------------------

    [Test]
    public async Task GetUsers_ReturnsEmpty_WhenNoUsersExist()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetUsers_ReturnsEmpty_WhenNoUsersExist));

        var result = await CreateService(db).GetUsersAsync(0, 30);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetUsers_ReturnsUsers_OrderedByCreatedAt()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetUsers_ReturnsUsers_OrderedByCreatedAt));
        db.Users.AddRange(
            new User { Name = "Alice", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new User { Name = "Bob",   CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetUsersAsync(0, 30);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Alice");
        result[1].Name.Should().Be("Bob");
    }

    [Test]
    public async Task GetUsers_AdminsFirst_ThenByCreatedAt()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetUsers_AdminsFirst_ThenByCreatedAt));
        db.Users.AddRange(
            new User { Name = "UserA",  Role = UserRole.User,  CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new User { Name = "AdminB", Role = UserRole.Admin, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new User { Name = "UserC",  Role = UserRole.User,  CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetUsersAsync(0, 30);

        result[0].Name.Should().Be("AdminB");
        result[1].Name.Should().Be("UserA");
        result[2].Name.Should().Be("UserC");
    }

    [Test]
    public async Task GetUsers_RespectsSkipAndTake()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetUsers_RespectsSkipAndTake));
        for (var i = 0; i < 10; i++)
            db.Users.Add(new User { Name = $"User{i}", Role = UserRole.User, CreatedAt = DateTime.UtcNow.AddMinutes(i) });
        await db.SaveChangesAsync();

        var page1 = await CreateService(db).GetUsersAsync(0, 4);
        var page2 = await CreateService(db).GetUsersAsync(4, 4);

        page1.Should().HaveCount(4);
        page2.Should().HaveCount(4);
        page1.Select(u => u.Id).Should().NotIntersectWith(page2.Select(u => u.Id));
    }

    // -------------------------
    // UpdateUserRoleAsync
    // -------------------------

    [Test]
    public async Task UpdateUserRole_ReturnsFailure_WhenUserNotFound()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserRole_ReturnsFailure_WhenUserNotFound));

        var result = await CreateService(db).UpdateUserRoleAsync("non-existent-id", new UpdateUserRoleDto(UserRole.Admin));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("not_found");
    }

    [Test]
    public async Task UpdateUserRole_ReturnsFailure_WhenDemotingLastAdmin()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserRole_ReturnsFailure_WhenDemotingLastAdmin));
        var admin = new User { Name = "Solo Admin", Role = UserRole.Admin };
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        var result = await CreateService(db).UpdateUserRoleAsync(admin.Id, new UpdateUserRoleDto(UserRole.User));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("last_admin");
        db.Users.First().Role.Should().Be(UserRole.Admin);
    }

    [Test]
    public async Task UpdateUserRole_ReturnsSuccess_WhenDemotingAdminIfOtherAdminExists()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserRole_ReturnsSuccess_WhenDemotingAdminIfOtherAdminExists));
        var admin1 = new User { Name = "Admin 1", Role = UserRole.Admin };
        var admin2 = new User { Name = "Admin 2", Role = UserRole.Admin };
        db.Users.AddRange(admin1, admin2);
        await db.SaveChangesAsync();

        var result = await CreateService(db).UpdateUserRoleAsync(admin1.Id, new UpdateUserRoleDto(UserRole.User));

        result.IsSuccess.Should().BeTrue();
        db.Users.First(u => u.Id == admin1.Id).Role.Should().Be(UserRole.User);
    }

    [Test]
    public async Task UpdateUserRole_ReturnsSuccess_AndUpdatesRole()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateUserRole_ReturnsSuccess_AndUpdatesRole));
        var user = new User { Name = "Thomas", Role = UserRole.User };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await CreateService(db).UpdateUserRoleAsync(user.Id, new UpdateUserRoleDto(UserRole.Admin));

        result.IsSuccess.Should().BeTrue();
        db.Users.First().Role.Should().Be(UserRole.Admin);
    }

    // -------------------------
    // DeleteUserAsync
    // -------------------------

    [Test]
    public async Task DeleteUser_ReturnsFailure_WhenUserNotFound()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUser_ReturnsFailure_WhenUserNotFound));

        var result = await CreateService(db).DeleteUserAsync("non-existent-id");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("not_found");
    }

    [Test]
    public async Task DeleteUser_ReturnsFailure_WhenDeletingLastAdmin()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUser_ReturnsFailure_WhenDeletingLastAdmin));
        var admin = new User { Name = "Solo Admin", Role = UserRole.Admin };
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        var result = await CreateService(db).DeleteUserAsync(admin.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("last_admin");
        db.Users.Should().HaveCount(1);
    }

    [Test]
    public async Task DeleteUser_ReturnsFailure_WhenDeletingSelf()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUser_ReturnsFailure_WhenDeletingSelf));
        var self  = new User { Name = "Me",    Role = UserRole.Admin };
        var other = new User { Name = "Other", Role = UserRole.Admin };
        db.Users.AddRange(self, other);
        await db.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = self.Id;
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor { HttpContext = httpContext }, NullNotificationDispatcher.Instance, FakeSettingsService.Instance, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.DeleteUserAsync(self.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("cannot_delete_self");
        db.Users.Should().HaveCount(2);
    }

    [Test]
    public async Task DeleteUser_ReturnsSuccess_WhenDeletingRegularUser()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUser_ReturnsSuccess_WhenDeletingRegularUser));
        var admin = new User { Name = "Admin",    Role = UserRole.Admin };
        var user  = new User { Name = "ToDelete", Role = UserRole.User };
        db.Users.AddRange(admin, user);
        await db.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = admin.Id;
        var refreshTokenService = new FakeRefreshTokenService();
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor { HttpContext = httpContext }, NullNotificationDispatcher.Instance, FakeSettingsService.Instance, refreshTokenService, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.DeleteUserAsync(user.Id);

        result.IsSuccess.Should().BeTrue();
        db.Users.Should().HaveCount(1);
        db.Users.First().Id.Should().Be(admin.Id);
    }

    [Test]
    public async Task DeleteUser_RevokesAllRefreshTokens_ForTheDeletedUser()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUser_RevokesAllRefreshTokens_ForTheDeletedUser));
        var admin = new User { Name = "Admin",    Role = UserRole.Admin };
        var user  = new User { Name = "ToDelete", Role = UserRole.User };
        db.Users.AddRange(admin, user);
        await db.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = admin.Id;
        var refreshTokenService = new FakeRefreshTokenService();
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor { HttpContext = httpContext }, NullNotificationDispatcher.Instance, FakeSettingsService.Instance, refreshTokenService, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.DeleteUserAsync(user.Id);

        result.IsSuccess.Should().BeTrue();
        refreshTokenService.RevokedAllForUserIds.Should().ContainSingle().Which.Should().Be(user.Id);
    }

    [Test]
    public async Task DeleteUser_ReturnsSuccess_WhenDeletingAdminIfOtherAdminExists()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUser_ReturnsSuccess_WhenDeletingAdminIfOtherAdminExists));
        var actor  = new User { Name = "Actor",  Role = UserRole.Admin };
        var target = new User { Name = "Target", Role = UserRole.Admin };
        db.Users.AddRange(actor, target);
        await db.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = actor.Id;
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor { HttpContext = httpContext }, NullNotificationDispatcher.Instance, FakeSettingsService.Instance, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.DeleteUserAsync(target.Id);

        result.IsSuccess.Should().BeTrue();
        db.Users.Should().HaveCount(1);
        db.Users.First().Id.Should().Be(actor.Id);
    }

    [Test]
    public async Task DeleteUser_LogsWarning_OnSuccess()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUser_LogsWarning_OnSuccess));
        var admin = new User { Name = "Admin",  Role = UserRole.Admin };
        var user  = new User { Name = "Victim", Role = UserRole.User };
        db.Users.AddRange(admin, user);
        await db.SaveChangesAsync();

        var logSpy = new LogSpy();
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = admin.Id;
        var service = new AdminService(db, logSpy, new HttpContextAccessor { HttpContext = httpContext }, NullNotificationDispatcher.Instance, FakeSettingsService.Instance, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

        await service.DeleteUserAsync(user.Id);

        logSpy.Warnings.Should().ContainSingle()
            .Which.Should().Contain(user.Id);
    }

    // -------------------------
    // GetAccessModeAsync
    // -------------------------

    [Test]
    public async Task GetAccessMode_IsDiscordOpenMode_WhenDiscordRestrictionDisabled()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessMode_IsDiscordOpenMode_WhenDiscordRestrictionDisabled));
        var settings = new FakeSettingsService { DiscordRestricted = false };
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor(), NullNotificationDispatcher.Instance, settings, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.GetAccessModeAsync();

        result.IsDiscordOpenMode.Should().BeTrue();
        result.IsDiscordRestrictionLocksEveryone.Should().BeFalse();
    }

    [Test]
    public async Task GetAccessMode_IsDiscordRestrictionLocksEveryone_WhenRestrictedButNoGuildConfigured()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessMode_IsDiscordRestrictionLocksEveryone_WhenRestrictedButNoGuildConfigured));
        var settings = new FakeSettingsService { DiscordRestricted = true };
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor(), NullNotificationDispatcher.Instance, settings, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.GetAccessModeAsync();

        result.IsDiscordOpenMode.Should().BeFalse();
        result.IsDiscordRestrictionLocksEveryone.Should().BeTrue();
    }

    [Test]
    public async Task GetAccessMode_DiscordFlagsBothFalse_WhenRestrictedWithGuildConfigured()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessMode_DiscordFlagsBothFalse_WhenRestrictedWithGuildConfigured));
        db.AllowedGuilds.Add(new AllowedGuild { GuildId = "123" });
        await db.SaveChangesAsync();
        var settings = new FakeSettingsService { DiscordRestricted = true };
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor(), NullNotificationDispatcher.Instance, settings, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.GetAccessModeAsync();

        result.IsDiscordOpenMode.Should().BeFalse();
        result.IsDiscordRestrictionLocksEveryone.Should().BeFalse();
    }

    [Test]
    public async Task GetAccessMode_IsGoogleOpenMode_WhenGoogleRestrictionDisabled()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessMode_IsGoogleOpenMode_WhenGoogleRestrictionDisabled));
        var settings = new FakeSettingsService { GoogleRestricted = false };
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor(), NullNotificationDispatcher.Instance, settings, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.GetAccessModeAsync();

        result.IsGoogleOpenMode.Should().BeTrue();
        result.IsGoogleRestrictionLocksEveryone.Should().BeFalse();
    }

    [Test]
    public async Task GetAccessMode_IsGoogleRestrictionLocksEveryone_WhenRestrictedButNoEmailConfigured()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessMode_IsGoogleRestrictionLocksEveryone_WhenRestrictedButNoEmailConfigured));
        var settings = new FakeSettingsService { GoogleRestricted = true };
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor(), NullNotificationDispatcher.Instance, settings, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.GetAccessModeAsync();

        result.IsGoogleOpenMode.Should().BeFalse();
        result.IsGoogleRestrictionLocksEveryone.Should().BeTrue();
    }

    [Test]
    public async Task GetAccessMode_GoogleFlagsBothFalse_WhenRestrictedWithEmailConfigured()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessMode_GoogleFlagsBothFalse_WhenRestrictedWithEmailConfigured));
        db.AllowedEmails.Add(new AllowedEmail { Email = "thomas@gmail.com" });
        await db.SaveChangesAsync();
        var settings = new FakeSettingsService { GoogleRestricted = true };
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor(), NullNotificationDispatcher.Instance, settings, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.GetAccessModeAsync();

        result.IsGoogleOpenMode.Should().BeFalse();
        result.IsGoogleRestrictionLocksEveryone.Should().BeFalse();
    }

    [Test]
    public async Task GetAccessMode_NoLoginMethodAvailable_WhenBothRestrictedWithEmptyAllowlists()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessMode_NoLoginMethodAvailable_WhenBothRestrictedWithEmptyAllowlists));
        var settings = new FakeSettingsService { DiscordRestricted = true, GoogleRestricted = true };
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor(), NullNotificationDispatcher.Instance, settings, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.GetAccessModeAsync();

        result.NoLoginMethodAvailable.Should().BeTrue();
    }

    [Test]
    public async Task GetAccessMode_NoLoginMethodAvailable_IsFalse_WhenOnlyOneProviderIsUnusable()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(GetAccessMode_NoLoginMethodAvailable_IsFalse_WhenOnlyOneProviderIsUnusable));
        db.AllowedEmails.Add(new AllowedEmail { Email = "thomas@gmail.com" });
        await db.SaveChangesAsync();
        var settings = new FakeSettingsService { DiscordRestricted = true, GoogleRestricted = true };
        var service = new AdminService(db, NullAppLogService.Instance, new HttpContextAccessor(), NullNotificationDispatcher.Instance, settings, FakeRefreshTokenService.Instance, Options.Create(new AppOptions { Url = "https://localhost" }));

        var result = await service.GetAccessModeAsync();

        result.IsDiscordRestrictionLocksEveryone.Should().BeTrue();
        result.IsGoogleRestrictionLocksEveryone.Should().BeFalse();
        result.NoLoginMethodAvailable.Should().BeFalse();
    }
}
