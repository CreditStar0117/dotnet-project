using Microsoft.EntityFrameworkCore;
using Todo.Application.Abstractions;
using Todo.Application.Contracts;
using Todo.Application.Exceptions;
using Todo.Domain.Entities;

namespace Todo.Infrastructure.Persistence;

public sealed class TodoRepository(TodoDbContext dbContext) : ITodoRepository
{
    public Task AddAsync(TodoItem item, CancellationToken cancellationToken) =>
        dbContext.Todos.AddAsync(item, cancellationToken).AsTask();

    public Task<TodoItem?> FindForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Todos.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task<TodoDetails?> GetDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await dbContext.Todos
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new TodoProjection(
                item.Id,
                item.Title,
                item.Description,
                item.IsCompleted,
                item.CreatedAtUtc,
                item.UpdatedAtUtc,
                item.RowVersion))
            .SingleOrDefaultAsync(cancellationToken);

        return item is null ? null : new TodoDetails(
            item.Id, item.Title, item.Description, item.IsCompleted,
            item.CreatedAtUtc, item.UpdatedAtUtc, Convert.ToBase64String(item.RowVersion));
    }

    public async Task<TodoPage> GetPageAsync(
        int page,
        int pageSize,
        bool? isCompleted,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Todos.AsNoTracking().AsQueryable();
        if (isCompleted.HasValue)
        {
            query = query.Where(item => item.IsCompleted == isCompleted.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(item => item.CreatedAtUtc)
            .ThenByDescending(item => item.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new TodoProjection(
                item.Id,
                item.Title,
                item.Description,
                item.IsCompleted,
                item.CreatedAtUtc,
                item.UpdatedAtUtc,
                item.RowVersion))
            .ToListAsync(cancellationToken);

        var items = rows.Select(item => new TodoListItem(
            item.Id,
            item.Title,
            item.Description,
            item.IsCompleted,
            item.CreatedAtUtc,
            item.UpdatedAtUtc,
            Convert.ToBase64String(item.RowVersion))).ToList();

        return new TodoPage(items, page, pageSize, totalCount);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException();
        }
    }

    private sealed record TodoProjection(
        Guid Id,
        string Title,
        string? Description,
        bool IsCompleted,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc,
        byte[] RowVersion);
}
