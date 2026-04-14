---
title: "Minimal APIs in .NET: Building Lightweight HTTP APIs"
date: 2025-04-10T00:00:00Z
draft: false
tags: [".NET", "API", "Web Development"]
slug: "minimal-apis-dotnet"
description: "A comprehensive guide to building clean, fast HTTP APIs with .NET Minimal APIs — from route handlers and parameter binding to filters and OpenAPI integration."
---

If you've been building APIs with ASP.NET Core for a while, you're probably very familiar with the controller-based approach: create a controller class, decorate it with attributes, inject your services through the constructor, and wire up your routes. It works, and it works well — but sometimes it feels like you're writing a lot of ceremony for what amounts to "take this request, do something, return a response."

That's exactly the problem Minimal APIs set out to solve. Introduced in .NET 6, Minimal APIs let you define HTTP endpoints with very little boilerplate. No controllers, no attributes, no startup class juggling — just direct route-to-handler mappings in a clean, functional style.

In this post I want to walk you through everything you need to know about Minimal APIs: from your first endpoint all the way to organizing large applications, handling authentication, validation, OpenAPI docs, and performance considerations. Let's get to it.

## Why Minimal APIs?

Before we jump in, let's talk about *why* you'd pick Minimal APIs over the traditional controller-based approach.

**Controllers** are great when you need a structured, opinionated framework. They give you model binding, filters, content negotiation, and a clear separation of concerns out of the box. For large enterprise applications with dozens of developers, the consistency that controllers enforce can be a real benefit.

**Minimal APIs**, on the other hand, shine when you want:

- **Less boilerplate** — no controller classes, no `[ApiController]` attributes, no separate startup configuration.
- **Faster startup** — fewer reflection-based operations at boot time.
- **Simpler mental model** — a route maps directly to a handler. That's it.
- **Microservice-friendly** — when your API has 5-10 endpoints, a full controller setup can feel like overkill.

The good news is that this isn't an either-or decision. You can mix controllers and Minimal APIs in the same project. But once you get comfortable with the minimal approach, you might find yourself reaching for it more often than you'd expect.

## Getting Started

Let's create a Minimal API from scratch. If you have the .NET SDK installed, it's as simple as:

```bash
dotnet new web -n MyMinimalApi
cd MyMinimalApi
```

This gives you a `Program.cs` that looks something like this:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

That's it. That's a working API. No `Startup.cs`, no controller class, no routing configuration. You run `dotnet run`, hit `http://localhost:5000`, and you get "Hello World!" back. The simplicity here is the whole point.

Let's make it a bit more useful with a classic todo API:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var todos = new List<Todo>();

app.MapGet("/todos", () => todos);

