---
title: "Authentication and Authorization in Blazor: A Practical Guide"
date: 2025-07-12T00:00:00Z
draft: false
tags: ["Blazor", ".NET", "Security", "Web Development"]
slug: "blazor-authentication-authorization"
description: "A practical guide to implementing authentication and authorization in Blazor apps — from ASP.NET Identity to OAuth, role-based access, and securing components."
---

If you've worked with ASP.NET MVC or Razor Pages, you probably have a mental model for how authentication works: middleware intercepts the request, checks a cookie or token, populates `HttpContext.User`, and you're off to the races. Blazor changes that mental model in ways that are subtle but important — and if you don't understand those differences early, you'll end up debugging auth issues that feel impossible.

In this post, I want to walk through how authentication and authorization actually work in Blazor, covering both Server and WebAssembly hosting models. We'll go from the fundamentals all the way through custom providers, external OAuth, and the pitfalls I've seen trip up even experienced .NET developers.

## Why Auth in Blazor Is Different

In traditional ASP.NET, every user interaction is an HTTP request. The server validates credentials, sets a cookie, and every subsequent request carries that cookie. The auth pipeline is linear and predictable.

Blazor Server operates over a persistent SignalR connection. After the initial HTTP request loads the page, all subsequent interactions happen over WebSockets. There's no new HTTP request for each button click, so middleware doesn't re-execute on every interaction. The `HttpContext` is available during the initial connection, but relying on it throughout the lifetime of a circuit is a recipe for bugs.

Blazor WebAssembly runs entirely in the browser. There's no server-side `HttpContext` at all. Authentication state must be fetched from an API, stored client-side, and managed through tokens — typically JWTs. The server trusts the client only as far as the token validation goes.

This means Blazor needs its own abstraction for authentication state, one that works regardless of hosting model. That abstraction is the `AuthenticationStateProvider`.

## Authentication State: The Foundation

At the heart of Blazor's auth system is `AuthenticationStateProvider`. This is an abstract class that exposes a single critical method:

```csharp
public abstract Task<AuthenticationState> GetAuthenticationStateAsync();
```

The `AuthenticationState` object wraps a `ClaimsPrincipal` — the same identity model used throughout .NET. Components don't talk to cookies or tokens directly; they ask the `AuthenticationStateProvider` for the current state.

To make this state available to your entire component tree, Blazor provides `CascadingAuthenticationState`. You typically wrap your router with it in `App.razor` or your layout:

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData"
                                DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    <p>You're not authorized to view this page.</p>
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(MainLayout)">
                <p>Page not found.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

The `AuthorizeRouteView` is doing double duty here: it checks whether the user is authenticated and authorized before rendering the matched page component, and it provides a fallback UI when they're not.

In .NET 8 and later with the unified Blazor model, you'll configure this in your `App.razor` and the framework handles the cascading parameter automatically when you use `AddCascadingAuthenticationState()` in your service registration.

## ASP.NET Identity Integration

For most projects, you don't need to build auth from scratch. ASP.NET Identity gives you user management, password hashing, two-factor authentication, and account confirmation out of the box.

Setting it up with Blazor starts in `Program.cs`:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    });
```

With the Blazor Web App template in .NET 8+, the scaffolded Identity UI uses Razor components directly. You get login, registration, and account management pages that integrate naturally with the rest of your Blazor application — no more awkward mix of Razor Pages and Blazor components.

The `ApplicationDbContext` inherits from `IdentityDbContext` and you'll need to run migrations to create the identity tables:

```csharp
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
}
```

```bash
dotnet ef migrations add InitialIdentity
dotnet ef database update
```

## The AuthorizeView Component

Once authentication state is flowing through your component tree, `AuthorizeView` lets you conditionally render UI:

```razor
<AuthorizeView>
    <Authorized>
        <p>Welcome, @context.User.Identity?.Name!</p>
        <a href="/account/manage">Manage Account</a>
        <form method="post" action="/account/logout">
            <button type="submit">Log Out</button>
        </form>
    </Authorized>
    <NotAuthorized>
        <a href="/account/login">Log In</a>
        <a href="/account/register">Register</a>
    </NotAuthorized>
