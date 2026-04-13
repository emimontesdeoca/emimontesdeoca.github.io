---
title: "C# Union Types: Discriminated Unions Are Finally Coming"
date: 2026-04-01T00:00:00Z
draft: false
tags: [".NET", "C#"]
slug: "csharp-union-types"
description: "A deep dive into the upcoming C# discriminated union types — what they are, how they work, and why they'll change how you model your domains."
---

If you've been writing C# for any significant amount of time, there's a good chance you've hit a wall when trying to model something that can be "one of several things." Maybe you needed a method to return either a success value or an error. Maybe you were building a payment system that handles credit cards, bank transfers, and digital wallets — each with entirely different data. Or maybe you just looked at F# or Rust and thought, "Why can't I have that in C#?"

The wait is nearly over. **Discriminated unions are coming to C#.**

This has been one of the most requested language features for years, with community discussions stretching back to 2017 and earlier. The C# language design team has been working on a proposal that introduces a `union` keyword for defining closed type hierarchies with exhaustive pattern matching. In this post, I want to walk you through what discriminated unions are, why they matter so much, how we've been faking them until now, and what the proposed syntax actually looks like — with real code examples for each.

A quick note before we dive in: as of this writing, the union types feature is still in the proposal and preview stage. The syntax and behavior I describe here are based on the latest publicly available design documents and discussions from the C# language design team. Things may change before the final release. I'll be clear about what's confirmed versus what's still under discussion.

## What Are Discriminated Unions?

At its core, a discriminated union (sometimes called a "tagged union" or "sum type") is a type that can hold one of a fixed set of possible values, where each variant can carry different data. The "discriminated" part means that the runtime always knows which variant it is — there's a tag that identifies it.

Think of it like an `enum`, but where each member can carry its own payload of data.

If you've used other languages, you've probably seen this concept before:

**F#** has had discriminated unions since day one:

```fsharp
type Shape =
    | Circle of radius: float
    | Rectangle of width: float * height: float
    | Triangle of base: float * height: float
```

**Rust** uses `enum` for the same idea:

```rust
enum Shape {
    Circle { radius: f64 },
    Rectangle { width: f64, height: f64 },
    Triangle { base: f64, height: f64 },
}
```

**TypeScript** achieves something similar with tagged unions:

```typescript
type Shape =
  | { kind: "circle"; radius: number }
  | { kind: "rectangle"; width: number; height: number }
  | { kind: "triangle"; base: number; height: number };
```

In all these cases, the compiler knows every possible variant and can enforce that you handle all of them. That's the superpower: **exhaustiveness checking**. If you add a new variant, the compiler tells you everywhere you forgot to handle it.

C# has never had a first-class way to express this. Until now.

## How We Simulate Unions Today

Over the years, the C# community has come up with several workarounds, each with its own trade-offs. Let me walk through the most common approaches.

### Abstract Records with Inheritance

The most idiomatic workaround in modern C# is using abstract records with sealed derived types:

```csharp
public abstract record Shape
{
    public sealed record Circle(double Radius) : Shape;
    public sealed record Rectangle(double Width, double Height) : Shape;
    public sealed record Triangle(double Base, double Height) : Shape;

    private Shape() { } // Prevent external inheritance
}
```

This works reasonably well. You get immutability, value equality, and you can use pattern matching:

```csharp
double Area(Shape shape) => shape switch
{
    Shape.Circle c => Math.PI * c.Radius * c.Radius,
    Shape.Rectangle r => r.Width * r.Height,
    Shape.Triangle t => 0.5 * t.Base * t.Height,
    _ => throw new InvalidOperationException("Unknown shape")
};
```

But there are significant drawbacks. The compiler doesn't know the hierarchy is closed, so you always need that `_` discard arm or you get a warning. If you add a new variant, the compiler won't tell you about all the places you forgot to handle it — the discard silently swallows it. That completely defeats the purpose.

### The OneOf Library

