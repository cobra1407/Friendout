using FluentAssertions;
using Friendout.Domain.DTOs.Activity;
using Friendout.Domain.DTOs.SubActivity;
using Friendout.Domain.Enums;
using Friendout.Domain.Enums.FilterEnums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Friendout.Test;

public class ActivityServiceTests
{
    [Test]
    public async Task GetActivitiesAsync_WhenUpcomingFilter_ReturnsOnlyUpcomingActivities()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(GetActivitiesAsync_WhenUpcomingFilter_ReturnsOnlyUpcomingActivities));

        var user = new User { Id = "user-filter-1", Name = "Filter User", Email = "filter1@example.com" };
        var localisation = new Localisation { Id = "loc-filter-1", Type = LocalisationType.Address, DisplayName = "Paris" };

        context.Users.Add(user);
        context.Activities.AddRange(
            new Activity
            {
                Id = "act-upcoming", Title = "Upcoming", Description = "Future activity",
                StartAt = DateTime.UtcNow.AddDays(1), EndAt = DateTime.UtcNow.AddDays(1).AddHours(1),
                CreatedBy = user.Id, Creator = user, Localisation = localisation
            },
            new Activity
            {
                Id = "act-past", Title = "Past", Description = "Past activity",
                StartAt = DateTime.UtcNow.AddDays(-1), EndAt = DateTime.UtcNow.AddDays(-1).AddHours(1),
                CreatedBy = user.Id, Creator = user, Localisation = localisation
            });

        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetActivitiesAsync(user.Id, new ActivityFilterDto { TimeFilter = ActivityTimeFilter.Upcoming, Skip = 0, Take = 20 });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle(a => a.Id == "act-upcoming");
    }

    [Test]
    public async Task GetActivitiesAsync_WhenPastFilter_ReturnsOnlyPastActivities()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(GetActivitiesAsync_WhenPastFilter_ReturnsOnlyPastActivities));

        var user = new User { Id = "user-filter-2", Name = "Filter User 2", Email = "filter2@example.com" };
        var localisation = new Localisation { Id = "loc-filter-2", Type = LocalisationType.Address, DisplayName = "Lyon" };

        context.Users.Add(user);
        context.Activities.AddRange(
            new Activity
            {
                Id = "act-upcoming-2", Title = "Upcoming 2", Description = "Future activity",
                StartAt = DateTime.UtcNow.AddDays(2), EndAt = DateTime.UtcNow.AddDays(2).AddHours(1),
                CreatedBy = user.Id, Creator = user, Localisation = localisation
            },
            new Activity
            {
                Id = "act-past-2", Title = "Past 2", Description = "Past activity",
                StartAt = DateTime.UtcNow.AddDays(-2), EndAt = DateTime.UtcNow.AddDays(-2).AddHours(1),
                CreatedBy = user.Id, Creator = user, Localisation = localisation
            });

        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetActivitiesAsync(user.Id, new ActivityFilterDto { TimeFilter = ActivityTimeFilter.Past, Skip = 0, Take = 20 });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle(a => a.Id == "act-past-2");
    }

    [Test]
    public async Task GetActivitiesAsync_WhenOnlyOwnActivity_ExcludesOtherCreators()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(GetActivitiesAsync_WhenOnlyOwnActivity_ExcludesOtherCreators));

        var owner = new User { Id = "user-owner", Name = "Owner", Email = "owner@example.com" };
        var other = new User { Id = "user-other", Name = "Other", Email = "other@example.com" };
        var localisation = new Localisation { Id = "loc-filter-3", Type = LocalisationType.Address, DisplayName = "Marseille" };

        context.Users.AddRange(owner, other);
        context.Activities.AddRange(
            new Activity
            {
                Id = "act-owner", Title = "Owner activity", Description = "Owned by owner",
                StartAt = DateTime.UtcNow.AddHours(3), EndAt = DateTime.UtcNow.AddHours(4),
                CreatedBy = owner.Id, Creator = owner, Localisation = localisation
            },
            new Activity
            {
                Id = "act-other", Title = "Other activity", Description = "Owned by other",
                StartAt = DateTime.UtcNow.AddHours(5), EndAt = DateTime.UtcNow.AddHours(6),
                CreatedBy = other.Id, Creator = other, Localisation = localisation
            });

        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetActivitiesAsync(owner.Id, new ActivityFilterDto { TimeFilter = ActivityTimeFilter.All, OnlyOwnActivity = true, Skip = 0, Take = 20 });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle(a => a.Id == "act-owner");
    }

    [Test]
    public async Task CreateActivityAsync_WhenStartAtDefault_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CreateActivityAsync_WhenStartAtDefault_ReturnsFailure));
        var service = CreateService(context);

        var result = await service.CreateActivityAsync(new CreateActivityDto
        {
            Title = "A", Description = "Desc", Time = "10:00",
            StartAt = default, EndAt = DateTime.UtcNow.AddHours(1)
        }, "user-1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("La date de debut est invalide.");
    }

    [Test]
    public async Task CreateActivityAsync_WhenEndBeforeStart_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CreateActivityAsync_WhenEndBeforeStart_ReturnsFailure));
        var service = CreateService(context);

        var start = DateTime.UtcNow.AddHours(2);
        var result = await service.CreateActivityAsync(new CreateActivityDto
        {
            Title = "A", Description = "Desc", Time = "10:00",
            StartAt = start, EndAt = start.AddHours(-1)
        }, "user-1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("La date de fin doit etre superieure ou egale a la date de debut.");
    }

    [Test]
    public async Task CreateActivityAsync_WhenSubActivityPriceNegative_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CreateActivityAsync_WhenSubActivityPriceNegative_ReturnsFailure));
        var service = CreateService(context);

        var start = DateTime.UtcNow.AddHours(1);
        var result = await service.CreateActivityAsync(new CreateActivityDto
        {
            Title = "A", Description = "Desc", Time = "10:00",
            StartAt = start, EndAt = start.AddHours(2),
            SubActivities = new List<CreateSubActivityDto>
            {
                new() { Name = "Sub", StartTime = start, EndTime = start.AddMinutes(30), Price = -1 }
            }
        }, "user-1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Le prix d'une sous-activite ne peut pas etre negatif.");
    }

    [Test]
    public async Task CreateActivityAsync_WithValidData_CreatesActivity()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CreateActivityAsync_WithValidData_CreatesActivity));

        var user = new User { Id = "user-1", Name = "Alice", Email = "alice@example.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var start = DateTime.UtcNow.AddHours(2);

        var result = await service.CreateActivityAsync(new CreateActivityDto
        {
            Title = "Randonnée", Description = "Sortie nature", Time = "14:00",
            StartAt = start, EndAt = start.AddHours(3), Address = "10 rue de Paris",
            RequiredEquipmentNames = new List<string> { "Chaussures", "chaussures", "Eau" },
            SubActivities = new List<CreateSubActivityDto>
            {
                new() { Name = "Pause", StartTime = start.AddMinutes(30), EndTime = start.AddMinutes(45), Description = "Pause rapide", Price = 5 }
            }
        }, user.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data.Title.Should().Be("Randonnée");
        context.Activities.Should().ContainSingle();
        context.SubActivities.Should().ContainSingle();
        context.Equipment.Count().Should().Be(2);
        context.ActivityEquipment.Count().Should().Be(2);
    }

    [Test]
    public async Task UpdateActivityAync_WhenActivityNotFound_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateActivityAync_WhenActivityNotFound_ReturnsFailure));
        var service = CreateService(context);

        var result = await service.UpdateActivityAsync(new UpdateActivityDto
        {
            Id = "missing", Title = "Updated", Description = "Desc",
            StartAt = DateTime.UtcNow.AddHours(1), EndAt = DateTime.UtcNow.AddHours(2),
        }, "user-1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Activity not found.");
    }

    [Test]
    public async Task UpdateActivityAync_WhenUserIsNotCreator_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateActivityAync_WhenUserIsNotCreator_ReturnsFailure));

        var creator = new User { Id = "creator-1", Name = "Creator", Email = "creator@example.com" };
        var otherUser = new User { Id = "other-1", Name = "Other", Email = "other@example.com" };
        var activity = new Activity
        {
            Id = "activity-1", Title = "Initial", Description = "Desc",
            StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddHours(1),
            CreatedBy = creator.Id, Creator = creator,
            Localisation = new Localisation { Id = "loc-1", Type = LocalisationType.Address, DisplayName = "Paris" }
        };

        context.Users.AddRange(creator, otherUser);
        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.UpdateActivityAsync(new UpdateActivityDto
        {
            Id = activity.Id, Title = "Updated", Description = "Updated desc",
            StartAt = DateTime.UtcNow.AddHours(2), EndAt = DateTime.UtcNow.AddHours(3),
        }, otherUser.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("You are not allowed to update this activity.");
    }

    [Test]
    public async Task UpdateActivityAync_WithValidData_UpdatesActivityAndRelations()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateActivityAync_WithValidData_UpdatesActivityAndRelations));

        var creator = new User { Id = "creator-1", Name = "Creator", Email = "creator@example.com" };
        var existingEquipment = new Equipment { Id = "eq-1", Name = "Backpack" };
        var location = new Localisation { Id = "loc-1", Type = LocalisationType.Address, Address = "Old address", DisplayName = "Old address" };
        var activity = new Activity
        {
            Id = "activity-1", Title = "Initial", Description = "Desc",
            StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddHours(1),
            CreatedBy = creator.Id, Creator = creator, Localisation = location, EstimatedPrice = 10
        };

        context.Users.Add(creator);
        context.Equipment.Add(existingEquipment);
        context.Activities.Add(activity);
        context.SubActivities.Add(new SubActivity
        {
            Id = "sub-old", ActivityId = activity.Id, Activity = activity, Name = "Old sub",
            StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(10),
            Localisation = location, LocalisationId = location.Id
        });
        context.ActivityEquipment.Add(new ActivityEquipment
        {
            Id = "ae-old", ActivityId = activity.Id, EquipmentId = existingEquipment.Id, Required = true, Quantity = 1
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var start = DateTime.UtcNow.AddDays(1);

        var result = await service.UpdateActivityAsync(new UpdateActivityDto
        {
            Id = activity.Id, Title = "Updated title", Description = "Updated description",
            StartAt = start, EndAt = start.AddHours(2), Address = "25 avenue des Champs Elysees",
            EstimatedPrice = 42,
            RequiredEquipmentNames = new List<string> { "Backpack", "Water" },
            SubActivities = new List<CreateSubActivityDto>
            {
                new() { Name = "New sub", StartTime = start.AddMinutes(30), EndTime = start.AddMinutes(60), Description = "New sub desc", Price = 9 }
            }
        }, creator.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data.Title.Should().Be("Updated title");
        context.Activities.Single().EstimatedPrice.Should().Be(42);
        context.SubActivities.Should().ContainSingle(s => s.Name == "New sub");
        context.SubActivities.Should().NotContain(s => s.Name == "Old sub");
        context.ActivityEquipment.Count().Should().Be(2);
    }

    [Test]
    public async Task UpdateActivityAync_WithChangedFields_PreservesImmutableDataAndUpdatesLinkedInformation()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateActivityAync_WithChangedFields_PreservesImmutableDataAndUpdatesLinkedInformation));

        var creator = new User { Id = "creator-2", Name = "Creator2", Email = "creator2@example.com" };
        var baseStart = DateTime.UtcNow.AddDays(2);
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var updatedAt = DateTime.UtcNow.AddDays(-2);
        var sharedLocalisation = new Localisation { Id = "loc-shared", Type = LocalisationType.Address, Address = "1 old street", DisplayName = "1 old street" };
        var activity = new Activity
        {
            Id = "activity-full-update", Title = "Old title", Description = "Old description",
            StartAt = baseStart, EndAt = baseStart.AddHours(2), EstimatedPrice = 15,
            CreatedBy = creator.Id, Creator = creator, Localisation = sharedLocalisation,
            CreatedAt = createdAt, UpdatedAt = updatedAt
        };
        var eqOld = new Equipment { Id = "eq-old", Name = "Backpack" };
        var subToUpdate = new SubActivity
        {
            Id = "sub-to-update", ActivityId = activity.Id, Activity = activity, Name = "Sub old",
            StartTime = baseStart.AddMinutes(10), EndTime = baseStart.AddMinutes(40),
            Description = "Sub old desc", Price = 2, Localisation = sharedLocalisation, LocalisationId = sharedLocalisation.Id
        };
        var subToDelete = new SubActivity
        {
            Id = "sub-to-delete", ActivityId = activity.Id, Activity = activity, Name = "Sub delete",
            StartTime = baseStart.AddMinutes(45), EndTime = baseStart.AddMinutes(65),
            Localisation = sharedLocalisation, LocalisationId = sharedLocalisation.Id
        };

        context.Users.Add(creator);
        context.Activities.Add(activity);
        context.Equipment.Add(eqOld);
        context.SubActivities.AddRange(subToUpdate, subToDelete);
        context.ActivityEquipment.Add(new ActivityEquipment { Id = "ae-old-2", ActivityId = activity.Id, EquipmentId = eqOld.Id, Required = true, Quantity = 1 });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var newStart = baseStart.AddDays(1);

        var result = await service.UpdateActivityAsync(new UpdateActivityDto
        {
            Id = activity.Id, Title = "New title", Description = "New description",
            StartAt = newStart, EndAt = newStart.AddHours(3), Address = "99 new street", EstimatedPrice = 99,
            RequiredEquipmentNames = new List<string> { "Backpack", "Water bottle" },
            SubActivities = new List<CreateSubActivityDto>
            {
                new() { Id = subToUpdate.Id, Name = "Sub updated", StartTime = newStart.AddMinutes(5), EndTime = newStart.AddMinutes(25), Description = "Sub updated desc", Price = 8 },
                new() { Name = "Sub created", StartTime = newStart.AddMinutes(30), EndTime = newStart.AddMinutes(50), Description = "Sub new desc", Price = 12 }
            }
        }, creator.Id);

        result.IsSuccess.Should().BeTrue();
        var persisted = await context.Activities.Include(a => a.Localisation).Include(a => a.SubActivities).Include(a => a.ActivityEquipments!).SingleAsync(a => a.Id == activity.Id);
        persisted.Id.Should().Be(activity.Id);
        persisted.CreatedBy.Should().Be(creator.Id);
        persisted.CreatedAt.Should().Be(createdAt);
        persisted.UpdatedAt.Should().BeAfter(updatedAt);
        persisted.Title.Should().Be("New title");
        persisted.Description.Should().Be("New description");
        persisted.StartAt.Should().Be(newStart);
        persisted.EndAt.Should().Be(newStart.AddHours(3));
        persisted.EstimatedPrice.Should().Be(99);
        persisted.Localisation.Address.Should().Be("99 new street");
        persisted.Localisation.DisplayName.Should().Be("99 new street");
        persisted.SubActivities.Should().HaveCount(2);
        persisted.SubActivities.Should().Contain(s => s.Id == subToUpdate.Id && s.Name == "Sub updated");
        persisted.SubActivities.Should().NotContain(s => s.Id == subToDelete.Id);
        persisted.SubActivities.Should().Contain(s => s.Name == "Sub created");
        var equipmentNames = await context.ActivityEquipment.Where(ae => ae.ActivityId == activity.Id).Join(context.Equipment, ae => ae.EquipmentId, e => e.Id, (_, e) => e.Name).OrderBy(n => n).ToListAsync();
        equipmentNames.Should().Equal("Backpack", "Water bottle");
    }

    [Test]
    public async Task UpdateActivityAync_WithSameValues_KeepsDataConsistent()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateActivityAync_WithSameValues_KeepsDataConsistent));

        var creator = new User { Id = "creator-3", Name = "Creator3", Email = "creator3@example.com" };
        var start = DateTime.UtcNow.AddDays(3);
        var createdAt = DateTime.UtcNow.AddDays(-7);
        var localisation = new Localisation { Id = "loc-same", Type = LocalisationType.Address, Address = "10 same street", DisplayName = "10 same street" };
        var activity = new Activity
        {
            Id = "activity-same-update", Title = "Stable title", Description = "Stable description",
            StartAt = start, EndAt = start.AddHours(2), EstimatedPrice = 30,
            CreatedBy = creator.Id, Creator = creator, Localisation = localisation,
            CreatedAt = createdAt, UpdatedAt = createdAt
        };
        var equipment = new Equipment { Id = "eq-same", Name = "Bottle" };
        var sub = new SubActivity
        {
            Id = "sub-same", ActivityId = activity.Id, Activity = activity, Name = "Sub stable",
            StartTime = start.AddMinutes(15), EndTime = start.AddMinutes(45),
            Description = "Stable sub description", Price = 4, Localisation = localisation, LocalisationId = localisation.Id
        };

        context.Users.Add(creator);
        context.Activities.Add(activity);
        context.Equipment.Add(equipment);
        context.SubActivities.Add(sub);
        context.ActivityEquipment.Add(new ActivityEquipment { Id = "ae-same", ActivityId = activity.Id, EquipmentId = equipment.Id, Required = true, Quantity = 1 });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.UpdateActivityAsync(new UpdateActivityDto
        {
            Id = activity.Id, Title = "Stable title", Description = "Stable description",
            StartAt = start, EndAt = start.AddHours(2), Address = "10 same street", EstimatedPrice = 30,
            RequiredEquipmentNames = new List<string> { "Bottle" },
            SubActivities = new List<CreateSubActivityDto>
            {
                new() { Id = sub.Id, Name = "Sub stable", StartTime = start.AddMinutes(15), EndTime = start.AddMinutes(45), Description = "Stable sub description", Price = 4 }
            }
        }, creator.Id);

        result.IsSuccess.Should().BeTrue();
        var persisted = await context.Activities.Include(a => a.Localisation).Include(a => a.SubActivities).SingleAsync(a => a.Id == activity.Id);
        persisted.Id.Should().Be(activity.Id);
        persisted.CreatedBy.Should().Be(creator.Id);
        persisted.CreatedAt.Should().Be(createdAt);
        persisted.Title.Should().Be("Stable title");
        persisted.Description.Should().Be("Stable description");
        persisted.StartAt.Should().Be(start);
        persisted.EndAt.Should().Be(start.AddHours(2));
        persisted.EstimatedPrice.Should().Be(30);
        persisted.Localisation.DisplayName.Should().Be("10 same street");
        persisted.SubActivities.Should().ContainSingle(s => s.Id == sub.Id && s.Name == "Sub stable" && s.Description == "Stable sub description" && s.Price == 4);
        var equipmentNames = await context.ActivityEquipment.Where(ae => ae.ActivityId == activity.Id).Join(context.Equipment, ae => ae.EquipmentId, e => e.Id, (_, e) => e.Name).ToListAsync();
        equipmentNames.Should().ContainSingle().Which.Should().Be("Bottle");
    }

    [Test]
    public async Task DeleteActivityAsync_WhenActivityNotFound_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteActivityAsync_WhenActivityNotFound_ReturnsFailure));
        var service = CreateService(context);

        var result = await service.DeleteActivityAsync("non-existent-activity", "user-1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Activity not found");
    }

    [Test]
    public async Task DeleteActivityAsync_WhenUserIsNotCreator_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteActivityAsync_WhenUserIsNotCreator_ReturnsFailure));

        var creator = new User { Id = "creator-1", Name = "Creator", Email = "creator@example.com" };
        var otherUser = new User { Id = "other-1", Name = "Other", Email = "other@example.com" };
        var activity = new Activity
        {
            Id = "activity-1", Title = "Test Activity", Description = "Test Description",
            StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddHours(1),
            CreatedBy = creator.Id, Creator = creator,
            Localisation = new Localisation { Id = "loc-1", Type = LocalisationType.Address, DisplayName = "Paris" }
        };

        context.Users.AddRange(creator, otherUser);
        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.DeleteActivityAsync(activity.Id, otherUser.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Activity not found");
    }

    [Test]
    public async Task DeleteActivityAsync_WhenValidRequest_DeletesAllRelatedData()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteActivityAsync_WhenValidRequest_DeletesAllRelatedData));

        var creator = new User { Id = "creator-1", Name = "Creator", Email = "creator@example.com" };
        var participant = new User { Id = "user-1", Name = "User1", Email = "user1@example.com" };
        var equipment = new Equipment { Id = "eq-1", Name = "Test Equipment" };
        var localisation = new Localisation { Id = "localisation-1", Type = LocalisationType.Virtual, Address = "123 Main St", MapLink = "https://maps.example.com/?q=123+Main+St", VirtualUrl = null, DisplayName = "Salle de Test" };
        var activity = new Activity
        {
            Id = "activity-1", Title = "Test Activity", Description = "Test Description",
            StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddHours(2),
            CreatedBy = creator.Id, Creator = creator, Localisation = localisation
        };
        var subActivity = new SubActivity { Id = "sub-1", Name = "Test Sub Activity", StartTime = DateTime.UtcNow.AddMinutes(30), EndTime = DateTime.UtcNow.AddMinutes(90), ActivityId = activity.Id, Activity = activity, Localisation = localisation };
        var activityEquipment = new ActivityEquipment { Id = "ae-1", ActivityId = activity.Id, EquipmentId = equipment.Id, Required = true, Quantity = 1 };
        var participation = new UserParticipation { Id = "part-1", ActivityId = activity.Id, UserId = participant.Id, Activity = activity, User = participant };

        context.Users.AddRange(creator, participant);
        context.Equipment.Add(equipment);
        context.Activities.Add(activity);
        context.SubActivities.Add(subActivity);
        context.ActivityEquipment.Add(activityEquipment);
        context.UserParticipation.Add(participation);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.DeleteActivityAsync(activity.Id, creator.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(activity.Id);
        context.Activities.Should().BeEmpty();
        context.SubActivities.Should().BeEmpty();
        context.ActivityEquipment.Should().BeEmpty();
        context.UserParticipation.Should().BeEmpty();
    }

    [Test]
    public async Task GetActivityDetails_TotalPrice_IsCorrectlyCalculated_InAllEdgeCases()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(GetActivityDetails_TotalPrice_IsCorrectlyCalculated_InAllEdgeCases));

        var user = new User { Id = "u1", Name = "Test User", Email = "test@example.com" };
        context.Users.Add(user);
        var localisation = new Localisation { Id = "loc-1" };
        context.Localisations.Add(localisation);

        var act1 = new Activity { Id = "act-1", Title = "Complete Activity", Description = "desc", EstimatedPrice = 45.50, CreatedBy = user.Id, Creator = user, StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddHours(2), Localisation = localisation };
        context.Activities.Add(act1);
        context.SubActivities.AddRange(
            new SubActivity { Id = "s1", ActivityId = act1.Id, Price = 10, Name = "sub1", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Localisation = localisation },
            new SubActivity { Id = "s2", ActivityId = act1.Id, Price = 15.75, Name = "sub2", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Localisation = localisation }
        );

        var act2 = new Activity { Id = "act-2", Title = "Solo Activity", Description = "desc", EstimatedPrice = 120, CreatedBy = user.Id, Creator = user, StartAt = DateTime.UtcNow.AddDays(1), EndAt = DateTime.UtcNow.AddDays(1).AddHours(3), Localisation = localisation };
        context.Activities.Add(act2);

        var act3 = new Activity { Id = "act-3", Title = "Free Main Activity", Description = "desc", EstimatedPrice = null, CreatedBy = user.Id, Creator = user, StartAt = DateTime.UtcNow.AddDays(2), EndAt = DateTime.UtcNow.AddDays(2).AddHours(2), Localisation = localisation };
        context.Activities.Add(act3);
        context.SubActivities.AddRange(
            new SubActivity { Id = "s3", ActivityId = act3.Id, Price = 8, Name = "sub3", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Localisation = localisation },
            new SubActivity { Id = "s4", ActivityId = act3.Id, Price = null, Name = "sub4", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Localisation = localisation }
        );

        var act4 = new Activity { Id = "act-4", Title = "Completely Free", Description = "desc", EstimatedPrice = null, CreatedBy = user.Id, Creator = user, StartAt = DateTime.UtcNow.AddDays(3), EndAt = DateTime.UtcNow.AddDays(3).AddHours(2), Localisation = localisation };
        context.Activities.Add(act4);

        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result1 = await service.GetActivityByIdAsync("act-1", user.Id);
        var result2 = await service.GetActivityByIdAsync("act-2", user.Id);
        var result3 = await service.GetActivityByIdAsync("act-3", user.Id);
        var result4 = await service.GetActivityByIdAsync("act-4", user.Id);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result3.IsSuccess.Should().BeTrue();
        result4.IsSuccess.Should().BeTrue();
        result1.Data!.TotalPrice.Should().Be(45.50 + 10 + 15.75);
        result2.Data!.TotalPrice.Should().Be(120);
        result3.Data!.TotalPrice.Should().Be(8);
        result4.Data!.TotalPrice.Should().Be(0);
    }

    [Test]
    public async Task CreateActivity_StartAt_HourShouldMatchInput()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CreateActivity_StartAt_HourShouldMatchInput));
        var user = new User { Id = "user-utc-1", Name = "UTC User", Email = "utc@example.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var startAt = new DateTime(2026, 8, 15, 18, 0, 0, DateTimeKind.Utc);
        var result = await service.CreateActivityAsync(BuildMinimalCreateDto(startAt), user.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.StartAt.Hour.Should().Be(18, because: "the service must not shift or alter the hour value");
    }

    [Test]
    public async Task CreateActivity_StartAt_ShouldNotHaveLocalKind()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CreateActivity_StartAt_ShouldNotHaveLocalKind));
        var user = new User { Id = "user-utc-2", Name = "UTC User 2", Email = "utc2@example.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var startAt = new DateTime(2026, 8, 15, 18, 0, 0, DateTimeKind.Utc);
        var result = await service.CreateActivityAsync(BuildMinimalCreateDto(startAt), user.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.StartAt.Kind.Should().NotBe(DateTimeKind.Local, because: "DateTimeKind.Local would cause incorrect serialization on servers in non-UTC time zones");
    }

    [Test]
    public async Task UpdateActivity_StartAt_HourShouldMatchInput()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateActivity_StartAt_HourShouldMatchInput));
        var user = new User { Id = "user-utc-3", Name = "UTC User 3", Email = "utc3@example.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var created = await service.CreateActivityAsync(BuildMinimalCreateDto(new DateTime(2026, 8, 15, 18, 0, 0, DateTimeKind.Utc)), user.Id);
        created.IsSuccess.Should().BeTrue();

        var newStartAt = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);
        var result = await service.UpdateActivityAsync(new UpdateActivityDto
        {
            Id = created.Data!.Id, Title = "Updated", Description = "Updated desc",
            StartAt = newStartAt, EndAt = newStartAt.AddHours(2),
            RequiredEquipmentNames = [], SubActivities = []
        }, user.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.StartAt.Hour.Should().Be(10, because: "the updated hour must exactly match what the frontend sent in UTC");
    }

    private static CreateActivityDto BuildMinimalCreateDto(DateTime startAt) => new()
    {
        Title = "UTC Test Activity", Description = "Testing UTC hour preservation",
        StartAt = startAt, EndAt = startAt.AddHours(2),
        Time = $"{startAt.Hour:D2}:{startAt.Minute:D2}",
        RequiredEquipmentNames = [], SubActivities = []
    };

    private static ActivityService CreateService(Friendout.Domain.Context.FriendoutDbContext context)
    {
        return new ActivityService(
            context,
            TestLogger<ActivityService>.Instance,
            new NoopFileService(),
            new NoopNotificationDispatcher(),
            new NoopScopeFactory());
    }

    private sealed class NoopNotificationDispatcher : INotificationDispatcher
    {
        public Task DispatchNotificationAsync(Guid userId, Friendout.Domain.Enums.NotificationType type, Dictionary<string, string> data)
            => Task.CompletedTask;
    }

    private sealed class NoopScopeFactory : IServiceScopeFactory
    {
        public IServiceScope CreateScope() => new NoopScope();

        private sealed class NoopScope : IServiceScope
        {
            public IServiceProvider ServiceProvider => new NoopServiceProvider();
            public void Dispose() { }
        }

        private sealed class NoopServiceProvider : IServiceProvider
        {
            public object? GetService(Type serviceType) => null;
        }
    }

    private sealed class NoopFileService : IFileService
    {
        public Task<string> SaveFileAsync(FileUpload file, Friendout.Infrastructure.Enums.FileCategory category) => Task.FromResult("file.png");
        public Task DeleteFileAsync(string fileName, Friendout.Infrastructure.Enums.FileCategory category) => Task.CompletedTask;
        public string GetFilePath(string fileName, Friendout.Infrastructure.Enums.FileCategory category) => fileName;
        public string GetFileUrl(string fileName, Friendout.Infrastructure.Enums.FileCategory category) => $"/uploads/{fileName}";
    }
}
