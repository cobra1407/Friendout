using FluentAssertions;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Command.Participant;
using Friendout.Infrastructure.Services;

namespace Friendout.Test;

public class ParticipantServiceTests
{
    [Test]
    public async Task GetActivityParticipantsAsync_WhenActivityMissing_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(GetActivityParticipantsAsync_WhenActivityMissing_ReturnsFailure));
        var service = new ParticipantService(context, TestLogger<ParticipantService>.Instance, new NoopActivitiesHubNotifier());

        var result = await service.GetActivityParticipantsAsync("unknown");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Cannot find this activity");
    }

    [Test]
    public async Task SaveParticipationAsync_WithoutSubActivities_UpsertsMainParticipation()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(SaveParticipationAsync_WithoutSubActivities_UpsertsMainParticipation));

        var user = new User { Id = "user-1", Name = "Alice", Email = "alice@example.com" };
        var activity = CreateActivity("activity-1", user);

        context.Users.Add(user);
        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var service = new ParticipantService(context, TestLogger<ParticipantService>.Instance, new NoopActivitiesHubNotifier());

        var createResult = await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.Participating,
            SubActivityIds = new List<string>()
        }, user.Id);

        createResult.IsSuccess.Should().BeTrue();
        createResult.Data.UserMainParticipation.Should().NotBeNull();
        createResult.Data.UserMainParticipation!.Status.Should().Be(ParticipationStatus.Participating);

        var updateResult = await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.Maybe,
            SubActivityIds = new List<string>()
        }, user.Id);

        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Data.UserMainParticipation!.Status.Should().Be(ParticipationStatus.Maybe);
        context.UserParticipation.Count().Should().Be(1);
    }

    [Test]
    public async Task SaveParticipationAsync_WithSubActivities_CreatesSubParticipations()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(SaveParticipationAsync_WithSubActivities_CreatesSubParticipations));

        var user = new User { Id = "user-1", Name = "Alice", Email = "alice@example.com" };
        var activity = CreateActivity("activity-1", user);
        var location = new Localisation { Id = "loc-sub", Type = Friendout.Domain.Enums.LocalisationType.Address, DisplayName = "Sub" };

        var sub1 = new SubActivity
        {
            Id = "sub-1",
            ActivityId = activity.Id,
            Activity = activity,
            Name = "Sub 1",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(30),
            Localisation = location,
            LocalisationId = location.Id
        };

        var sub2 = new SubActivity
        {
            Id = "sub-2",
            ActivityId = activity.Id,
            Activity = activity,
            Name = "Sub 2",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(45),
            Localisation = location,
            LocalisationId = location.Id
        };

        context.Users.Add(user);
        context.Activities.Add(activity);
        context.SubActivities.AddRange(sub1, sub2);
        await context.SaveChangesAsync();

        var service = new ParticipantService(context, TestLogger<ParticipantService>.Instance, new NoopActivitiesHubNotifier());

        var result = await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.Participating,
            SubActivityIds = new List<string> { sub1.Id, sub2.Id, "missing" }
        }, user.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data.UserSubActivitiesParticipations.Should().HaveCount(2);
        result.Data.SubActivitiesParticipants.Should().HaveCount(2);
    }

    [Test]
    public async Task SaveParticipationAsync_WhenActivityIsInPast_RejectsCreateAndUpdate()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(SaveParticipationAsync_WhenActivityIsInPast_RejectsCreateAndUpdate));

        var user = new User { Id = "user-1", Name = "Alice", Email = "alice@example.com" };
        var activity = CreateActivity("activity-past", user);
        activity.StartAt = DateTime.UtcNow.AddHours(-2);
        activity.EndAt = DateTime.UtcNow.AddHours(-1);

        context.Users.Add(user);
        context.Activities.Add(activity);
        context.UserParticipation.Add(new UserParticipation
        {
            Id = "existing-main",
            UserId = user.Id,
            ActivityId = activity.Id,
            SubActivityId = null,
            Status = ParticipationStatus.Participating,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var service = new ParticipantService(context, TestLogger<ParticipantService>.Instance, new NoopActivitiesHubNotifier());

        var createResult = await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.Maybe,
            SubActivityIds = new List<string> { "sub-does-not-matter" }
        }, "user-2");

        createResult.IsSuccess.Should().BeFalse();
        createResult.ErrorMessage.Should().Be("This activity is already started");

        var updateResult = await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.NotParticipating,
            SubActivityIds = new List<string>()
        }, user.Id);

        updateResult.IsSuccess.Should().BeFalse();
        updateResult.ErrorMessage.Should().Be("This activity is already started");

        var persisted = context.UserParticipation.Single(p => p.Id == "existing-main");
        persisted.Status.Should().Be(ParticipationStatus.Participating);
        context.UserParticipation.Count().Should().Be(1);
    }

    [Test]
    public async Task SaveParticipationAsync_WhenSameUserSentMultipleTimes_KeepsSingleParticipationPerActivityAndSubActivity()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(SaveParticipationAsync_WhenSameUserSentMultipleTimes_KeepsSingleParticipationPerActivityAndSubActivity));

        var user = new User { Id = "user-1", Name = "Alice", Email = "alice@example.com" };
        var activity = CreateActivity("activity-unique", user);
        var location = new Localisation { Id = "loc-unique", Type = Friendout.Domain.Enums.LocalisationType.Address, DisplayName = "Sub" };
        var sub1 = new SubActivity
        {
            Id = "sub-1",
            ActivityId = activity.Id,
            Activity = activity,
            Name = "Sub 1",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(30),
            Localisation = location,
            LocalisationId = location.Id
        };

        context.Users.Add(user);
        context.Activities.Add(activity);
        context.SubActivities.Add(sub1);
        await context.SaveChangesAsync();

        var service = new ParticipantService(context, TestLogger<ParticipantService>.Instance, new NoopActivitiesHubNotifier());

        var mainFirst = await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.Participating,
            SubActivityIds = new List<string>()
        }, user.Id);

        var mainSecond = await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.Maybe,
            SubActivityIds = new List<string>()
        }, user.Id);

        var subDuplicated = await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.Participating,
            SubActivityIds = new List<string> { sub1.Id, sub1.Id, sub1.Id }
        }, user.Id);

        var subSecond = await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.NotParticipating,
            SubActivityIds = new List<string> { sub1.Id }
        }, user.Id);

        mainFirst.IsSuccess.Should().BeTrue();
        mainSecond.IsSuccess.Should().BeTrue();
        subDuplicated.IsSuccess.Should().BeTrue();
        subSecond.IsSuccess.Should().BeTrue();

        context.UserParticipation.Count(p =>
            p.UserId == user.Id &&
            p.ActivityId == activity.Id &&
            p.SubActivityId == null).Should().Be(1);

        context.UserParticipation.Count(p =>
            p.UserId == user.Id &&
            p.ActivityId == activity.Id &&
            p.SubActivityId == sub1.Id).Should().Be(1);
    }

    [Test]
    public async Task SaveParticipationAsync_CannotParticipateTwiceInMainActivity_ButCanInSubActivity()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(SaveParticipationAsync_CannotParticipateTwiceInMainActivity_ButCanInSubActivity));

        var user = new User { Id = "user-1", Name = "Bob", Email = "bob@example.com" };
        var activity = CreateActivity("activity-unique-test", user);

        var subActivity = new SubActivity
        {
            Id = "sub-1",
            ActivityId = activity.Id,
            Activity = activity,
            Name = "Sous-Activité 1",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            Localisation = new Localisation { Id = "loc-sub-1", Type = Friendout.Domain.Enums.LocalisationType.Address, DisplayName = "Sub" }
        };

        context.Users.Add(user);
        context.Activities.Add(activity);
        context.SubActivities.Add(subActivity);
        await context.SaveChangesAsync();

        var service = new ParticipantService(context, TestLogger<ParticipantService>.Instance, new NoopActivitiesHubNotifier());

        await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.Participating,
            SubActivityIds = new List<string>()
        }, user.Id);

        await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.Participating,
            SubActivityIds = new List<string>()
        }, user.Id);

        await service.SaveParticipationAsync(new UpdateParticipationCommand
        {
            ActivityId = activity.Id,
            Status = ParticipationStatus.Participating,
            SubActivityIds = new List<string> { subActivity.Id }
        }, user.Id);

        var allParticipations = context.UserParticipation.Where(p => p.UserId == user.Id).ToList();

        allParticipations.Should().HaveCount(2);

        allParticipations.Should().ContainSingle(p => p.ActivityId == activity.Id && p.SubActivityId == null,
            "User should have only one main participation for the activity");

        allParticipations.Should().ContainSingle(p => p.ActivityId == activity.Id && p.SubActivityId == subActivity.Id,
            "User should have only one participation for the sub-activity");
    }

    private static Activity CreateActivity(string activityId, User creator)
    {
        var startAt = DateTime.UtcNow.AddHours(1);
        return new Activity
        {
            Id = activityId,
            Title = "Activity",
            Description = "Desc",
            StartAt = startAt,
            EndAt = startAt.AddHours(1),
            CreatedBy = creator.Id,
            Creator = creator,
            Localisation = new Localisation { Id = $"loc-{activityId}", Type = Friendout.Domain.Enums.LocalisationType.Address, DisplayName = "Paris" }
        };
    }
}
