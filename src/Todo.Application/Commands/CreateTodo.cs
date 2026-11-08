using Todo.Application.Abstractions;
using Todo.Application.Contracts;
using Todo.Domain.Entities;

namespace Todo.Application.Commands;

public sealed record CreateTodo(string Title, string? Description);

public sealed class CreateTodoHandler(ITodoRepository repository, IClock clock)
{
    public async Task<TodoDetails> Handle(CreateTodo command, CancellationToken cancellationToken)
    {
        var item = TodoItem.Create(command.Title, command.Description, clock.UtcNow);
        await repository.AddAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new TodoDetails(
            item.Id,
            item.Title,
            item.Description,
            item.IsCompleted,
            item.CreatedAtUtc,
            item.UpdatedAtUtc,
            Convert.ToBase64String(item.RowVersion));
    }
}
