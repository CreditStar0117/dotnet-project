using Todo.Domain.Entities;
using Xunit;

namespace Todo.Domain.Tests;

public sealed class TodoItemTests
{
    [Fact]
    public void Create_TrimsTitleAndNormalizesBlankDescription()
    {
        var now = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var item = TodoItem.Create("  Plan release  ", "  ", now);

        Assert.Equal("Plan release", item.Title);
        Assert.Null(item.Description);
        Assert.Equal(now, item.CreatedAtUtc);
        Assert.Equal(now, item.UpdatedAtUtc);
        Assert.False(item.IsCompleted);
    }

    [Fact]
    public void Create_RejectsTitleOverMaximumLength()
    {
        var title = new string('x', TodoItem.TitleMaxLength + 1);

        Assert.Throws<ArgumentException>(() => TodoItem.Create(title, null, DateTime.UtcNow));
    }

    [Fact]
    public void Complete_IsIdempotentAndDoesNotMoveUpdatedTimestampTwice()
    {
        var createdAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var completedAt = createdAt.AddMinutes(2);
        var item = TodoItem.Create("Plan release", null, createdAt);

        item.Complete(completedAt);
        item.Complete(completedAt.AddMinutes(2));

        Assert.True(item.IsCompleted);
        Assert.Equal(completedAt, item.UpdatedAtUtc);
    }
}