</AuthorizeView>
```

The `context` parameter inside `<Authorized>` gives you access to the `AuthenticationState`, so you can inspect claims, roles, and the user's identity directly in your markup.

You can also use `AuthorizeView` with roles and policies:

```razor
<AuthorizeView Roles="Admin,Moderator">
    <Authorized>
        <button @onclick="DeletePost">Delete Post</button>
    </Authorized>
</AuthorizeView>

<AuthorizeView Policy="CanEditArticles">
    <Authorized>
        <button @onclick="EditArticle">Edit</button>
    </Authorized>
</AuthorizeView>
```

One thing to keep in mind: `AuthorizeView` is a UI concern. It hides or shows elements, but it doesn't protect the underlying logic. If someone can call your API endpoint or invoke your method directly, they bypass `AuthorizeView` entirely. Always enforce authorization on the server side too.

## The [Authorize] Attribute

To protect an entire page, apply the `[Authorize]` attribute:

```razor
@page "/admin/dashboard"
@attribute [Authorize(Roles = "Admin")]

<h1>Admin Dashboard</h1>
<p>Only administrators can see this page.</p>
```

When an unauthenticated user navigates to this page, the `AuthorizeRouteView` kicks in and renders the `<NotAuthorized>` template you defined earlier. You can redirect to a login page instead by handling the `NotAuthorized` case with navigation:

```razor
<NotAuthorized>
    @if (!context.User.Identity?.IsAuthenticated ?? true)
    {
        <RedirectToLogin />
    }
    else
    {
        <p>You don't have permission to access this page.</p>
    }
</NotAuthorized>
```

A simple `RedirectToLogin` component might look like:

```razor
@inject NavigationManager Navigation

@code {
    protected override void OnInitialized()
    {
        var returnUrl = Uri.EscapeDataString(Navigation.Uri);
        Navigation.NavigateTo($"/account/login?returnUrl={returnUrl}", forceLoad: true);
    }
}
```

The `forceLoad: true` is important here — you want an actual HTTP navigation so the server-side auth middleware can handle the login flow properly.

## Role-Based and Policy-Based Authorization

Roles are the simplest model: assign users to groups like "Admin" or "Editor," then check membership. But policies give you much more flexibility.

Register policies in `Program.cs`:

```csharp
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("CanPublish", policy =>
        policy.RequireClaim("Permission", "Publish"));

    options.AddPolicy("MinimumAge", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));

    options.AddPolicy("PremiumUser", policy =>
        policy.RequireRole("Premium")
              .RequireClaim("Subscription", "Active"));
});
```

Custom requirements need a handler:

```csharp
public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public int MinimumAge { get; }
    public MinimumAgeRequirement(int minimumAge) => MinimumAge = minimumAge;
}

public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        var dateOfBirthClaim = context.User.FindFirst("DateOfBirth");

        if (dateOfBirthClaim is null)
            return Task.CompletedTask;

        var dateOfBirth = DateOnly.Parse(dateOfBirthClaim.Value);
        var age = DateOnly.FromDateTime(DateTime.Today).Year - dateOfBirth.Year;

        if (age >= requirement.MinimumAge)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

Register the handler:

```csharp
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
```

In components, you can also check authorization programmatically when you need dynamic logic:

```razor
@inject IAuthorizationService AuthorizationService
@inject AuthenticationStateProvider AuthStateProvider

@code {
    private bool canPublish;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var result = await AuthorizationService.AuthorizeAsync(
            authState.User, "CanPublish");
        canPublish = result.Succeeded;
    }
}
```

## External OAuth Providers

Supporting "Sign in with Google" or "Sign in with GitHub" is straightforward with the ASP.NET authentication middleware. These are configured server-side since the OAuth flow requires HTTP redirects.

```csharp
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Auth:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"]!;
        options.Scope.Add("profile");
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["Auth:Microsoft:ClientId"]!;
        options.ClientSecret = builder.Configuration["Auth:Microsoft:ClientSecret"]!;
    })
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Auth:GitHub:ClientId"]!;
        options.ClientSecret = builder.Configuration["Auth:GitHub:ClientSecret"]!;
    });
```

For GitHub, you'll need the `AspNet.Security.OAuth.GitHub` NuGet package, as it's not included in the default ASP.NET libraries.

