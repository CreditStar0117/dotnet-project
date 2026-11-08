using Todo.Application.Abstractions;
using Todo.Application.Contracts;

namespace Todo.Application.Queries;

public sealed record GetTodos(int Page, int PageSize, bool? IsCompleted);

public sealed class GetTodosHandler(ITodoRepository repository)
{
    public Task<TodoPage> Handle(GetTodos query, CancellationToken cancellationToken) =>
        repository.GetPageAsync(query.Page, query.PageSize, query.IsCompleted, cancellationToken);
}
