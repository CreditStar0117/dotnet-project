namespace Todo.Application.Contracts;

public sealed record TodoDetails(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    string Version);

public sealed record TodoListItem(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    string Version);

public sealed record TodoPage(
    IReadOnlyList<TodoListItem> Items,
    int Page,
    int PageSize,
    int TotalCount);
