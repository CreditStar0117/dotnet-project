using Todo.Application.Abstractions;
using Todo.Application.Contracts;
using Todo.Application.Exceptions;

namespace Todo.Application.Commands;

public sealed record CompleteTodo(Guid Id, byte[] ExpectedVersion);

public sealed class CompleteTodoHandler(ITodoRepository repository, IClock clock)
{
    public async Task<TodoDetails?> Handle(CompleteTodo command, CancellationToken cancellationToken)
    {
        var item = await repository.FindForUpdateAsync(command.Id, cancellationToken);
        if (item is null)
        {
            return null;
        }

        if (!item.RowVersion.AsSpan().SequenceEqual(command.ExpectedVersion))
        {
            throw new ConcurrencyConflictException();
        }

        item.Complete(clock.UtcNow);
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
