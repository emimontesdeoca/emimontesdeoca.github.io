---
title: "Blazor Interactivity in .NET 9 and .NET 10: A Complete Guide"
date: 2025-06-15T00:00:00Z
draft: false
tags: ["Blazor", ".NET", "Web Development"]
slug: "blazor-interactivity-dotnet-9-10"
description: "A deep dive into Blazor's render modes, streaming SSR, enhanced navigation, and the new interactivity features in .NET 9 and .NET 10."
---

If you've been building web applications with Blazor over the past few years, you know the framework has come a *long* way. What started as a choice between Blazor Server and Blazor WebAssembly has evolved into a unified, flexible rendering model that lets you pick the right interactivity strategy for each component in your application.

With .NET 8 we got the foundational shift — the introduction of render modes and static server-side rendering (SSR) as the default. Now, .NET 9 and .NET 10 build on top of that foundation with refinements that make the developer experience smoother and the end-user experience faster.

In this post, I want to walk you through the full picture of Blazor interactivity as it stands today: how render modes work, what streaming SSR brings to the table, how enhanced navigation and form handling change the game, and what's new in the latest releases. If you're planning a new Blazor project or thinking about upgrading, this is the guide I wish I had when I started digging into all of this.

## A Quick Look Back: How We Got Here

Before .NET 8, you had to commit to a hosting model at the project level. Blazor Server meant everything ran on the server over a SignalR connection. Blazor WebAssembly meant everything ran in the browser. Each had trade-offs, and mixing them was painful.

.NET 8 changed the game by introducing a single project template — the Blazor Web App — that unifies both models. The key concept is **render modes**: you decide *per component* how it should render and where interactivity happens. The default became static SSR, meaning components render on the server and send plain HTML to the browser — no SignalR, no WebAssembly, just fast HTML.

.NET 9 polished these concepts, improved the developer experience, and optimized performance. .NET 10 takes it further with better reconnection handling, persistent component state across render modes, and improvements to how the Blazor script itself is delivered.

Let's break it all down.

## Render Modes in .NET 9

At the heart of modern Blazor is the concept of render modes. There are four modes you should know about:

### 1. Static SSR (the default)

When you create a new Blazor Web App, components render statically on the server by default. The server processes the Razor component, generates HTML, and sends it to the browser. There's no persistent connection, no WebAssembly runtime — just traditional request/response.

This is perfect for content-heavy pages, dashboards that mostly display data, or any page where you don't need real-time interaction.

```razor
@page "/products"

<h1>Our Products</h1>

@foreach (var product in products)
{
    <div class="product-card">
        <h3>@product.Name</h3>
        <p>@product.Description</p>
        <span class="price">@product.Price.ToString("C")</span>
    </div>
}

@code {
    private List<Product> products = new();

    protected override async Task OnInitializedAsync()
    {
        products = await ProductService.GetAllAsync();
    }
}
```

This component renders on the server, produces HTML, and that's it. No ongoing connection. Fast initial load, great for SEO, minimal server resource usage.

### 2. Interactive Server

When you need real-time interactivity — handling button clicks, processing user input, updating the UI dynamically — you can opt into Interactive Server mode. This establishes a SignalR connection between the browser and the server, and UI updates happen over that connection.

```razor
@page "/counter"
@rendermode InteractiveServer

<h1>Counter</h1>

<p>Current count: @currentCount</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}
```

The `@rendermode InteractiveServer` directive is all it takes. The component renders initially on the server, and then a SignalR connection is established to handle subsequent interactions. Your C# event handlers run on the server, and the UI diffs are sent to the browser.

**When to use it:** When you need interactivity, your components need access to server-side resources (databases, APIs, file systems), and you want a fast initial load without waiting for WebAssembly to download.

### 3. Interactive WebAssembly

If you want interactivity without maintaining a server connection, Interactive WebAssembly runs your component logic directly in the browser using the .NET WebAssembly runtime.

```razor
@page "/search"
@rendermode InteractiveWebAssembly

<h1>Product Search</h1>

<input @bind="searchTerm" @bind:event="oninput" placeholder="Search products..." />

@if (filteredProducts.Any())
{
    <ul>
        @foreach (var product in filteredProducts)
        {
            <li>@product.Name — @product.Price.ToString("C")</li>
        }
    </ul>
}

@code {
    private string searchTerm = string.Empty;
    private List<Product> allProducts = new();
    private IEnumerable<Product> filteredProducts => allProducts
        .Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        allProducts = await Http.GetFromJsonAsync<List<Product>>("api/products") ?? new();
    }
}
```

The trade-off: there's an initial download cost for the .NET runtime and your assemblies. But once loaded, the component runs entirely in the browser with no server round-trips for UI interactions.

**When to use it:** For highly interactive components where latency matters (think: rich text editors, drawing tools, real-time filtering), when you want to reduce server load, or when you're building a progressive web app (PWA).