Another popular approach is the [OneOf](https://github.com/mcintyre321/OneOf) NuGet package:

```csharp
public OneOf<Success<Order>, NotFound, ValidationError> ProcessOrder(OrderRequest request)
{
    // ...
}

// Usage
var result = ProcessOrder(request);
result.Switch(
    success => Console.WriteLine($"Order {success.Value.Id} processed"),
    notFound => Console.WriteLine("Order not found"),
    error => Console.WriteLine($"Validation failed: {error.Message}")
);
```

OneOf provides exhaustiveness checking at compile time through its generic type parameters, which is great. But it relies on positional matching (first type, second type, etc.), the generic signatures get unwieldy fast, and it doesn't integrate with the language's pattern matching. It's a clever hack, but it's still a hack.

### Manual Enum + Data Pattern

Some developers go the classic route with an enum tag and a data container:

```csharp
public enum PaymentType { CreditCard, BankTransfer, DigitalWallet }

public class Payment
{
    public PaymentType Type { get; init; }
    public CreditCardInfo? CreditCard { get; init; }
    public BankTransferInfo? BankTransfer { get; init; }
    public DigitalWalletInfo? DigitalWallet { get; init; }
}
```

This is fragile. Nothing stops you from setting `Type` to `CreditCard` but populating the `BankTransfer` property. The compiler can't help you, and you end up with runtime errors and null checks everywhere. It's the "stringly-typed" approach to type modeling, and it doesn't scale.

All of these approaches share a fundamental problem: **they're fighting the language instead of working with it.** The compiler can't reason about the closed set of possibilities, so you lose the most valuable property of discriminated unions — exhaustive checking.

## The C# Proposal: The `union` Keyword

The C# language team's proposal introduces a dedicated `union` keyword that defines a closed set of named members, each optionally carrying data. Here's the basic syntax as it's been proposed:

```csharp
union Shape
{
    Circle(double Radius),
    Rectangle(double Width, double Height),
    Triangle(double Base, double Height)
}
```

That's it. Clean, concise, and immediately readable. Each member inside the union defines a distinct variant with its own data. The compiler knows that `Shape` can only ever be one of these three things.

Under the hood, the compiler generates a sealed type hierarchy — similar to what you'd write by hand with abstract records, but with full compiler awareness of the closed nature of the type. This means the compiler can enforce exhaustiveness in pattern matching, which is the key benefit.

### Value-Only Members

Union members don't have to carry data. You can mix data-carrying members with simple value members:

```csharp
union Option<T>
{
    Some(T Value),
    None
}
```

This is the classic `Option`/`Maybe` type that functional programmers have been asking for in C# for years. `None` carries no data — it's just a tag.

### Generic Unions

As you can see from the `Option<T>` example above, unions support generics. Here's a more involved example:

```csharp
union Result<T, E>
{
    Ok(T Value),
    Error(E Err)
}
```

This opens up an entire style of error handling that doesn't rely on exceptions for expected failure cases — something that's been standard practice in Rust and functional languages for years.

### Unions with Methods

The proposal also allows unions to have methods, computed properties, and implement interfaces, just like any other type:

```csharp
union Shape
{
    Circle(double Radius),
    Rectangle(double Width, double Height),
    Triangle(double Base, double Height);

    public double Area => this switch
    {
        Circle(var r) => Math.PI * r * r,
        Rectangle(var w, var h) => w * h,
        Triangle(var b, var h) => 0.5 * b * h
    };

    public double Perimeter => this switch
    {
        Circle(var r) => 2 * Math.PI * r,
        Rectangle(var w, var h) => 2 * (w + h),
        Triangle(var b, var h) => b + h + Math.Sqrt(b * b + h * h)
    };
}
```

Notice how the `switch` expression inside `Area` and `Perimeter` doesn't need a default arm. The compiler knows the union is exhaustive — there are only three variants, and all three are handled. If you add a fourth variant later, the compiler will flag every `switch` that doesn't handle it.

## Pattern Matching Integration

Pattern matching has been evolving in C# since version 7.0, and union types are designed to be a first-class citizen of that system.

### Exhaustive Switch Expressions

The most impactful feature is exhaustive switch checking. With unions, the compiler **knows** all possible cases:

```csharp
string Describe(Shape shape) => shape switch
{
    Circle(var r)         => $"A circle with radius {r}",
    Rectangle(var w, var h) => $"A {w}x{h} rectangle",
    Triangle(var b, var h)  => $"A triangle with base {b} and height {h}"
};
```

No discard arm. No `_ => throw new NotImplementedException()`. If you forget a case, the compiler emits an error, not a warning. This is a fundamental improvement in safety.

### Nested Pattern Matching

Unions compose naturally with nested patterns:

```csharp
union Expr
{
    Literal(double Value),
    Add(Expr Left, Expr Right),
    Multiply(Expr Left, Expr Right),
    Negate(Expr Inner)
}

double Evaluate(Expr expr) => expr switch
{
    Literal(var v)          => v,
    Add(var left, var right) => Evaluate(left) + Evaluate(right),
    Multiply(var left, var right) => Evaluate(left) * Evaluate(right),
    Negate(var inner)        => -Evaluate(inner)
};
```

This kind of recursive data structure is extremely common in compilers, interpreters, rules engines, and mathematical modeling. Today in C# you'd need a deep class hierarchy and the visitor pattern. With unions, the code is dramatically simpler.

### Guard Clauses

Pattern matching with unions supports `when` guards just as you'd expect:

```csharp
string Classify(Shape shape) => shape switch
{
    Circle(var r) when r > 100   => "Large circle",
    Circle(var r) when r > 10    => "Medium circle",
    Circle(_)                     => "Small circle",
    Rectangle(var w, var h) when w == h => "Square",
    Rectangle _                   => "Rectangle",
    Triangle _                    => "Triangle"
};
```

## Practical Examples

Let me walk through some real-world scenarios where union types dramatically improve the code.

### The Result Pattern: Replacing Exceptions for Expected Errors

One of the most common patterns in modern application development is representing operations that can succeed or fail without using exceptions for control flow. Exceptions should be exceptional — things like network failures or out-of-memory conditions. A validation error or a "not found" result is an expected outcome, not an exception.

```csharp
union Result<T, E>
{
    Ok(T Value),
    Error(E Err)
}

union OrderError
{
    NotFound(Guid OrderId),
    InsufficientStock(string ProductId, int Requested, int Available),
    PaymentDeclined(string Reason),
    ValidationFailed(IReadOnlyList<string> Errors)
}

Result<Order, OrderError> ProcessOrder(OrderRequest request)
{
    if (!Validate(request, out var errors))
        return new ValidationFailed(errors);

    var product = catalog.Find(request.ProductId);
    if (product is null)
        return new NotFound(request.OrderId);

    if (product.Stock < request.Quantity)
        return new InsufficientStock(request.ProductId, request.Quantity, product.Stock);

    var paymentResult = paymentGateway.Charge(request.Payment);
    if (!paymentResult.Success)
        return new PaymentDeclined(paymentResult.Message);

    var order = CreateOrder(request, product);
    return new Ok(order);
}
```

The caller is then forced to handle every possible outcome:

```csharp
var result = ProcessOrder(request);

var response = result switch
{
    Ok(var order) => Results.Created($"/orders/{order.Id}", order),
    Error(NotFound(var id)) => Results.NotFound($"Order {id} not found"),
    Error(InsufficientStock(var pid, var req, var avail)) =>
        Results.Conflict($"Product {pid}: requested {req}, only {avail} available"),
    Error(PaymentDeclined(var reason)) =>
        Results.UnprocessableEntity($"Payment declined: {reason}"),
    Error(ValidationFailed(var errors)) =>
        Results.BadRequest(new { Errors = errors })
};
```

No try-catch blocks, no forgotten exception types, no runtime surprises. Every failure mode is visible in the type signature and enforced by the compiler. This is a massive improvement for API reliability.

### Domain Modeling: Payment Types

Here's a real-world domain modeling example — handling different payment methods in an e-commerce system:

```csharp
union PaymentMethod
{
    CreditCard(string CardNumber, string Expiry, string Cvv),
    BankTransfer(string Iban, string Bic),
    DigitalWallet(string Provider, string Token),
    CashOnDelivery
}

decimal CalculateProcessingFee(PaymentMethod method, decimal amount) => method switch
{
    CreditCard _                         => amount * 0.029m + 0.30m,
    BankTransfer _                       => 1.50m,
    DigitalWallet(provider: "PayPal", _) => amount * 0.034m + 0.35m,
    DigitalWallet _                      => amount * 0.025m,
    CashOnDelivery                       => 4.99m
};

string FormatForReceipt(PaymentMethod method) => method switch
{
    CreditCard(var num, _, _)       => $"Credit Card ending in {num[^4..]}",
    BankTransfer(var iban, _)       => $"Bank Transfer ({iban[..4]}****)",
    DigitalWallet(var provider, _)  => $"Digital Wallet ({provider})",
    CashOnDelivery                  => "Cash on Delivery"
};
```

Compare this to the current approach where you'd have an interface or abstract class with five different implementations spread across five files, possibly with a visitor pattern on top. The union approach keeps the data definition and the operations together, readable, and exhaustively checked.

### State Machines

State machines are everywhere in software — order processing, workflow engines, connection management, UI state. Unions make them explicit and safe:

```csharp
union ConnectionState
{
    Disconnected,
    Connecting(string Host, int Port, DateTime StartedAt),
    Connected(string Host, int Port, TcpClient Client),
    Reconnecting(string Host, int Port, int Attempt, TimeSpan Delay),
    Failed(string Host, Exception Error)
}

ConnectionState HandleEvent(ConnectionState state, ConnectionEvent evt) => (state, evt) switch
{
    (Disconnected, Connect(var host, var port)) =>
        new Connecting(host, port, DateTime.UtcNow),

    (Connecting(var h, var p, _), ConnectionSucceeded(var client)) =>
        new Connected(h, p, client),

    (Connecting(var h, var p, _), ConnectionFailed(var ex)) =>
        new Reconnecting(h, p, Attempt: 1, Delay: TimeSpan.FromSeconds(1)),

    (Reconnecting(var h, var p, var attempt, _), ConnectionSucceeded(var client)) =>
        new Connected(h, p, client),

    (Reconnecting(var h, var p, var attempt, _), ConnectionFailed(var ex)) when attempt >= 5 =>
        new Failed(h, ex),

    (Reconnecting(var h, var p, var attempt, _), ConnectionFailed(_)) =>
        new Reconnecting(h, p, attempt + 1, TimeSpan.FromSeconds(Math.Pow(2, attempt))),

    (Connected(_, _, var client), Disconnect) =>
        new Disconnected,

    _ => state // Ignore events that don't apply to current state
};
```

Each state carries exactly the data relevant to that state. You can't accidentally access a `TcpClient` when you're in the `Connecting` state because it doesn't exist on that variant. The type system enforces the state machine's invariants.

## Serialization and Interop Considerations

One of the practical questions that comes up immediately with union types is: how do you serialize them? If you're building APIs or storing data, you need JSON serialization to work correctly.

The design team has been discussing built-in `System.Text.Json` support for union types. The expected approach involves serializing with a discriminator property:

```json
{
  "$type": "Circle",
  "radius": 5.0
}
```

```json
{
  "$type": "Rectangle",
  "width": 10.0,
  "height": 20.0
}
```

This is consistent with the existing polymorphic serialization support introduced in .NET 7 with `JsonDerivedType` attributes. The expectation is that unions would work with `System.Text.Json` out of the box, using the variant name as the type discriminator by default.

For Entity Framework Core, the likely approach is storing union values using a discriminator column — similar to how table-per-hierarchy (TPH) inheritance mapping already works. The exact EF Core integration is still being designed, but the infrastructure for handling closed type hierarchies already exists.

It's worth noting that interop with other .NET languages should be smooth, since unions will compile down to standard IL class hierarchies under the hood. F# code consuming a C# union would see it as a standard type hierarchy, and vice versa.

## How to Try It

As of the time of writing, union types are available as a preview feature in the latest .NET SDK previews. To experiment with the proposed syntax, you'll need to:

1. Install the latest .NET preview SDK
2. Enable the preview language version in your project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>preview</LangVersion>
  </PropertyGroup>
</Project>
```

Keep in mind that preview features are subject to change. The syntax, behavior, and compiler diagnostics may evolve significantly before the final release. Don't ship production code relying on preview language features — but absolutely do experiment with them and provide feedback. The C# team actively monitors the [csharplang](https://github.com/dotnet/csharplang) repository discussions.

If you want to follow the proposal's progress, the key places to watch are:

- The [dotnet/csharplang](https://github.com/dotnet/csharplang) repository for language design discussions
- The [dotnet/roslyn](https://github.com/dotnet/roslyn) repository for compiler implementation progress
- The .NET blog for official announcements

## Comparison with Existing Approaches

Let me put together a quick comparison so you can see how the different approaches stack up:

| Feature | Abstract Records | OneOf<T1,T2> | Enum + Data | Union Types |
|---|---|---|---|---|
| Exhaustiveness checking | ❌ No | ✅ Yes | ❌ No | ✅ Yes |
| Pattern matching | ✅ Yes | ❌ Limited | ❌ Manual | ✅ Native |
| Compiler-enforced closure | ❌ No | ✅ Yes | ❌ No | ✅ Yes |
| Data per variant | ✅ Yes | ✅ Yes | ⚠️ Fragile | ✅ Yes |
| Readability | ⚠️ Verbose | ⚠️ Positional | ❌ Poor | ✅ Excellent |
| Serialization | ✅ Manual | ⚠️ Complex | ✅ Manual | ✅ Built-in |
| Boilerplate | ⚠️ Moderate | ✅ Low | ⚠️ High | ✅ Minimal |
| No external dependency | ✅ Yes | ❌ NuGet | ✅ Yes | ✅ Yes |

The union approach wins in nearly every category. It's not that the other approaches are bad — they were the best we had. But a first-class language feature can integrate with the compiler, the pattern matching system, and the serialization infrastructure in ways that library solutions simply can't.

## What This Means for the .NET Ecosystem

The introduction of discriminated unions will ripple across the entire .NET ecosystem. Here's what I expect to see:

**Library design will improve.** APIs that currently return `null` to indicate "not found" or throw exceptions for validation failures will be able to return `Result<T, E>` types instead. This makes failure modes explicit in the type signature — you can see what can go wrong by looking at the method signature, not by reading documentation or source code.

**Domain modeling becomes more expressive.** The gap between the problem domain and the code representation shrinks dramatically. When your domain expert says "a payment can be a credit card, a bank transfer, or cash on delivery," you can model that directly as a union rather than translating it into an inheritance hierarchy.

**F# ideas become accessible to C# developers.** Many C# developers have admired F#'s type system from a distance but haven't been able to adopt F# in their organizations. Union types bring one of F#'s most powerful features to C#, which is a win for the entire .NET ecosystem.

**Fewer runtime errors.** The exhaustiveness checking alone will prevent entire categories of bugs. Every time you add a new variant to a union, the compiler will guide you to every place in the codebase that needs updating. No more forgotten `switch` cases, no more `NotImplementedException` in default branches that only surface in production.

## Conclusion

Discriminated unions have been at the top of the C# community's wish list for nearly a decade, and for good reason. They solve a fundamental gap in the type system — the ability to model data that can be "one of several things" with compiler-enforced safety.

The proposed `union` keyword brings a clean, concise syntax that integrates deeply with C#'s pattern matching, works with generics, supports methods and interfaces, and enables exhaustive checking that catches bugs at compile time rather than runtime.

Whether you're building domain models, designing APIs with explicit error types, implementing state machines, or just trying to replace awkward inheritance hierarchies with something more natural — union types are going to change how you write C#.

We've been waiting a long time for this. The syntax is elegant, the integration with existing language features is thoughtful, and the practical impact will be felt across the entire .NET ecosystem.

Keep an eye on the preview releases, experiment with the feature, and provide feedback to the language design team. This is one of those features that, once you've used it, you'll wonder how you ever lived without it.
