using FluentAssertions;
using Friendout.Domain.DTOs.Admin;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Services;

namespace Friendout.Test;

public class AdminServiceTests
{
    // -------------------------
    // Helper
    // -------------------------

    private static AdminService CreateService(Friendout.Domain.Context.FriendoutDbContext db)
        => new(db, TestLogger<AdminService>.Instance);

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

        var result = await CreateService(db).GetUsersAsync();

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

        var result = await CreateService(db).GetUsersAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Alice");
        result[1].Name.Should().Be("Bob");
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
}