app.MapGet("/todos/{id:int}", (int id) =>
    todos.FirstOrDefault(t => t.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.MapPost("/todos", (Todo todo) =>
{
    todos.Add(todo);
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapDelete("/todos/{id:int}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null) return Results.NotFound();
    todos.Remove(todo);
    return Results.NoContent();
});

app.Run();

record Todo(int Id, string Title, bool IsComplete);
```

We have full CRUD in under 30 lines. That's the power of the minimal approach.

## Route Handlers

Route handlers are the functions that execute when a request matches a route. You've got several options for defining them.

### Lambda Expressions

The most common approach, and what you'll see in most examples:

```csharp
app.MapGet("/hello", () => "Hello!");
app.MapGet("/hello/{name}", (string name) => $"Hello, {name}!");
```

### Method Groups

For more complex logic, you can point to a named method:

```csharp
app.MapGet("/products", GetProducts);
app.MapPost("/products", CreateProduct);

static IResult GetProducts(AppDbContext db)
    => Results.Ok(db.Products.ToList());

static IResult CreateProduct(Product product, AppDbContext db)
{
    db.Products.Add(product);
    db.SaveChanges();
    return Results.Created($"/products/{product.Id}", product);
}
```

This is my preferred approach for anything beyond a one-liner. It keeps your route mapping section clean and readable — you can see at a glance what endpoints exist without wading through implementation details.

### Local Functions

You can also use local functions, which is useful when you want to keep handlers close to their route definitions:

```csharp
app.MapGet("/health", CheckHealth);

IResult CheckHealth()
{
    // Some health check logic
    return Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
}
```

## Parameter Binding

One of the things I really appreciate about Minimal APIs is how intuitive parameter binding is. The framework figures out where to pull values from based on context and type.

### Route Parameters

```csharp
app.MapGet("/users/{id:int}", (int id) => $"User {id}");
app.MapGet("/files/{*path}", (string path) => $"File: {path}"); // catch-all
```

### Query String Parameters

```csharp
app.MapGet("/search", (string? query, int page = 1, int pageSize = 20) =>
    Results.Ok(new { query, page, pageSize }));
```

Nullable types become optional parameters. Default values work exactly how you'd expect.

### Request Body

```csharp
app.MapPost("/orders", (Order order) =>
{
    // 'order' is automatically deserialized from the JSON body
    return Results.Created($"/orders/{order.Id}", order);
});
```

### Header and Service Binding

```csharp
app.MapGet("/protected", (
    [FromHeader(Name = "X-Api-Key")] string apiKey,
    [FromServices] ILogger<Program> logger) =>
{
    logger.LogInformation("Request with API key: {Key}", apiKey[..4] + "****");
    return Results.Ok("Authorized");
});
```

### HttpContext and HttpRequest

When you need lower-level access:

```csharp
app.MapGet("/info", (HttpContext context) =>
{
    var userAgent = context.Request.Headers.UserAgent.ToString();
    var ip = context.Connection.RemoteIpAddress?.ToString();
    return Results.Ok(new { userAgent, ip });
});
```

### Custom Binding with BindAsync

For complex types, you can implement a static `BindAsync` method:

```csharp
public record PaginationParams(int Page, int PageSize)
{
    public static ValueTask<PaginationParams?> BindAsync(HttpContext context)
    {
        int.TryParse(context.Request.Query["page"], out var page);
        int.TryParse(context.Request.Query["pageSize"], out var pageSize);

        var result = new PaginationParams(
            Page: page > 0 ? page : 1,
            PageSize: pageSize > 0 ? Math.Min(pageSize, 100) : 20
        );

        return ValueTask.FromResult<PaginationParams?>(result);
    }
}

app.MapGet("/items", (PaginationParams pagination, AppDbContext db) =>
{
    var items = db.Items
        .Skip((pagination.Page - 1) * pagination.PageSize)
        .Take(pagination.PageSize)
        .ToList();

    return Results.Ok(items);
});
```

This is incredibly powerful. You define the binding logic once and reuse it across all your endpoints.

## Validation

One area where Minimal APIs don't give you as much out of the box compared to controllers is model validation. There's no automatic `[Required]` or `[StringLength]` enforcement. But there are clean patterns for handling it.

### Manual Validation

The simplest approach — just validate in the handler:

```csharp
app.MapPost("/users", (CreateUserRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email))
        return Results.BadRequest(new { Error = "Email is required" });

    if (request.Name?.Length > 100)
        return Results.BadRequest(new { Error = "Name must be 100 characters or less" });

    // Create user...
    return Results.Created($"/users/{request.Email}", request);
});
```

### Using a Validation Library

For anything non-trivial, I'd recommend reaching for FluentValidation or a similar library:

```csharp
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Age).InclusiveBetween(0, 150);
    }
}
```

You can wire this into your endpoints through endpoint filters, which brings us to the next section.

## Endpoint Filters

Endpoint filters are one of the best features of Minimal APIs. Think of them as middleware, but scoped to specific endpoints instead of the entire pipeline. They were introduced in .NET 7 and they're fantastic for cross-cutting concerns.

### Basic Filter

```csharp
app.MapPost("/todos", (Todo todo) =>
{
    // Handle the request
    return Results.Created($"/todos/{todo.Id}", todo);
})
.AddEndpointFilter(async (context, next) =>
{
    var todo = context.GetArgument<Todo>(0);

    if (string.IsNullOrEmpty(todo.Title))
    {
        return Results.BadRequest(new { Error = "Title is required" });
    }

    return await next(context);
});
```

### Validation Filter with FluentValidation

Here's where it gets really powerful — a reusable validation filter:

```csharp
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var model = context.Arguments
            .OfType<T>()
            .FirstOrDefault();

        if (model is null)
            return Results.BadRequest(new { Error = "Request body is required" });

        var result = await _validator.ValidateAsync(model);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}

// Usage
app.MapPost("/users", (CreateUserRequest request) =>
{
    // Only reached if validation passes
    return Results.Created($"/users/{request.Email}", request);
})
.AddEndpointFilter<ValidationFilter<CreateUserRequest>>();
```

### Logging Filter

```csharp
app.MapGet("/products", (AppDbContext db) => db.Products.ToList())
    .AddEndpointFilter(async (context, next) =>
    {
        var logger = context.HttpContext
            .RequestServices.GetRequiredService<ILogger<Program>>();

        var sw = Stopwatch.StartNew();
        var result = await next(context);
        sw.Stop();

        logger.LogInformation("Endpoint executed in {Elapsed}ms", sw.ElapsedMilliseconds);

        return result;
    });
