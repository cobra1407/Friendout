using FluentAssertions;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Services;

namespace Friendout.Test;

public class CommentServiceTests
{
    [Test]
    public async Task CreateCommentAsync_WithValidData_ReturnsCommentDto()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CreateCommentAsync_WithValidData_ReturnsCommentDto));

        var user = new User { Id = "user-1", Name = "Alice", Email = "alice@example.com" };
        var activity = new Activity
        {
            Id = "activity-1",
            Title = "Test",
            Description = "Desc",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            CreatedBy = user.Id,
            Creator = user,
            Localisation = new Localisation { Id = "loc-1", Type = Friendout.Domain.Enums.LocalisationType.Address, DisplayName = "Paris" }
        };

        context.Users.Add(user);
        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var service = new CommentService(context, TestLogger<CommentService>.Instance);

        var result = await service.CreateCommentAsync(activity.Id, user.Id, "  Hello world  ");

        result.IsSuccess.Should().BeTrue();
        result.Data.Content.Should().Be("Hello world");
        result.Data.SendBy.Should().Be("Alice");
        context.Comments.Count().Should().Be(1);
    }

    [Test]
    public async Task CreateCommentAsync_WithEmptyContent_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CreateCommentAsync_WithEmptyContent_ReturnsFailure));
        var service = new CommentService(context, TestLogger<CommentService>.Instance);

        var result = await service.CreateCommentAsync("activity-1", "user-1", " ");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Content cannot be empty.");
    }

    [Test]
    public async Task UpdateCommentAsync_WhenUserIsNotOwner_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(UpdateCommentAsync_WhenUserIsNotOwner_ReturnsFailure));

        var owner = new User { Id = "owner", Name = "Owner", Email = "owner@example.com" };
        var other = new User { Id = "other", Name = "Other", Email = "other@example.com" };
        var activity = new Activity
        {
            Id = "activity-1",
            Title = "Test",
            Description = "Desc",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            CreatedBy = owner.Id,
            Creator = owner,
            Localisation = new Localisation { Id = "loc-1", Type = Friendout.Domain.Enums.LocalisationType.Address, DisplayName = "Paris" }
        };

        var comment = new ActivityComment
        {
            Id = "comment-1",
            ActivityId = activity.Id,
            UserId = owner.Id,
            Content = "Initial",
            User = owner,
            Activity = activity
        };

        context.Users.AddRange(owner, other);
        context.Activities.Add(activity);
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        var service = new CommentService(context, TestLogger<CommentService>.Instance);

        var result = await service.UpdateCommentAsync(activity.Id, comment.Id, other.Id, "Updated");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("You are not allowed to edit this comment.");
    }

    [Test]
    public async Task DeleteCommentAsync_WhenOwner_DeletesComment()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext(nameof(DeleteCommentAsync_WhenOwner_DeletesComment));

        var user = new User { Id = "user-1", Name = "Alice", Email = "alice@example.com" };
        var activity = new Activity
        {
            Id = "activity-1",
            Title = "Test",
            Description = "Desc",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            CreatedBy = user.Id,
            Creator = user,
            Localisation = new Localisation { Id = "loc-1", Type = Friendout.Domain.Enums.LocalisationType.Address, DisplayName = "Paris" }
        };

        var comment = new ActivityComment
        {
            Id = "comment-1",
            ActivityId = activity.Id,
            UserId = user.Id,
            Content = "Initial",
            User = user,
            Activity = activity
        };

        context.Users.Add(user);
        context.Activities.Add(activity);
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        var service = new CommentService(context, TestLogger<CommentService>.Instance);

        var result = await service.DeleteCommentAsync(activity.Id, comment.Id, user.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        context.Comments.Should().BeEmpty();
    }
}