### 4. Interactive Auto

This is the pragmatic middle ground and one of my favorite features. Interactive Auto starts with server-side rendering via SignalR for the first load, then silently downloads the WebAssembly runtime in the background. On subsequent visits, the component runs on WebAssembly.

```razor
@page "/dashboard"
@rendermode InteractiveAuto

<h1>Dashboard</h1>

<DashboardWidget Title="Sales" Value="@salesTotal" />
<DashboardWidget Title="Users" Value="@activeUsers" />

<button @onclick="RefreshData">Refresh</button>

@code {
    private decimal salesTotal;
    private int activeUsers;

    protected override async Task OnInitializedAsync()
    {
        await RefreshData();
    }

    private async Task RefreshData()
    {
        var data = await DashboardService.GetSummaryAsync();
        salesTotal = data.SalesTotal;
        activeUsers = data.ActiveUsers;
    }
}
```

This gives you the best of both worlds: fast first render (no waiting for WASM download), and eventually the component runs client-side. The user doesn't notice the transition.

**When to use it:** When you want both fast initial loads and eventual client-side execution. It's a great default choice for many interactive components.

## Streaming SSR: The Best of Both Worlds

Streaming SSR is one of those features that sounds simple but makes a massive difference in perceived performance. Here's the idea: instead of waiting for all your data to load before sending any HTML, the server sends the page shell immediately and then *streams* content updates as data becomes available.

```razor
@page "/reports"
@attribute [StreamRendering]

<h1>Monthly Reports</h1>

@if (reports is null)
{
    <div class="loading-spinner">
        <p>Loading reports...</p>
    </div>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Month</th>
                <th>Revenue</th>
                <th>Growth</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var report in reports)
            {
                <tr>
                    <td>@report.Month</td>
                    <td>@report.Revenue.ToString("C")</td>
                    <td class="@(report.Growth >= 0 ? "text-success" : "text-danger")">
                        @report.Growth.ToString("P1")
                    </td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private List<MonthlyReport>? reports;

    protected override async Task OnInitializedAsync()
    {
        // This might take a couple of seconds
        reports = await ReportService.GetMonthlyReportsAsync();
    }
}
```

With the `[StreamRendering]` attribute, here's what happens:

1. The server immediately sends the HTML with the loading spinner.
2. `OnInitializedAsync` runs and fetches the data.
3. When the data arrives, the server streams the updated HTML (the table) to the browser.
4. The browser patches the DOM without a full page reload.

The user sees the page *instantly* with a loading indicator, then the content fills in. No JavaScript framework needed. No WebSocket connection. Just clever use of HTTP streaming.

**When to use it:** Any static SSR page that fetches data during rendering. Product listing pages, dashboards, reports — anywhere the initial data load might take more than a few hundred milliseconds.

**When NOT to use it:** If the data loads extremely fast (under 100ms), the streaming overhead isn't worth it. Also, streaming SSR doesn't give you ongoing interactivity — for that, you still need an interactive render mode.

## Enhanced Navigation and Form Handling

One of the subtle but powerful improvements in modern Blazor is enhanced navigation. By default, Blazor intercepts internal link clicks and form submissions, fetching the new page content via `fetch` and patching the DOM rather than doing a full browser navigation.

This means navigating between static SSR pages feels like an SPA — no full page flash, scroll position can be preserved, and the experience is silky smooth.

### How It Works

When the Blazor script (`blazor.web.js`) is loaded, it automatically intercepts clicks on internal links. Instead of a traditional browser navigation, it:

1. Makes a `fetch` request to the target URL.
2. Receives the HTML response.
3. Merges the new content into the existing DOM.
4. Updates the browser's URL and history.

You don't have to do anything to enable this — it's on by default. But you can control it:

```html
<!-- Disable enhanced navigation for a specific link -->
<a href="/legacy-page" data-enhance-nav="false">Legacy Page</a>

<!-- Force a full page reload for external-like behavior -->
<a href="/downloads/report.pdf" data-enhance-nav="false">Download Report</a>
```

### Enhanced Form Handling

Forms get the same treatment. When you use `EditForm` or the standard `<form>` element with Blazor's form handling, submissions are intercepted and handled via `fetch`:

