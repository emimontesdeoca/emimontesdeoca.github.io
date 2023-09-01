Dependency injection is probably one of the best features we have on .NET at this point. There's no way in any posible case that you're not using it, so if you are like me, you pretty much want to add it to all the implementations you make.

Filters, as per the official Microsoft's [documentation](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-3.1):

>Filters in ASP.NET Core allow code to be run before or after specific stages in the request processing pipeline.
>
>Built-in filters handle tasks such as:
>
>* Authorization, preventing access to resources a user isn't authorized for.
>* Response caching, short-circuiting the request pipeline to return a cached response.
>
>Custom filters can be created to handle cross-cutting concerns. Examples of cross-cutting concerns include error handling, caching, configuration, authorization, and logging. Filters avoid duplicating code.

I work a lot with APIs and there are some stuff that must run every single request, or pretty much all of them so, ideally what we want to do is work with it plus.... dependency injection!

But it's a bit trick sometimes, it doesn't work as we want if we want to inherit from `ActionAttribute` so we need to work with `TypeFilterAttribute`, which let us do stuff when overriding `OnActionExecutionAsync`.

I usually create this filters to do some logging, so we're gonna use that as example:

```csharp
/// <summary>
/// LoggedQueryAttribute class
/// </summary>
public class LoggedQueryTypeFilterAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Constructor for <see cref="LoggedQueryTypeFilterAttribute"/>
    /// </summary>
    public LoggedQueryTypeFilterAttribute() : base(typeof(LoggedQueryFilter))
    {
    }

    /// <summary>
    /// LoggedQueryFilter class
    /// </summary>
    private class LoggedQueryFilter : IAsyncActionFilter
    {
        /// <summary>
        /// <see cref="_loggingService"/> object
        /// </summary>
        private readonly LoggingService _loggingService;

        /// <summary>
        /// Constructor for <see cref="LoggedQueryFilter"/>
        /// </summary>
        /// <param cref="LoggingService" name="loggingService">Parameter for loggingService</param>
        public LoggedQueryFilter(LoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        /// <summary>
        /// OnActionExecutionAsync
        /// </summary>
        /// <param cref="ActionExecutingContext" name="context">Parameter for context</param>
        /// <param cref="ActionExecutionDelegate" name="next">Parameter for next</param>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get properties
            var properties = (Request)context.ActionArguments.First().Value!;

            // Get call from context
            var call = context.HttpContext.Request.Path.Value!;

            // Logging
            _loggingService.LogCustomEvent(call);

            // Continue call
            await next();
        }
    }
}
```

Logic is pretty simple, we get the body by accessing the `context` object with `context.ActionArguments.First().Value`, alsowe ge the method call with `context.HttpContext.Request.Path.Value`.

Then we just call our method from our service, in this case is ` _loggingService.LogCustomEvent(call)`.

Then, we must call `await next();`, because the pipeline must continue.

This is for the attribute, now, we must actually include this attribute to a method.

```csharp
[LoggedQueryTypeFilterAttribute]
public ActionResult<string> TestFilter()
{
    return Ok("Hello world!");
}
```

Hope you liked it, if you have any questions or you want to get in touch, don't hestiate and contact me!