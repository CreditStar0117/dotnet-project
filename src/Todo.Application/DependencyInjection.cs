using Microsoft.Extensions.DependencyInjection;
using Todo.Application.Abstractions;
using Todo.Application.Commands;
using Todo.Application.Queries;

namespace Todo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTodoApplication(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<CreateTodoHandler>();
        services.AddScoped<CompleteTodoHandler>();
        services.AddScoped<GetTodoHandler>();
        services.AddScoped<GetTodosHandler>();
        return services;
    }

    private sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
