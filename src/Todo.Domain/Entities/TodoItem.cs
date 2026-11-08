namespace Todo.Domain.Entities;

public sealed class TodoItem
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private TodoItem()
    {
    }

    private TodoItem(string title, string? description, DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        Title = NormalizeTitle(title);
        Description = NormalizeDescription(description);
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public static TodoItem Create(string title, string? description, DateTime utcNow) =>
        new(title, description, EnsureUtc(utcNow));

    public void Complete(DateTime utcNow)
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        UpdatedAtUtc = EnsureUtc(utcNow);
    }

    private static string NormalizeTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var normalized = title.Trim();
        if (normalized.Length > TitleMaxLength)
        {
            throw new ArgumentException($"Title cannot exceed {TitleMaxLength} characters.", nameof(title));
        }

        return normalized;
    }

    private static string? NormalizeDescription(string? description)
    {
        var normalized = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalized?.Length > DescriptionMaxLength)
        {
            throw new ArgumentException($"Description cannot exceed {DescriptionMaxLength} characters.", nameof(description));
        }

        return normalized;
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", nameof(value));
        }

        return value;
    }
}
