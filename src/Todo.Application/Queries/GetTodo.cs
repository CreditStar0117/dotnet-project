using Todo.Application.Abstractions;
using Todo.Application.Contracts;

namespace Todo.Application.Queries;

public sealed class GetTodoHandler(ITodoRepository repository)
{
    public Task<TodoDetails?> Handle(Guid id, CancellationToken cancellationToken) =>
        repository.GetDetailsAsync(id, cancellationToken);
}
