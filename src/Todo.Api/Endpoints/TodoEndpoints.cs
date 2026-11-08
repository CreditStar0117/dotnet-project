using Microsoft.AspNetCore.Mvc;
using Todo.Application.Commands;
using Todo.Application.Queries;

namespace Todo.Api.Endpoints;

public static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/todos").WithTags("Todos");

        group.MapPost("/", async (
            CreateTodoRequest request,
            CreateTodoHandler handler,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var errors = Validate(request);
            if (errors.Count != 0)
            {
                return Results.ValidationProblem(errors);
            }

            var todo = await handler.Handle(
                new CreateTodo(request.Title!, request.Description),
                cancellationToken);
            context.Response.Headers["ETag"] = QuoteVersion(todo.Version);
            return Results.Created($"/api/todos/{todo.Id}", todo);
        });

        group.MapGet("/", async (
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] bool? isCompleted,
            GetTodosHandler handler,
            CancellationToken cancellationToken) =>
        {
            var requestedPage = page ?? 1;
            var requestedPageSize = pageSize ?? 20;
            if (requestedPage < 1 ||
                requestedPageSize < 1 ||
                requestedPageSize > 100 ||
                requestedPage > int.MaxValue / requestedPageSize)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["page"] = ["Page must be positive, pageSize must be between 1 and 100, and the offset must fit in a 32-bit integer."]
                });
            }

            var result = await handler.Handle(
                new GetTodos(requestedPage, requestedPageSize, isCompleted),
                cancellationToken);
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            GetTodoHandler handler,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var todo = await handler.Handle(id, cancellationToken);
            if (todo is null)
            {
                return Results.NotFound();
            }

            context.Response.Headers["ETag"] = QuoteVersion(todo.Version);
            return Results.Ok(todo);
        });

        group.MapPost("/{id:guid}/complete", async (
            Guid id,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            CompleteTodoHandler handler,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(ifMatch))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status428PreconditionRequired,
                    title: "If-Match header is required",
                    detail: "Use the ETag returned by the todo resource.");
            }

            var version = ifMatch.Trim().Trim('"');
            var buffer = new byte[8];
            if (version.Length != 12 ||
                !Convert.TryFromBase64String(version, buffer, out var bytesWritten) ||
                bytesWritten != buffer.Length)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["If-Match"] = ["If-Match must contain a valid todo ETag."]
                });
            }

            var todo = await handler.Handle(
                new CompleteTodo(id, buffer[..bytesWritten]),
                cancellationToken);
            if (todo is null)
            {
                return Results.NotFound();
            }

            context.Response.Headers["ETag"] = QuoteVersion(todo.Version);
            return Results.Ok(todo);
        });

        return endpoints;
    }

    private static Dictionary<string, string[]> Validate(CreateTodoRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors["title"] = ["Title is required."];
        }
        else if (request.Title.Trim().Length > 200)
        {
            errors["title"] = ["Title cannot exceed 200 characters."];
        }

        if (request.Description?.Length > 2000)
        {
            errors["description"] = ["Description cannot exceed 2000 characters."];
        }

        return errors;
    }

    private static string QuoteVersion(string version) => $"\"{version}\"";
}

public sealed record CreateTodoRequest(string? Title, string? Description);