```

You can chain multiple filters, and they execute in the order they're added — just like middleware.

## OpenAPI / Swagger Integration

Good API documentation isn't optional anymore. Thankfully, Minimal APIs have first-class support for OpenAPI through the `Microsoft.AspNetCore.OpenApi` package.

### Basic Setup

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.MapGet("/todos", () => new List<Todo>())
    .WithName("GetTodos")
    .WithDescription("Retrieves all todo items")
    .WithTags("Todos");

app.Run();
```

### Rich Endpoint Metadata

You can provide detailed information about your endpoints:

```csharp
app.MapPost("/todos", (Todo todo) =>
{
    return Results.Created($"/todos/{todo.Id}", todo);
})
.WithName("CreateTodo")
.WithDescription("Creates a new todo item")
.WithTags("Todos")
.Accepts<Todo>("application/json")
.Produces<Todo>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.ProducesValidationProblem();
```

### Using WithOpenApi for Customization

For fine-grained control over the generated OpenAPI document:

```csharp
app.MapGet("/todos/{id:int}", (int id) => Results.Ok(new Todo(id, "Sample", false)))
    .WithOpenApi(operation =>
    {
        operation.Summary = "Get a specific todo";
        operation.Description = "Retrieves a single todo item by its unique identifier.";
        operation.Parameters[0].Description = "The unique identifier of the todo item";
        return operation;
    });
```

This gives you the same level of documentation control you'd get with Swashbuckle XML comments on controllers, but in a more explicit, code-first way.

## Authentication & Authorization

Securing Minimal API endpoints follows the same patterns as the rest of ASP.NET Core — you just apply them differently.

### Basic Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-identity-provider.com";
        options.Audience = "your-api";
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("PremiumUser", policy => policy.RequireClaim("subscription", "premium"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
```

### Applying Authorization to Endpoints

```csharp
// Require any authenticated user
app.MapGet("/profile", (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        Name = user.FindFirstValue(ClaimTypes.Name),
        Email = user.FindFirstValue(ClaimTypes.Email)
    });
}).RequireAuthorization();

// Require a specific policy
app.MapDelete("/admin/users/{id}", (int id) =>
{
    // Delete user logic
    return Results.NoContent();
}).RequireAuthorization("AdminOnly");

// Allow anonymous access
app.MapGet("/public/health", () => Results.Ok(new { Status = "Healthy" }))
    .AllowAnonymous();
```

Notice how you can inject `ClaimsPrincipal` directly into your handler parameters — the framework takes care of the rest. This is one of those small things that makes Minimal APIs feel really elegant.

## Organizing Large APIs

The elephant in the room with Minimal APIs is organization. When your `Program.cs` has 50 endpoints, it becomes a mess. Here are the patterns I use to keep things manageable.

### Route Groups

Route groups (introduced in .NET 7) let you share configuration across related endpoints:

```csharp
var todos = app.MapGroup("/todos")
    .WithTags("Todos")
    .RequireAuthorization();

todos.MapGet("/", (AppDbContext db) => db.Todos.ToListAsync());
todos.MapGet("/{id:int}", (int id, AppDbContext db) => db.Todos.FindAsync(id));
todos.MapPost("/", (Todo todo, AppDbContext db) =>
{
    db.Todos.Add(todo);
    db.SaveChangesAsync();
    return Results.Created($"/todos/{todo.Id}", todo);
});
```

All endpoints in the group share the `/todos` prefix, the `Todos` tag, and the authorization requirement. Clean.

### Extension Methods

This is the pattern that really scales. Move each group of endpoints into its own static class:

```csharp
// Endpoints/TodoEndpoints.cs
public static class TodoEndpoints
{
    public static RouteGroupBuilder MapTodoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/todos").WithTags("Todos");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);

        return group;
    }

    private static async Task<IResult> GetAll(AppDbContext db)
        => Results.Ok(await db.Todos.ToListAsync());

    private static async Task<IResult> GetById(int id, AppDbContext db)
        => await db.Todos.FindAsync(id) is { } todo
            ? Results.Ok(todo)
            : Results.NotFound();

    private static async Task<IResult> Create(Todo todo, AppDbContext db)
    {
        db.Todos.Add(todo);
        await db.SaveChangesAsync();
        return Results.Created($"/todos/{todo.Id}", todo);
    }

    private static async Task<IResult> Update(int id, Todo updated, AppDbContext db)
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo is null) return Results.NotFound();

        todo.Title = updated.Title;
        todo.IsComplete = updated.IsComplete;
        await db.SaveChangesAsync();

        return Results.Ok(todo);
    }

    private static async Task<IResult> Delete(int id, AppDbContext db)
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo is null) return Results.NotFound();

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}

