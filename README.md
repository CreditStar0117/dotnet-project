# Todo platform foundation

A compact .NET 8 product-management Todo API organized into Domain, Application, Infrastructure, and API projects. It uses explicit command/query handlers (CQRS without a mediator dependency), EF Core 8, and SQL Server.

## Requirements and run

- .NET 8 SDK
- SQL Server (local or remote)

Set `ConnectionStrings__TodoDb` to a SQL Server connection string, then create and apply the initial migration:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.22
dotnet ef migrations add InitialCreate --project src/Todo.Infrastructure --startup-project src/Todo.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/Todo.Infrastructure --startup-project src/Todo.Api
dotnet run --project src/Todo.Api
```

The checked-in configuration uses trusted SQL Server authentication on `localhost`. Override it via the environment variable above, especially for SQL authentication or a hosted database. Migrations are applied explicitly; the API does not mutate schema on startup.

## API

- `POST /api/todos` — create a Todo (`title` required, up to 200 characters; `description` optional, up to 2,000).
- `GET /api/todos?page=1&pageSize=20&isCompleted=false` — list with bounded page size (1–100).
- `GET /api/todos/{id}` — retrieve one Todo.
- `POST /api/todos/{id}/complete` — complete a Todo using the quoted ETag from the previous response in the `If-Match` header.
- `GET /health` — process liveness.

Create and detail responses include a base64 `version` and an `ETag` header. Missing `If-Match` returns 428; stale versions return 409.

## Data access and scaling

- Commands and queries are separate handlers. Application depends on domain contracts; Infrastructure owns EF Core mappings and SQL Server operations.
- Queries are asynchronous, use `AsNoTracking`, project to response shapes, filter and paginate server-side, and order deterministically.
- `IsCompleted, CreatedAtUtc DESC` is indexed for the list filter/order. Review query plans and workload before adding indexes; avoid indexing long text without evidence.
- SQL Server `rowversion` is the optimistic concurrency token. Each command persists at one `SaveChangesAsync` boundary, which EF Core wraps transactionally. Conflicts return 409 instead of silently overwriting.
- Transient SQL failures are retried. Keep transactions short and within one command boundary. For multi-aggregate workflows or external side effects, introduce an explicit transaction/outbox rather than holding a database transaction across network calls.
- EF Core is the only ORM here. EF6 is a separate ORM generation and is omitted because this repository has no legacy EF6 model that needs it; including both would duplicate persistence stacks without benefit.

Run domain tests with:

```powershell
dotnet test Todo.sln
```