The login UI then provides links that trigger the external challenge:

```razor
@page "/account/external-login"

<h2>Sign in with an external provider</h2>

<form method="post" action="/api/auth/external-login">
    <button type="submit" name="provider" value="Google">
        Sign in with Google
    </button>
    <button type="submit" name="provider" value="Microsoft">
        Sign in with Microsoft
    </button>
    <button type="submit" name="provider" value="GitHub">
        Sign in with GitHub
    </button>
</form>
```

The API endpoint triggers the challenge and handles the callback:

```csharp
app.MapPost("/api/auth/external-login", (string provider, HttpContext context) =>
{
    var properties = new AuthenticationProperties
    {
        RedirectUri = "/api/auth/external-callback"
    };
    return Results.Challenge(properties, [provider]);
});
```

External auth in Blazor always requires a full page navigation — you can't complete an OAuth redirect inside a SignalR circuit or a WebAssembly app without going through the server.

## Token-Based Auth in Blazor WebAssembly

Blazor WebAssembly runs on the client, so cookie-based auth doesn't apply in the same way. Instead, you typically use JWTs stored in memory and attached to outgoing HTTP requests.

The framework provides `AuthorizationMessageHandler` to attach tokens automatically:

```csharp
builder.Services.AddHttpClient("API",
    client => client.BaseAddress = new Uri("https://api.example.com"))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
```

For standalone Blazor WASM apps that authenticate against their own API, you'll implement a custom `AuthenticationStateProvider` that parses the JWT:

```csharp
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;

    public JwtAuthenticationStateProvider(
        ILocalStorageService localStorage,
        HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyAuthStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];

        var padded = payload.Length % 4 switch
        {
            2 => payload + "==",
            3 => payload + "=",
            _ => payload
        };

        var bytes = Convert.FromBase64String(padded);
        var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bytes);

        return json?.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())) 
               ?? Enumerable.Empty<Claim>();
    }
}
```

After a successful login, you store the token and notify the auth state:

```csharp
public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly JwtAuthenticationStateProvider _authStateProvider;

    public AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = (JwtAuthenticationStateProvider)authStateProvider;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login",
            new { Email = email, Password = password });

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content
            .ReadFromJsonAsync<LoginResponse>();

        await _localStorage.SetItemAsync("authToken", result!.Token);
        _authStateProvider.NotifyAuthStateChanged();

        return true;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _authStateProvider.NotifyAuthStateChanged();
    }
}
```

A word of caution: storing JWTs in `localStorage` exposes them to XSS attacks. For higher-security applications, consider keeping tokens in memory only and using refresh tokens, or adopting the Backend-for-Frontend (BFF) pattern where the server manages tokens and the client uses HTTP-only cookies.

## Blazor Server vs. WebAssembly: Security Considerations

The hosting model fundamentally changes your security posture.

**Blazor Server** keeps all your component logic on the server. The client only sees rendered HTML diffs over SignalR. This means:

- Sensitive logic never leaves the server
- You can access databases and internal services directly from components
- Authentication state comes from the server's `HttpContext` on initial connection
- The circuit can outlive the auth cookie — if a user's cookie expires, the circuit stays alive until disconnected
- You should handle circuit disconnection gracefully and re-validate auth state on reconnection

**Blazor WebAssembly** runs entirely in the browser. This means:

- All your component code is downloadable and inspectable
- Never put secrets, connection strings, or sensitive business logic in WASM components
- Auth is only enforced on the client for UX; the real enforcement must happen at your API layer
- Token management is your responsibility
- Consider using the hosted model where a server project handles auth and serves the WASM app

A pattern I recommend for WebAssembly apps is to treat every component as if it's "untrusted UI" and every API endpoint as if it's being called by an unknown client. Validate everything server-side regardless of what the client checks.

## Building a Custom AuthenticationStateProvider

Sometimes the built-in providers don't fit your architecture. Maybe you're integrating with a legacy auth system, or you need to poll for auth state changes. Here's a more complete custom provider for Blazor Server:

