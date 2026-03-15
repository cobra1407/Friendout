using System.Threading.Tasks;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;
using Friendout.Infrastructure.Services;
using NUnit.Framework;
using FluentAssertions;

namespace Friendout.Test;

public class UserServiceTests
{
    [Test]
    public async Task GetUserByProviderAccountIdAsync_WhenAccountExists_ReturnsUser()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(GetUserByProviderAccountIdAsync_WhenAccountExists_ReturnsUser));

        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var account = new Account
        {
            UserId = user.Id,
            Provider = ProviderEnum.Discord.GetEnumMemberValue(),
            ProviderAccountId = "provider-123"
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = new UserService(context);

        // Act
        var result = await service.GetUserByProviderAccountIdAsync(ProviderEnum.Discord, "provider-123");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(user.Id);
    }

    [Test]
    public async Task GetUserByProviderAccountIdAsync_WhenAccountDoesNotExist_ReturnsFailure()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(GetUserByProviderAccountIdAsync_WhenAccountDoesNotExist_ReturnsFailure));
        var service = new UserService(context);

        // Act
        var result = await service.GetUserByProviderAccountIdAsync(ProviderEnum.Discord, "unknown");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("User not found for this provider.");
    }

    [Test]
    public async Task CreateUserFromOAuthAsync_FirstUserGetsAdminRole_AndAccountLinked()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CreateUserFromOAuthAsync_FirstUserGetsAdminRole_AndAccountLinked));
        var service = new UserService(context);

        // Act
        var result = await service.CreateUserFromOAuthAsync(
            ProviderEnum.Discord,
            "provider-1",
            "First User",
            "first@example.com",
            null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var createdUser = result.Data!;
        createdUser.Role.Should().Be(UserRole.Admin);

        var account = await context.Accounts.FindAsync(context.Accounts.First().Id);
        account.Should().NotBeNull();
        account!.UserId.Should().Be(createdUser.Id);
    }

    [Test]
    public async Task CreateUserFromOAuthAsync_ExistingAccountReturnsSameUser()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CreateUserFromOAuthAsync_ExistingAccountReturnsSameUser));
        var service = new UserService(context);

        var firstResult = await service.CreateUserFromOAuthAsync(
            ProviderEnum.Discord,
            "provider-1",
            "First User",
            "first@example.com",
            null);

        // Act
        var secondResult = await service.CreateUserFromOAuthAsync(
            ProviderEnum.Discord,
            "provider-1",
            "First User Again",
            "first@example.com",
            null);

        // Assert
        secondResult.IsSuccess.Should().BeTrue();
        secondResult.Data!.Id.Should().Be(firstResult.Data!.Id);
        context.Users.Count().Should().Be(1);
    }
}

