Using mutiple libraries to split your code for future cases is a good thing. Personally I love doing it, if I can split my logic so it can be reused in other projects or parts of the projects.

For me at least, it's a must to split the data models, that includes the `Entity Framework`, so it can be referenced and used in a `Console application`, a `Blazor application` or an `API`.

Let's take a look to an example of how I do things, this is an screenshot of a project that have a few applications and a shared library that has all the models.

<img src="https://imgur.com/gdg2nn3.png">

This is an example of a class that I moved from the `API` project with documentation with `summary` on each property and class.

```csharp
namespace CustomLibrariesDocumentation.Models
{
    /// <summary>
    /// WeatherForecast class
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// Gets or sets the Date
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Gets or sets the TemperatureC
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// Gets or sets the TemperatureF
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// Gets or sets the Summary
        /// </summary>
        public string? Summary { get; set; }
    }
}
```

If we reference it to the `Console application` and we start to use the model, we can see the documentation, that's a standard feature in Visual Studio so it works fine, as you can see here:

<img src="https://imgur.com/Djf1MeO.png">

But then, if we reference it to the `API` and we go to `Swagger`, there's no summary documentation.

<img src="https://imgur.com/3Dg2Xpm.png">

How do we fix this?

First, we have to enable XML comments on the library, in order to do this, you need to update the projects settings and enable ``:

<img src="https://imgur.com/R1j8SfX.png">

This will generate some files on the build folder that end with the file extension `xml`, as you can see here:

<img src="https://imgur.com/O8ywjr2.png">

Now that we have this done, we have to add some stuff on the `Program.cs`, in order to swagger to read those files, this is because by default it only loads the XML definition of the project it's on.

It uses a method that we have inside the `AddSwaggerGen` that is called `IncludeXmlComments`.

The idea is that if we have all the `xml` files, we are going to force loading them onto the Swagger.

```csharp
builder.Services.AddSwaggerGen(s =>
{
    // Comments
    var allXmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");

    foreach (string xmlFiles in allXmlFiles)
    {
        s.IncludeXmlComments(xmlFiles);
    }
});
```

It's straight forwared, we get the `xml` files from the build directory and add them with the `IncludeXmlComments` method.

Now we load again the `API` and we check if we can see the documentation.

<img src="https://imgur.com/uNVNswn.png">

And you can see that we can see the documentation!


I hope it helped you, if you have any question feel free to contact me!