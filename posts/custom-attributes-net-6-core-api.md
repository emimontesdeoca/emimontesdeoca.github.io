Custom attributes are really a good thing to use, I've come to start using them very recently, because they allow me to create a single one of them and reusing them on either the controller, the class, or the method itself.

They really help when you want to do some security stuff like checking for headers, or check the value of a parameter that you definitely need.

In my case we are going to use it on the an .NET Core API project, where we are going to check if all the request contain a certain header.

# HeaderCheckAttribute

So after we created our cool .NET Core API, let's create a folder to store our stuff, because we like using folders.

<img src="https://i.imgur.com/i2VKbZN.png"/>


And then we are going to add the logic to our `HeaderCheckAttribute` class.


```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotNet6CustomAttribute.Attributes
{
    public class HeaderCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Get all headers
            var headers = context.HttpContext.Request.Headers;

            // Check if headers has x-dotnet-6-custom-attribute
            if (!headers.ContainsKey("x-dotnet-6-custom-attribute"))
            {
                context.Result = new BadRequestObjectResult("The header x-dotnet-6-custom-attribute is missing");
            }
            else if (string.IsNullOrEmpty(headers["x-dotnet-6-custom-attribute"]))
            {
                context.Result = new BadRequestObjectResult("The header x-dotnet-6-custom-attribute can't be null or empty");
            }

            base.OnActionExecuting(context);
        }
    }
}

```

Basically the logic is, first it checks for a header with the key `x-dotnet-6-custom-attribute` and the if it's there, it check if it has values.

If both of those expressions are true, it will reutnr a `BadRequestObjectResult` with a certain message.

# Adding it to the controller

We can add this logic to multiple places, we can add directly to the entire controller, or we can add it to some of the methods, we are going to add it first to methods and then to the entire controller.

So let's decorate the `WeatherForecastController` class with them.

```csharp
using DotNet6CustomAttribute.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace DotNet6CustomAttribute.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("GetWeatherForecastWithCheck")]
        [HeaderCheckAttribute]
        public IEnumerable<WeatherForecast> GetWithCheck()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("GetWeatherForecastWithoutCheck")]
        public IEnumerable<WeatherForecast> GetWithoutCheck()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
```

Let's run the project!

<img src="https://i.imgur.com/ZvO4LnE.png"/>

We have 2 functions in there: `GetWeatherForecastWithCheck` and `GetWeatherForecastWithoutCheck`, one of them will fail and other will not, but let's check it out on Swagger!


<img src="https://i.imgur.com/x1yRb9j.png"/>

<img src="https://i.imgur.com/XiO4GC9.png">

As you can see one of the returns an 400 error with our message, and the other returns the values, now to fully test this, let's run Postman and add a header so we also see the data using `GetWeatherForecastWithCheck`.

# Postman

Now running on Postman, we add the header and we see that the error message has changed, since now we do supply the header but there's no value to it

<img src="https://i.imgur.com/JHP8ZXZ.png"/>

If we add a value to it, we finally get the values!

<img src="https://i.imgur.com/jxt6xFe.png"/>

# That's it

Well that's it! Pretty simple right? Well now you know how to create an attribute adn assign it to methods and controllers!

Have fun with them! 

# Code

This entire project is on Github and you can find it [here](https://github.com/emimontesdeoca/dotnet-6-attribute-post)!

If you have any issues or question, feel free to contact me on any social media at @emimontesdeoca (in Twitter is actually `@emimontesdeocaa` with two `aa` at the end). You can also find most of my socials on the blog's header.

Hope you liked the post! Cya!