// Program.cs — stays clean
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapTodoEndpoints();
app.MapUserEndpoints();
app.MapOrderEndpoints();

app.Run();
```

Your `Program.cs` becomes a table of contents for your API. Each endpoint group lives in its own file. This is the approach I recommend for production applications.

### The Carter Library

If you want even more structure, the Carter library provides a module-based approach:

```csharp
public class TodoModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/todos", async (AppDbContext db) =>
            await db.Todos.ToListAsync());

        app.MapPost("/todos", async (Todo todo, AppDbContext db) =>
        {
            db.Todos.Add(todo);
            await db.SaveChangesAsync();
            return Results.Created($"/todos/{todo.Id}", todo);
        });
    }
}
```

Carter automatically discovers and registers all modules. It's a nice middle ground between the raw Minimal API approach and full controllers.

## TypedResults and Response Types

Starting with .NET 7, you can use `TypedResults` instead of `Results` for type-safe responses. This might seem like a small change, but it has real benefits for OpenAPI documentation and testability.

```csharp
app.MapGet("/todos/{id:int}", async Task<Results<Ok<Todo>, NotFound>> (int id, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    return todo is not null
        ? TypedResults.Ok(todo)
        : TypedResults.NotFound();
});
```

The return type `Results<Ok<Todo>, NotFound>` explicitly tells the framework (and your OpenAPI docs) exactly which response types this endpoint can produce. No more guessing, no more manual `Produces<>()` calls for basic cases.

For multiple possible outcomes:

```csharp
app.MapPost("/todos",
    async Task<Results<Created<Todo>, ValidationProblem, Conflict>>
    (Todo todo, AppDbContext db) =>
{
    if (string.IsNullOrEmpty(todo.Title))
        return TypedResults.ValidationProblem(
            new Dictionary<string, string[]>
            {
                { "Title", new[] { "Title is required" } }
            });

    if (await db.Todos.AnyAsync(t => t.Title == todo.Title))
        return TypedResults.Conflict();

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/todos/{todo.Id}", todo);
});
```

I've started using `TypedResults` in all new projects. The compiler catches mismatches between your declared return types and what you actually return, which eliminates an entire class of runtime surprises.

## Performance Considerations

One of the selling points of Minimal APIs is performance, and it's worth understanding *why* they're faster.

**Reduced startup overhead.** Controllers rely heavily on reflection to discover endpoints, bind models, and apply filters. Minimal APIs use source generators (starting in .NET 7) to generate binding code at compile time. This means less work at startup and less memory allocation per request.

**No MVC pipeline.** Controller-based APIs go through the full MVC pipeline: action selection, model binding, action filters, result execution. Minimal APIs skip all of that and go straight from routing to your handler.

**RequestDelegate compilation.** The framework compiles your lambda expressions into optimized `RequestDelegate` instances. The resulting code is very close to what you'd write by hand if you were working directly with `HttpContext`.

Here are some practical tips for maximizing performance:

```csharp
// Use AsNoTracking for read-only queries
app.MapGet("/products", async (AppDbContext db) =>
    await db.Products.AsNoTracking().ToListAsync());

// Return results directly — avoid unnecessary allocations
app.MapGet("/count", async (AppDbContext db) =>
    await db.Products.CountAsync());

// Use cancellation tokens for long-running operations
app.MapGet("/report", async (AppDbContext db, CancellationToken ct) =>
    await db.Orders
        .AsNoTracking()
        .GroupBy(o => o.Status)
        .Select(g => new { Status = g.Key, Count = g.Count() })
        .ToListAsync(ct));
```

It's also worth mentioning that the performance gap between controllers and Minimal APIs keeps narrowing with each .NET release. For most applications, the difference won't be your bottleneck — your database queries and external service calls will. Choose based on developer experience and project needs, not benchmarks.

## Conclusion

Minimal APIs have come a long way since their introduction in .NET 6. What started as a "hello world" demo feature has matured into a legitimate choice for production APIs. With endpoint filters, route groups, typed results, and solid OpenAPI support, you have everything you need to build well-structured, maintainable services.

My recommendation? If you're starting a new API project — especially a microservice or a focused internal API — give Minimal APIs a serious try. Use the extension method pattern for organization, lean on endpoint filters for cross-cutting concerns, and leverage `TypedResults` for type safety.

For existing controller-based projects, there's no rush to migrate. Both approaches work well, and you can even use them side by side. But next time you need to add a small service or a quick internal API, skip the controllers and go minimal. You might not go back.

Happy coding!
