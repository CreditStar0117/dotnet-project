namespace Todo.Application.Exceptions;

public sealed class ConcurrencyConflictException()
    : Exception("The todo item was changed by another request. Reload it and try again.")
{
}