```razor
@page "/contact"

<EditForm Model="contactModel" OnValidSubmit="HandleSubmit" FormName="contact" Enhance>
    <DataAnnotationsValidator />

    <div class="mb-3">
        <label for="name">Name</label>
        <InputText id="name" @bind-Value="contactModel.Name" class="form-control" />
        <ValidationMessage For="() => contactModel.Name" />
    </div>

    <div class="mb-3">
        <label for="email">Email</label>
        <InputText id="email" @bind-Value="contactModel.Email" class="form-control" />
        <ValidationMessage For="() => contactModel.Email" />
    </div>

    <div class="mb-3">
        <label for="message">Message</label>
        <InputTextArea id="message" @bind-Value="contactModel.Message" class="form-control" />
        <ValidationMessage For="() => contactModel.Message" />
    </div>

    <button type="submit" class="btn btn-primary">Send</button>
</EditForm>

@code {
    [SupplyParameterFromForm]
    private ContactModel contactModel { get; set; } = new();

    private async Task HandleSubmit()
    {
        await ContactService.SubmitAsync(contactModel);
        contactModel = new ContactModel();
    }
}
```

The `Enhance` attribute on `EditForm` tells Blazor to intercept the form submission. The form posts to the server, the server processes it and re-renders the page, and the updated HTML is streamed back — all without a full page navigation. It feels interactive, but it's completely server-rendered.

Notice the `[SupplyParameterFromForm]` attribute — this is how Blazor binds posted form data to your model in static SSR scenarios. It's the bridge between traditional form posts and Blazor's component model.

## Per-page and Per-component Interactivity

One of the most powerful aspects of the new model is that you can mix render modes within a single application. Your product listing page can be static SSR, your shopping cart can be Interactive Server, and your product configurator can be Interactive WebAssembly — all in the same app.

### Setting Render Modes at the Component Level

You can set the render mode directly on a component using the `@rendermode` directive:

```razor
@* This component is interactive via Server *@
@rendermode InteractiveServer

<h3>Live Chat</h3>
<!-- chat UI here -->
```

Or you can set it when using a component from a parent:

```razor
@page "/product/{Id:int}"

<h1>@product?.Name</h1>
<p>@product?.Description</p>

<!-- This child component gets its own interactive render mode -->
<ProductConfigurator Product="product" @rendermode="InteractiveWebAssembly" />

<!-- This stays static -->
<ProductReviews ProductId="Id" />

@code {
    [Parameter] public int Id { get; set; }
    private Product? product;

    protected override async Task OnInitializedAsync()
    {
        product = await ProductService.GetByIdAsync(Id);
    }
}
```

In this example, the page itself is static SSR. The `ProductConfigurator` component runs on WebAssembly for rich client-side interactivity. The `ProductReviews` component stays static because it's just displaying data.

### Setting Render Modes Globally

If you want all pages to be interactive by default, you can set the render mode at the root level in `App.razor`:

```razor
<Routes @rendermode="InteractiveServer" />
```

This makes every page interactive via Server rendering. Individual components can still override this if needed.

### Important Rules to Remember

There are a few constraints to keep in mind when mixing render modes:

- **A child component cannot have a "more interactive" render mode than its parent.** If a parent is static, children can be static or interactive. But if a parent is Interactive Server, a child can't be Interactive WebAssembly (it would need to be Server or Auto).
- **Interactive components can't directly use scoped services from static parents.** If a component runs on WebAssembly, it can't access server-side `DbContext` instances directly — you'll need an API layer.
- **State doesn't automatically transfer between render modes.** If a component pre-renders on the server and then switches to WebAssembly, you need to handle state persistence explicitly — more on this in the .NET 10 section.

## What's New in .NET 10

.NET 10 continues the incremental improvement approach, focusing on reliability and developer experience. Here are the highlights for Blazor interactivity:

### Improved Reconnection Experience

If you've used Interactive Server mode in production, you've probably encountered the reconnection overlay — that moment when the SignalR connection drops and the user sees a "Reconnecting..." message. In .NET 10, this experience gets significantly better.

The reconnection logic is now smarter about retrying. Instead of a fixed retry interval, it uses a backoff strategy that adapts to the situation. The UI during reconnection is also more polished, and you have better hooks to customize the experience:

```razor
<!-- In your App.razor or layout -->
<div id="components-reconnect-modal">
    <div class="reconnect-visible">
        <p>Connection lost. Attempting to reconnect...</p>
        <div class="spinner"></div>
    </div>
    <div class="reconnect-failed">
        <p>Could not reconnect to the server.</p>
        <button onclick="location.reload()">Reload</button>
    </div>
    <div class="reconnect-rejected">
        <p>Your session has expired.</p>
        <a href="/">Return to Home</a>
    </div>
</div>
```

The framework now also attempts reconnection more aggressively on transient failures, and it can restore the circuit state more reliably. This means fewer "please reload the page" moments for your users.

### Persistent Component State Across Render Modes

This is a big one. In .NET 9, when a component pre-renders on the server and then transitions to WebAssembly (or switches between render modes), the component state is lost. The component effectively re-initializes, which can lead to duplicate data fetches and flickering UI.

.NET 10 improves the `PersistentComponentState` service to work more seamlessly across render mode transitions:

