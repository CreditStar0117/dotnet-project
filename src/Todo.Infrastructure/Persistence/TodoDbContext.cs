using Microsoft.EntityFrameworkCore;
using Todo.Domain.Entities;

namespace Todo.Infrastructure.Persistence;

public sealed class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoDbContext).Assembly);
}
