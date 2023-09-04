Whenever I'm building stuff like services, repositories, attributes or whatever to inject into my applications, there's this step that we must do, which is actually adding the services to the application.

This is always the same, you go to `Program.cs`, then proceed to at some part of the file add `builder.Services.AddScoped<MyService>();` in order to inject the service.

Something like this:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Repositories
builder.Services.AddScoped<ARepository>(); // 👀
builder.Services.AddScoped<BRepository>(); // 👀

// Services
builder.Services.AddScoped<AService>(); // 👀
builder.Services.AddScoped<BService>(); // 👀
builder.Services.AddScoped<CService>(); // 👀
builder.Services.AddScoped<DService>(); // 👀

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
```

I mean, it works, but I'm picky, and I don't really like it.

Let's say that we have multiple dependency injections that we want to do, and they are not really the same stuff, in my case this could be something like having a library that contains all the repositories, another library that has all the services and lastly, another library that contains attributes.

In that case, can you imagine the amount of lines we have to add to `Program.cs`.

Let's say that we have a library that contains some services, if we want to include all our services, we have to work with `IServiceCollection`.

So we're going to create a `static class` that will have a `static` method called `AddServices` that returns a `IServiceCollection`.

In this case, it will be named `IServiceCollectionServicesExtensions`.

```csharp
/// <summary>
/// IServiceCollectionServicesExtensions class
/// </summary>
public static class IServiceCollectionServicesExtensions
{
    /// <summary>
    /// AddCoreServices
    /// </summary>
    /// <param cref="IServiceCollection" name="services">Parameter for <see cref="IServiceCollection"/></param>
    /// <returns>An object of type <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddScoped<AService>()
            .AddScoped<BService>()
            .AddScoped<CService>()
            .AddScoped<DService>();

        return services;
    }
}
```

Not only that, we also have another library that include some repositories which are being used by this services, so let's do the same.

```csharp
/// <summary>
/// IServiceCollectionServicesExtensions class
/// </summary>
public static class IServiceCollectionServicesExtensions
{
    /// <summary>
    /// AddCoreServices
    /// </summary>
    /// <param cref="IServiceCollection" name="services">Parameter for <see cref="IServiceCollection"/></param>
    /// <returns>An object of type <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services
            .AddScoped<ARepository>()
            .AddScoped<BRepository>();

        return services;
    }
}
```

Now, we have our repositories and services methods to inject created, but how do we use them?

Let's go back to our `Program.cs` and add the following:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Repositories
builder.Services.AddRepositories(); // 👀

// Services
builder.Services.AddServices(); // 👀

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
```

This looks way cleaner, isn't it? Well with that, we have successfully injected some services and repositories, but now it looks nicer and we actually have what we inject in the external library.