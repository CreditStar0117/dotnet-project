using Microsoft.AspNetCore.Mvc;
using Todo.Application.Exceptions;

namespace Todo.Api.Middleware;

public sealed class ConcurrencyConflictMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ConcurrencyConflictException)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Concurrency conflict",
                Detail = "The todo item changed after it was loaded. Reload it before retrying."
            });
        }
    }
}
