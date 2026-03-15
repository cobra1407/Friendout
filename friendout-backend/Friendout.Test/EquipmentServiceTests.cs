using FluentAssertions;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Services;

namespace Friendout.Test;

public class EquipmentServiceTests
{
    [Test]
    public async Task GetUserEquipmentForActivityAsync_WhenActivityNotFound_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(GetUserEquipmentForActivityAsync_WhenActivityNotFound_ReturnsFailure));
        var service = new EquipmentService(context, TestLogger<EquipmentService>.Instance);

        var result = await service.GetUserEquipmentForActivityAsync("unknown", "user-1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Activity not found");
    }

    [Test]
    public async Task SetUserEquipmentAsync_WithPositiveQuantity_CreatesEntry()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(SetUserEquipmentAsync_WithPositiveQuantity_CreatesEntry));

        var user = new User { Id = "user-1", Name = "Alice", Email = "alice@example.com" };
        var equipment = new Equipment { Id = "eq-1", Name = "Backpack" };
        var activity = CreateActivity("activity-1", user);

        context.Users.Add(user);
        context.Equipment.Add(equipment);
        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var service = new EquipmentService(context, TestLogger<EquipmentService>.Instance);

        var result = await service.SetUserEquipmentAsync(activity.Id, equipment.Id, user.Id, 2);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data[0].Quantity.Should().Be(2);
    }

    [Test]
    public async Task SetUserEquipmentAsync_WithZeroQuantity_RemovesExistingEntry()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(SetUserEquipmentAsync_WithZeroQuantity_RemovesExistingEntry));

        var user = new User { Id = "user-1", Name = "Alice", Email = "alice@example.com" };
        var equipment = new Equipment { Id = "eq-1", Name = "Backpack" };
        var activity = CreateActivity("activity-1", user);

        context.Users.Add(user);
        context.Equipment.Add(equipment);
        context.Activities.Add(activity);
        context.UserEquipment.Add(new UserEquipment
        {
            UserId = user.Id,
            EquipmentId = equipment.Id,
            ActivityId = activity.Id,
            Quantity = 3,
            User = user,
            Equipment = equipment
        });
        await context.SaveChangesAsync();

        var service = new EquipmentService(context, TestLogger<EquipmentService>.Instance);

        var result = await service.SetUserEquipmentAsync(activity.Id, equipment.Id, user.Id, 0);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
        context.UserEquipment.Should().BeEmpty();
    }

    [Test]
    public async Task SetUserEquipmentAsync_WhenEquipmentMissing_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(SetUserEquipmentAsync_WhenEquipmentMissing_ReturnsFailure));

        var user = new User { Id = "user-1", Name = "Alice", Email = "alice@example.com" };
        var activity = CreateActivity("activity-1", user);

        context.Users.Add(user);
        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var service = new EquipmentService(context, TestLogger<EquipmentService>.Instance);

        var result = await service.SetUserEquipmentAsync(activity.Id, "missing-equipment", user.Id, 1);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Equipment not found");
    }

    private static Activity CreateActivity(string activityId, User creator)
    {
        return new Activity
        {
            Id = activityId,
            Title = "Activity",
            Description = "Desc",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            CreatedBy = creator.Id,
            Creator = creator,
            Localisation = new Localisation { Id = $"loc-{activityId}", Type = Friendout.Domain.Enums.LocalisationType.Address, DisplayName = "Paris" }
        };
    }
}

