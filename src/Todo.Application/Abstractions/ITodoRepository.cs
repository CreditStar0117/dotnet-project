using Todo.Application.Contracts;
using Todo.Domain.Entities;

namespace Todo.Application.Abstractions;

public interface ITodoRepository
{
    Task AddAsync(TodoItem item, CancellationToken cancellationToken);
    Task<TodoItem?> FindForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task<TodoDetails?> GetDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<TodoPage> GetPageAsync(int page, int pageSize, bool? isCompleted, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