```razor
@page "/weather"
@rendermode InteractiveWebAssembly
@inject PersistentComponentState ApplicationState

<h1>Weather Forecast</h1>

@if (forecasts is null)
{
    <p>Loading...</p>
}
else
{
    @foreach (var forecast in forecasts)
    {
        <div class="forecast-card">
            <h4>@forecast.Date.ToShortDateString()</h4>
            <p>@forecast.Summary — @forecast.TemperatureC°C</p>
        </div>
    }
}

@code {
    private List<WeatherForecast>? forecasts;
    private PersistingComponentStateSubscription persistingSubscription;

    protected override async Task OnInitializedAsync()
    {
        persistingSubscription = ApplicationState.RegisterOnPersisting(PersistData);

        if (!ApplicationState.TryTakeFromJson<List<WeatherForecast>>(
            "weather-forecasts", out var restored))
        {
            // Data wasn't persisted from prerendering — fetch it
            forecasts = await Http.GetFromJsonAsync<List<WeatherForecast>>("api/weather");
        }
        else
        {
            forecasts = restored;
        }
    }

    private Task PersistData()
    {
        ApplicationState.PersistAsJson("weather-forecasts", forecasts);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        persistingSubscription.Dispose();
    }
}
```

With the improvements in .NET 10, this pattern works more reliably. The state serialized during prerendering is properly available when the component initializes in its interactive mode, avoiding double data fetches and the brief flicker users might see.

### Blazor Script as a Static Web Asset

In previous versions, the Blazor JavaScript file (`blazor.web.js`) was served from the framework's internal endpoint. In .NET 10, it's delivered as a static web asset. This might sound like a small change, but it has practical benefits:

- **Better caching:** Static web assets get proper cache headers and fingerprinted URLs, so browsers cache them more effectively.
- **CDN-friendly:** Since it's a regular static file, CDNs can cache and serve it from edge locations.
- **Compression:** Static web asset compression applies automatically, reducing the script size on the wire.

You don't need to change how you reference it — the framework handles the updated path automatically.

## Best Practices: Choosing the Right Render Mode

After working with all these modes across several projects, here's my decision framework:

### Start with Static SSR

Make static SSR your default. Most pages in most applications are primarily about displaying data. Product pages, blog posts, user profiles, settings pages — these don't need real-time interactivity. Static SSR gives you the best performance, lowest resource usage, and simplest mental model.

### Add Interactivity Only Where Needed

Identify the specific components that need to respond to user interactions in real-time. A "like" button, a chat widget, a drag-and-drop interface — these need interactivity. But the page surrounding them probably doesn't.

### Use Interactive Auto as Your Go-To Interactive Mode

When you do need interactivity, Interactive Auto is often the best default choice. It gives you fast initial loads (server rendering) with eventual client-side execution (WebAssembly). The user gets the best of both worlds, and you write your code once.

### Reserve Interactive Server for Specific Cases

Use Interactive Server when:
- Your component needs direct access to server resources (databases, file system, internal APIs).
- WebAssembly download size is a concern and you can't use Auto.
- You need the component to always run on the server for security reasons (e.g., processing sensitive data).

### Use Streaming SSR Generously

If your static SSR pages fetch any data, add `[StreamRendering]`. The cost is minimal and the perceived performance improvement is significant. Users see content appearing progressively rather than staring at a blank page.

### Handle State Transitions Carefully

If you're using Interactive Auto or mixing pre-rendering with WebAssembly, always use `PersistentComponentState` to avoid duplicate data fetches. Your users will thank you for the absence of flickering content.

### Keep the Component Tree in Mind

Remember the render mode hierarchy rules. Plan your component tree so that interactive boundaries make sense. A common pattern is to have a static layout with interactive "islands" embedded where needed:

```razor
@* Layout: Static SSR *@
<header>
    <NavMenu />
    <UserMenu @rendermode="InteractiveServer" /> @* Needs real-time auth state *@
</header>

<main>
    @Body
</main>

<footer>
    <ChatWidget @rendermode="InteractiveAuto" /> @* Rich interactivity *@
</footer>
```

## Conclusion

Blazor's interactivity model in .NET 9 and .NET 10 represents a mature, well-thought-out approach to building web applications. The ability to choose render modes per component, the seamless enhanced navigation, streaming SSR, and the continued improvements in reconnection and state management make it a compelling choice for a wide range of applications.

The key insight is that **interactivity is a spectrum, not a binary choice**. Most of your application can be static. Some parts need server-driven interactivity. A few might benefit from running in the browser. Blazor now lets you make that choice at the finest granularity — the individual component — without fighting the framework.

If you're starting a new project, my advice is simple: create a Blazor Web App, start with static SSR, add `[StreamRendering]` to data-heavy pages, and promote individual components to interactive modes only when you need them. You'll end up with an application that's fast, efficient, and scales well.

Happy coding, and as always, feel free to reach out if you have questions!
