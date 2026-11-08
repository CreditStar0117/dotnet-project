using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todo.Application.Abstractions;
using Todo.Infrastructure.Persistence;

namespace Todo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTodoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TodoDb")
            ?? throw new InvalidOperationException("Connection string 'TodoDb' is not configured.");

        services.AddDbContext<TodoDbContext>(options => options.UseSqlServer(
            connectionString,
            sqlServer => sqlServer.EnableRetryOnFailure()));
        services.AddScoped<ITodoRepository, TodoRepository>();

        return services;
    }
}
