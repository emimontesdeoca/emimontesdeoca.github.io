Exceptions are bad, we know right? But what if we have to handle them?

What happens when we have an exception, for example, on an API, it displays a stack message that includes a lot of information that we might want to remove from the response that our users get.

For the demo, I've created an dotnet API and added a method that will throw an exception:

```csharp
[HttpGet]
[Route("GetWithoutExceptionHandler")]
public Task GetWithoutExceptionHandler()
{
    throw new Exception("This is a custom exception!");
}
```

If we throw an exception, it looks like this:

```csharp
System.Exception: This is a custom exception!
   at CustomExceptionHandleDemo.Controllers.WeatherForecastController.GetWithoutExceptionHandler()
   at lambda_method16(Closure , Object , Object[] )
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableResultExecutor.Execute(IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Logged|12_1(ControllerActionInvoker invoker)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
--- End of stack trace from previous location ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|6_0(Endpoint endpoint, Task requestTask, ILogger logger)
   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
   at Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIMiddleware.Invoke(HttpContext httpContext)
   at Swashbuckle.AspNetCore.Swagger.SwaggerMiddleware.Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware.Invoke(HttpContext context)
```

This looks normal, this is what you get from an exception, but for me, it's displaying a lot of information, if you have libraries and stuff, it could show sensible information regarding your client, your project or other stuff to someone that could be trying to see stuff.

This is what it looks on Swagger:

<img src="https://imgur.com/fUujR3s.png"/>

### Creating a custom exception

This step could be avoided, since we know what exception will be throw, in this case a `Exception`, but for me, having your custom exceptions is the better since you have more control of what you're throwing.

In this case, I just created an object `CustomException` that inherits from `Exception`:

```csharp
namespace CustomExceptionHandleDemo.Exceptions
{
    public class CustomException : Exception
    {
        /// <summary>
        /// Constructor for <see cref="CustomException"/>
        /// </summary>
        public CustomException() { }

        /// <summary>
        /// Constructor for <see cref="CustomException"/>
        /// </summary>
        /// <param cref="string" name="message">Parameter for message</param>
        public CustomException(string message) : base(message) { }

        /// <summary>
        /// Constructor for <see cref="CustomException"/>
        /// </summary>
        /// <param cref="string" name="message">Parameter for message</param>
        /// <param cref="Exception" name="inner">Parameter for inner</param>
        public CustomException(string message, Exception inner) : base(message, inner) { }
    }
}
```

After we have created our custom exception, let's update our method to throw `CustomException` instead of `Exception`:

```csharp
[HttpGet]
[Route("GetWithoutExceptionHandler")]
public Task GetWithoutExceptionHandler()
{
    throw new CustomException("This is a custom exception!");
}
```

This for now will not change anything but the stack trace will show that the object that was thrown is a `CustomException` instead of `Exception`, take a look at the start of the stack trace:

```csharp
CustomExceptionHandleDemo.Exceptions.CustomException: This is a custom exception!
   at CustomExceptionHandleDemo.Controllers.WeatherForecastController.GetWithoutExceptionHandler()
   at lambda_method24(Closure , Object , Object[] )
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableResultExecutor.Execute(IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Logged|12_1(ControllerActionInvoker invoker)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
--- End of stack trace from previous location ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|6_0(Endpoint endpoint, Task requestTask, ILogger logger)
   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
   at Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIMiddleware.Invoke(HttpContext httpContext)
   at Swashbuckle.AspNetCore.Swagger.SwaggerMiddleware.Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware.Invoke(HttpContext context)
```

### Creating a ExceptionFilterAttribute

Microsoft has given us a way to handle exceptions after they have thrown, you can check more information [here](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.filters.exceptionfilterattribute?view=aspnetcore-7.0).

But so far the documentation that they give us is:

>An abstract filter that runs asynchronously after an action has thrown an Exception. Subclasses must override OnException(ExceptionContext) or OnExceptionAsync(ExceptionContext) but not both.

So let's create one then, we're going to create `CustomExceptionFilterAttribute` in which we are going to override `OnException`:

```csharp
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using CustomExceptionHandleDemo.Exceptions;

namespace CustomExceptionHandleDemo.ExceptionFilterAttributes
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// OnException
        /// </summary>
        /// <param cref="ExceptionContext" name="context">Parameter for context</param>
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is CustomException)
            {
                context.HttpContext.Response.StatusCode = 500;
                context.Result = new ObjectResult(context.Exception.Message);
            }
        }
    }
}
```

As you can see, we are taking a look at the `ExceptionContext`, when the exception is a type of `CustomException`, we do something.

This something is updating the response and the status code of what we are going to return.

In order to update the status code, we must update `context.HttpContext.Response.StatusCode` and in order to return the result, we have to update the `context.Result` by giving it an object that is inherited from `ActionResult`.

This is a filter, so it means we have to add it something by adding `[CustomExceptionFilter]`.

### Using the filter

Now, let's replicate the method we have and add this filter so it takes action, our API endpoint will end up like this:

```csharp
using CustomExceptionHandleDemo.ExceptionFilterAttributes;
using CustomExceptionHandleDemo.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CustomExceptionHandleDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        public WeatherForecastController()
        {
        }

        [HttpGet]
        [Route("GetWithoutExceptionHandler")]
        public Task GetWithoutExceptionHandler()
        {
            throw new CustomException("This is a custom exception!");
        }

        [HttpGet]
        [Route("GetWithExceptionHandler")]
        [CustomExceptionFilter]
        public Task GetWithExceptionHandler()
        {
            throw new CustomException("This is a custom exception!");
        }
    }
}
```

As you can see, we have a new method called `GetWithExceptionHandler`, that has the same logic that `GetWithoutExceptionHandler` has, but in this case, we've added the filter `[CustomExceptionFilter]` to the method .

The result is the following after we run the method, I'll display an image, because it's not showing the stack trace anymore:

<img src="https://imgur.com/a8TSAy2.png"/>

So with this we've created a custom exception, a filter to override what happens when we throw and exception and use it on a method.

This can be used for a lot of stuff like logging and knowing what, when and where the error happens.