```csharp
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;

    public CustomAuthStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IUserService userService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity()));

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity()));

        var user = await _userService.GetUserWithClaimsAsync(userId);

        if (user is null)
            return new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email)
        };

        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(user.Permissions.Select(p => new Claim("Permission", p)));

        var identity = new ClaimsIdentity(claims, "Custom");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void MarkUserAsAuthenticated(string userId)
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void MarkUserAsLoggedOut()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymous));
        NotifyAuthenticationStateChanged(authState);
    }
}
```

Register it in `Program.cs`:

```csharp
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();
```

The key insight here is `NotifyAuthenticationStateChanged` — calling it triggers the cascading parameter to update, which re-evaluates every `AuthorizeView` and `AuthorizeRouteView` in your component tree. This is how you make the UI react to login/logout events without a full page refresh.

## Common Pitfalls and Solutions

After working with Blazor auth across many projects, here are the issues I see most often:

### 1. Using HttpContext in Blazor Server Components

`HttpContext` is available during the initial HTTP request, but it's `null` or stale during SignalR interactions. Don't inject `IHttpContextAccessor` into components that run after the initial render.

**Solution:** Capture what you need from `HttpContext` during initialization and store it in a scoped service:

```csharp
public class UserContext
{
    public string? UserId { get; set; }
    public string? AccessToken { get; set; }
}

// In a component that renders during the initial HTTP request:
@inject IHttpContextAccessor HttpContextAccessor
@inject UserContext UserContext

@code {
    protected override void OnInitialized()
    {
        var context = HttpContextAccessor.HttpContext;
        UserContext.UserId = context?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        UserContext.AccessToken = context?.Request.Headers.Authorization
            .ToString().Replace("Bearer ", "");
    }
}
```

### 2. Auth State Not Updating After Login

You call your login API, it succeeds, but the UI still shows "Log In."

**Solution:** You must call `NotifyAuthenticationStateChanged` on your `AuthenticationStateProvider` after the auth state changes. The framework doesn't magically detect that a token was stored or a cookie was set.

### 3. Authorize Attribute Not Working on Components

You add `[Authorize]` to a component but it doesn't block unauthenticated users.

**Solution:** Make sure you're using `AuthorizeRouteView` instead of plain `RouteView` in your `App.razor`. The standard `RouteView` ignores authorization attributes entirely.

### 4. Prerendering Breaks Auth State

During server-side prerendering in Blazor WebAssembly, there's no auth token available. Components render as unauthenticated, then flicker to authenticated state after WASM loads.

**Solution:** Either disable prerendering for auth-sensitive pages with `@rendermode InteractiveWebAssembly` (without prerender), or handle the loading state gracefully:

```razor
<AuthorizeView>
    <Authorized>
        <p>Welcome back, @context.User.Identity?.Name</p>
    </Authorized>
    <Authorizing>
        <p>Loading...</p>
    </Authorizing>
    <NotAuthorized>
        <a href="/login">Sign in</a>
    </NotAuthorized>
</AuthorizeView>
```

### 5. Token Expiration in Long-Running Circuits

Blazor Server circuits can stay alive for hours. If your token or session expires, the user stays "authenticated" in the UI but API calls start failing.

**Solution:** Implement a periodic check or use a `RevalidatingServerAuthenticationStateProvider`:

```csharp
public class RevalidatingAuthStateProvider
    : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RevalidatingAuthStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<IdentityUser>>();

        var user = await userManager.GetUserAsync(authenticationState.User);
        return user is not null;
    }
}
```

This validates the user still exists (and their security stamp hasn't changed) every 30 minutes.

## Conclusion

Authentication and authorization in Blazor require a shift in thinking from traditional request-response ASP.NET. The `AuthenticationStateProvider` abstraction is the key to understanding how it all fits together — once you internalize that, the rest follows naturally.

For most applications, start with ASP.NET Identity and the built-in templates. They handle the heavy lifting of user management, password hashing, and token generation. Layer on policies and claims-based authorization as your requirements grow. Add external OAuth providers when your users expect them.

The hosting model matters: Blazor Server gives you a more traditional security posture where code stays on the server, while WebAssembly pushes you toward API-first thinking where the client is untrusted by design. Neither is inherently more secure — they just have different threat models.

Whatever approach you choose, remember the golden rule: **authorization in the UI is for user experience, authorization on the server is for security.** Always enforce both.
