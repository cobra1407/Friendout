using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Friendout.Domain.Context;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Friendout.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Friendout.Test;

public class AccountDeletionServiceTests
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

    private sealed class NullNotificationDispatcher : INotificationDispatcher
    {
        public static readonly NullNotificationDispatcher Instance = new();
        public Task DispatchNotificationAsync(Guid userId, NotificationType type, System.Collections.Generic.Dictionary<string, string> data)
            => Task.CompletedTask;
    }

    private static IAccountDeletionService CreateService(FriendoutDbContext db)
        => new AccountDeletionService(
            db,
            NullNotificationDispatcher.Instance,
            NullAppLogService.Instance,
            Options.Create(new AppOptions { Url = "https://localhost" }),
            new ConfigurationBuilder().Build());

    /// <summary>Minimal valid Activity for tests — Localisation is required by the model.</summary>
    private static Activity CreateActivity(string createdBy) => new()
    {
        Title = "Test activity",
        Description = "Test description",
        StartAt = DateTime.UtcNow.AddDays(7),
        CreatedBy = createdBy,
        Localisation = new Localisation { Type = LocalisationType.Address, Address = "Somewhere" }
    };

    // -------------------------
    // DeleteUserDataAsync
    // -------------------------

    [Test]
    public async Task DeleteUserData_ReturnsFailure_WhenUserNotFound()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUserData_ReturnsFailure_WhenUserNotFound));

        var result = await CreateService(db).DeleteUserDataAsync("non-existent-id", deleteCreatedActivities: false);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("not_found");
    }

    [Test]
    public async Task DeleteUserData_RemovesTheUserRow_WhenNoCreatedActivities()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUserData_RemovesTheUserRow_WhenNoCreatedActivities));
        var user = new User { Name = "Solo", Email = "solo@example.com" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await CreateService(db).DeleteUserDataAsync(user.Id, deleteCreatedActivities: false);

        result.IsSuccess.Should().BeTrue();
        db.Users.Should().BeEmpty();
    }

    [Test]
    public async Task DeleteUserData_RemovesPersonalData_ForTheDeletedUser()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUserData_RemovesPersonalData_ForTheDeletedUser));
        var user = new User { Name = "Solo", Email = "solo@example.com" };
        db.Users.Add(user);
        db.RefreshTokens.Add(new RefreshToken { Token = "tok-1", UserId = user.Id, ExpiresAt = DateTime.UtcNow.AddDays(30) });
        db.Accounts.Add(new Account { UserId = user.Id, Provider = ProviderEnum.Discord, ProviderAccountId = "discord-123" });
        await db.SaveChangesAsync();

        var result = await CreateService(db).DeleteUserDataAsync(user.Id, deleteCreatedActivities: false);

        result.IsSuccess.Should().BeTrue();
        db.RefreshTokens.Should().BeEmpty();
        db.Accounts.Should().BeEmpty();
    }

    [Test]
    public async Task DeleteUserData_DeletesActivity_WhenUserWasTheOnlyOneInvolved()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUserData_DeletesActivity_WhenUserWasTheOnlyOneInvolved));
        var user = new User { Name = "Solo Organizer" };
        db.Users.Add(user);
        var activity = CreateActivity(user.Id);
        db.Activities.Add(activity);
        await db.SaveChangesAsync();

        var result = await CreateService(db).DeleteUserDataAsync(user.Id, deleteCreatedActivities: false);

        result.IsSuccess.Should().BeTrue();
        db.Activities.Should().BeEmpty("nothing else referenced this activity, so it should be deleted outright");
    }

    [Test]
    public async Task DeleteUserData_OrphansActivity_WhenOtherParticipantsExist_AndChoiceIsFalse()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUserData_OrphansActivity_WhenOtherParticipantsExist_AndChoiceIsFalse));
        var organizer   = new User { Name = "Organizer" };
        var participant = new User { Name = "Participant" };
        db.Users.AddRange(organizer, participant);
        var activity = CreateActivity(organizer.Id);
        db.Activities.Add(activity);
        db.UserParticipation.Add(new UserParticipation { ActivityId = activity.Id, UserId = participant.Id, Status = ParticipationStatus.Participating });
        await db.SaveChangesAsync();

        var result = await CreateService(db).DeleteUserDataAsync(organizer.Id, deleteCreatedActivities: false);

        result.IsSuccess.Should().BeTrue();
        db.Users.Should().ContainSingle().Which.Id.Should().Be(participant.Id);
        var remaining = db.Activities.Should().ContainSingle().Which;
        remaining.CreatedBy.Should().BeNull("the activity should be orphaned, not deleted, since another participant depends on it");
    }

    [Test]
    public async Task DeleteUserData_DeletesActivity_WhenOtherParticipantsExist_ButChoiceIsTrue()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUserData_DeletesActivity_WhenOtherParticipantsExist_ButChoiceIsTrue));
        var organizer   = new User { Name = "Organizer" };
        var participant = new User { Name = "Participant" };
        db.Users.AddRange(organizer, participant);
        var activity = CreateActivity(organizer.Id);
        db.Activities.Add(activity);
        db.UserParticipation.Add(new UserParticipation { ActivityId = activity.Id, UserId = participant.Id, Status = ParticipationStatus.Participating });
        await db.SaveChangesAsync();

        var result = await CreateService(db).DeleteUserDataAsync(organizer.Id, deleteCreatedActivities: true);

        result.IsSuccess.Should().BeTrue();
        db.Activities.Should().BeEmpty("the user explicitly chose to delete their created activities, even shared ones");
    }

    [Test]
    public async Task DeleteUserData_RemovesTheUsersOwnParticipation_InSomeoneElsesActivity()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUserData_RemovesTheUsersOwnParticipation_InSomeoneElsesActivity));
        var organizer = new User { Name = "Organizer" };
        var attendee  = new User { Name = "Attendee" };
        db.Users.AddRange(organizer, attendee);
        var activity = CreateActivity(organizer.Id);
        db.Activities.Add(activity);
        db.UserParticipation.Add(new UserParticipation { ActivityId = activity.Id, UserId = attendee.Id, Status = ParticipationStatus.Participating });
        await db.SaveChangesAsync();

        var result = await CreateService(db).DeleteUserDataAsync(attendee.Id, deleteCreatedActivities: false);

        result.IsSuccess.Should().BeTrue();
        db.UserParticipation.Should().BeEmpty();
        db.Activities.Should().ContainSingle("the organizer's activity is untouched by the attendee leaving");
    }

    [Test]
    public async Task DeleteUserData_RemovesComments_AuthoredByTheDeletedUser()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteUserData_RemovesComments_AuthoredByTheDeletedUser));
        var organizer = new User { Name = "Organizer" };
        var commenter = new User { Name = "Commenter" };
        db.Users.AddRange(organizer, commenter);
        var activity = CreateActivity(organizer.Id);
        db.Activities.Add(activity);
        db.Comments.Add(new ActivityComment { ActivityId = activity.Id, UserId = commenter.Id, Content = "Nice!" });
        await db.SaveChangesAsync();

        var result = await CreateService(db).DeleteUserDataAsync(commenter.Id, deleteCreatedActivities: false);

        result.IsSuccess.Should().BeTrue();
        db.Comments.Should().BeEmpty();
    }

    // -------------------------
    // RequestDeletionAsync
    // -------------------------

    [Test]
    public async Task RequestDeletion_ReturnsSuccess_WithoutCreatingARequest_WhenEmailNotFound()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(RequestDeletion_ReturnsSuccess_WithoutCreatingARequest_WhenEmailNotFound));

        var result = await CreateService(db).RequestDeletionAsync("nobody@example.com", deleteCreatedActivities: false);

        result.IsSuccess.Should().BeTrue();
        db.AccountDeletionRequests.Should().BeEmpty();
    }

    [Test]
    public async Task RequestDeletion_CreatesARequest_StoringTheUsersChoice()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(RequestDeletion_CreatesARequest_StoringTheUsersChoice));
        var user = new User { Name = "Thomas", Email = "thomas@example.com" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await CreateService(db).RequestDeletionAsync("thomas@example.com", deleteCreatedActivities: true);

        result.IsSuccess.Should().BeTrue();
        var request = db.AccountDeletionRequests.Should().ContainSingle().Which;
        request.UserId.Should().Be(user.Id);
        request.DeleteCreatedActivities.Should().BeTrue();
        request.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    // -------------------------
    // ConfirmDeletionAsync
    // -------------------------

    [Test]
    public async Task ConfirmDeletion_ReturnsFailure_WhenTokenDoesNotExist()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(ConfirmDeletion_ReturnsFailure_WhenTokenDoesNotExist));

        var result = await CreateService(db).ConfirmDeletionAsync("unknown-token");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("invalid_or_expired_token");
    }

    [Test]
    public async Task ConfirmDeletion_ReturnsFailure_WhenTokenIsExpired()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(ConfirmDeletion_ReturnsFailure_WhenTokenIsExpired));
        var user = new User { Name = "Thomas", Email = "thomas@example.com" };
        db.Users.Add(user);
        db.AccountDeletionRequests.Add(new AccountDeletionRequest
        {
            Token = "expired-token",
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        });
        await db.SaveChangesAsync();

        var result = await CreateService(db).ConfirmDeletionAsync("expired-token");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("invalid_or_expired_token");
        db.Users.Should().ContainSingle("an expired token must not delete the account");
    }

    [Test]
    public async Task ConfirmDeletion_DeletesTheUser_UsingTheChoiceStoredAtRequestTime()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext(nameof(ConfirmDeletion_DeletesTheUser_UsingTheChoiceStoredAtRequestTime));
        var organizer   = new User { Name = "Organizer", Email = "organizer@example.com" };
        var participant = new User { Name = "Participant" };
        db.Users.AddRange(organizer, participant);
        var activity = CreateActivity(organizer.Id);
        db.Activities.Add(activity);
        db.UserParticipation.Add(new UserParticipation { ActivityId = activity.Id, UserId = participant.Id, Status = ParticipationStatus.Participating });
        db.AccountDeletionRequests.Add(new AccountDeletionRequest
        {
            Token = "valid-token",
            UserId = organizer.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            DeleteCreatedActivities = true
        });
        await db.SaveChangesAsync();

        var result = await CreateService(db).ConfirmDeletionAsync("valid-token");

        result.IsSuccess.Should().BeTrue();
        db.Users.Should().ContainSingle().Which.Id.Should().Be(participant.Id);
        db.Activities.Should().BeEmpty("the stored choice was to delete created activities, even shared ones");
    }
}
