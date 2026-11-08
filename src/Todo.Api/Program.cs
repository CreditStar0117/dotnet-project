using Todo.Api.Endpoints;
using Todo.Api.Middleware;
using Todo.Application;
using Todo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTodoApplication();
builder.Services.AddTodoInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseMiddleware<ConcurrencyConflictMiddleware>();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapTodoEndpoints();

app.Run();

public partial class Program
{
}
