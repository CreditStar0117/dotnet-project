using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Todo.Domain.Entities;

namespace Todo.Infrastructure.Persistence.Configurations;

public sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("Todos");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Title)
            .HasMaxLength(TodoItem.TitleMaxLength)
            .IsRequired();

        builder.Property(item => item.Description)
            .HasMaxLength(TodoItem.DescriptionMaxLength);

        builder.Property(item => item.CreatedAtUtc)
            .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
            .IsRequired();
        builder.Property(item => item.UpdatedAtUtc)
            .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
            .IsRequired();
        builder.Property(item => item.RowVersion).IsRowVersion();

        builder.HasIndex(item => new { item.IsCompleted, item.CreatedAtUtc })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Todos_IsCompleted_CreatedAtUtc");
    }
}
