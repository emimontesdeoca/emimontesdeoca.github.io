Christmas is around the corner, and with it comes the joyous task of finding the perfect gifts for loved ones. If you're anything like me, you probably love getting a good deal, but navigating the skyrocketing prices of popular items during the holiday season can feel like a real-life sleigh ride—exciting but a bit overwhelming! This year, instead of driving myself crazy with daily price checks, I decided to put my tech skills to good use and automate the process.

<p align="center">
  <img src="https://imgur.com/7K7QC08.png" />
</p>

As a software developer and someone passionate about leveraging technology to solve everyday problems, I found a way to automate the process of checking prices. Instead of manually visiting multiple online stores every day to compare prices, I decided to create a system that can do it for me. This not only saves time but also ensures I get the best deals without the stress of daily checks.

## Why Automate Price Monitoring?

Automating price monitoring is like having your own team of Santa's elves working around the clock to ensure you get the best deals without lifting a finger. Here’s why you’ll love it:

1. **Expensive Items**: We all know Christmas presents can get pretty pricey, especially when gadgets and toys are on your list. Saving money on these can be as satisfying as finding the last tree-topper in stock.
2. **Daily Checks**: Who has the time (or the patience) to check prices every day? Not me!
3. **Automation**: Embrace the holiday spirit of giving—give yourself the gift of efficiency.
4. **Accuracy**: Automation ensures your price checks are as accurate as Rudolph's nose is bright.

## Core Technologies

To build my own Santa's little helper, I decided to use several key technologies:

### Containers

Containers are like the Christmas wrapping for software. They bundle up your applications with all their goodies (dependencies) so everything runs as smoothly as a sleigh ride in fresh snow. Docker is our go-to for creating these containers.

### Blazor

Blazor is a cool framework for building interactive web applications using .NET. It’s like replacing generic Christmas carols with your very own holiday playlist—tailored, efficient, and so much fun.

### Docker-Compose

Docker-Compose is the manager of our North Pole's operation. It helps us keep all our services—like the API and the Blazor front end—running together in perfect harmony. Think of it as the conductor of our holiday symphony.

## Step-by-Step Guide

Now, let’s dive into Santa's workshop and bring this project to life!

### Step 1: Creating the API

#### 1.1 Setting up the Project

Throw on your coding Santa hat and set up a new ASP.NET Core Web API project. Open your terminal (it’s a bit like opening your advent calendar) and run:

```bash
dotnet new webapi -o PriceMonitorApi
cd PriceMonitorApi
```

This command creates a new directory named `PriceMonitorApi` and sets up a basic Web API project. Imagine it’s like creating a sturdy base for your gingerbread house.

#### 1.2 Adding HttpClient and Scraping Libraries

Next, add `HttpClient` and a library to parse HTML. This will be our trusty sleigh for fetching and reading price data.

```bash
dotnet add package HtmlAgilityPack
```

HtmlAgilityPack is the elf that helps us parse HTML documents.

#### 1.3 Creating Models

Models are like the blueprint for our toys. Let’s create a model to represent the product information:

```csharp
namespace PriceMonitorApi.Models
{
    public class ProductInfo
    {
        public string Title { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
    }
}
```

We’ve just created the perfect template for our data.

#### 1.4 Implementing the Scraper Service

Our scraper service is like Santa’s little helper—fetching and processing information for us:

```csharp
using HtmlAgilityPack;
using PriceMonitorApi.Models;
using System.Globalization;

namespace PriceMonitorApi.Services
{
    public class ScraperService
    {
        private readonly HttpClient _httpClient;

        public ScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductInfo> ScrapeProductInfoAsync(string url)
        {
            var response = await _httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var title = htmlDoc.DocumentNode.SelectSingleNode("//h1[@class='product-title']").InnerText.Trim();
            var priceString = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='product-price']").InnerText.Trim();

            if (decimal.TryParse(priceString, NumberStyles.Currency, CultureInfo.InvariantCulture, out var price))
            {
                return new ProductInfo
                {
                    Title = title,
                    Price = price,
                    Date = DateTime.UtcNow
                };
            }

            throw new Exception("Unable to parse price");
        }
    }
}
```

This snippet turns our `HtmlAgilityPack` into a holiday magic wand.

#### 1.5 Creating the API Controller

Let’s create a controller that will act like the gatekeeper to our data:

```csharp
using Microsoft.AspNetCore.Mvc;
using PriceMonitorApi.Models;
using PriceMonitorApi.Services;

namespace PriceMonitorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScraperController : ControllerBase
    {
        private readonly ScraperService _scraperService;

        public ScraperController(ScraperService scraperService)
        {
            _scraperService = scraperService;
        }

        [HttpGet("productinfo")]
        public async Task<ActionResult<ProductInfo>> GetProductInfo(string url)
        {
            try
            {
                var productInfo = await _scraperService.ScrapeProductInfoAsync(url);
                return Ok(productInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
```

Our controller ensures data is delivered faster than Santa down the chimney.

#### 1.6 Registering Services in DI Container

Lastly, register your `ScraperService` to make sure it’s available when needed.

```csharp
services.AddHttpClient<ScraperService>();
```

Now your API is at the ready—like Santa’s sleigh on Christmas Eve!

---

### Step 2: Creating the Blazor Application

Blazor helps us decorate our project like a Christmas tree—making it visually appealing and interactive.

#### 2.1 Setting up the Blazor Project

Next, we’ll create a Blazor project that acts as the interface for our price monitoring sleigh ride.

```bash
dotnet new blazor -o PriceMonitorBlazor
cd PriceMonitorBlazor
```

This command sprinkles some holiday magic to set up a basic Blazor WebAssembly project.

#### 2.2 Adding Models

Just like setting up ornaments, add a `ProductInfo` model in the Blazor project:

```csharp
namespace PriceMonitorBlazor.Models
{
    public class ProductInfo
    {
        public string Title { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
    }
}
```

#### 2.3 Creating the Service for API Calls

Create a service to fetch data from our API—think of it as our online shopping buddy:

```csharp
using PriceMonitorBlazor.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PriceMonitorBlazor.Services
{
    public class ScraperService
    {
        private readonly HttpClient _httpClient;

        public ScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductInfo> GetProductInfoAsync(string url)
        {
            var response = await _httpClient.GetFromJsonAsync<ProductInfo>($"https://localhost:5001/api/scraper/productinfo?url={url}");
            return response;
        }
    }
}
```

#### 2.4 Registering Services in DI Container

Ensure `ScraperService` is registered so we can inject it into our components.

```csharp
builder.Services.AddScoped<ScraperService>();
```

### 2.5 Creating the Blazor Component

Update `Pages/Index.razor` to include a fun and interactive interface:

```razor
@page "/"
@using PriceMonitorBlazor.Models
@using PriceMonitorBlazor.Services
@inject ScraperService ScraperService

<h3>Price Monitor</h3>

<p>
    <label for="urlInput">Product URL:</label>
    <input id="urlInput" @bind="productUrl" />
    <button @onclick="FetchProductInfo">Fetch Price</button>
</p>

@if (productInfos.Any())
{
    <table class="table">
        <thead>
            <tr>
                <th>Title</th>
                <th>Price</th>
                <th>Date</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var product in productInfos)
            {
                <tr>
                    <td>@product.Title</td>
                    <td>@product.Price</td>
                    <td>@product.Date</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private string productUrl;
    private List<ProductInfo> productInfos = new List<ProductInfo>();

    private async Task FetchProductInfo()
    {
        if (!string.IsNullOrEmpty(productUrl))
        {
            var productInfo = await ScraperService.GetProductInfoAsync(productUrl);
            productInfos.Add(productInfo);
        }
    }
}
```

It's like setting up a festive holiday display—making our app interactive and delightful.

---

### Step 3: Connecting Everything Using Docker-Compose

Now let’s connect everything using Docker-Compose, turning our project into a well-oiled sleigh ride.

#### 3.1 Creating Dockerfiles

Create Dockerfiles for both the API and Blazor projects:

**PriceMonitorApi/Dockerfile**:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PriceMonitorApi/PriceMonitorApi.csproj", "PriceMonitorApi/"]
RUN dotnet restore "PriceMonitorApi/PriceMonitorApi.csproj"
COPY . .
WORKDIR "/src/PriceMonitorApi"
RUN dotnet build "PriceMonitorApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PriceMonitorApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PriceMonitorApi.dll"]
```

**PriceMonitorBlazor/Dockerfile**:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PriceMonitorBlazor/PriceMonitorBlazor.csproj", "PriceMonitorBlazor/"]
RUN dotnet restore "PriceMonitorBlazor/PriceMonitorBlazor.csproj"
COPY . .
WORKDIR "/src/PriceMonitorBlazor"
RUN dotnet build "PriceMonitorBlazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PriceMonitorBlazor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PriceMonitorBlazor.dll"]
```

#### 3.2 Creating docker-compose.yml

Connect all the dots (Christmas lights) with a single `docker-compose.yml` file:

```yaml
version: '3.4'

services:
  api:
    image: pricemonitorapi
    build:
      context: ./PriceMonitorApi
      dockerfile: Dockerfile
    ports:
      - "5000:80"
    networks:
      - price-monitor-network

  blazor:
    image: pricemonitorblazor
    build:
      context: ./PriceMonitorBlazor
      dockerfile: Dockerfile
    ports:
      - "5001:80"
    depends_on:
      - api
    networks:
      - price-monitor-network

networks:
  price-monitor-network:
    driver: bridge
```

### Diagrams

Below are the diagrams to help visualize the different components and the data flow, this will help us undestand what's really going on in our santa world:

#### System Architecture Diagram

![Imgur](https://imgur.com/nE0hSJ4.png)

#### Data Flow Diagram

![Imgur](https://imgur.com/hJXv4Jw.png)

#### Sequence Diagram

![Imgur](https://imgur.com/FH6VzuH.png)

#### Component Diagram for Docker Setup

![Imgur](https://imgur.com/efQ9WuT.png)

### Step 4: URL Management, Refresh, and Automated Periodic Refresh

In this demo we will just store it in memory, but in a real application, you would use a database. For this fun project, let's stick to in-memory storage.

#### 4.1 Modifying the Service to Manage URLs

First, we'll make sure our service can handle adding and retrieving URLs, as well as fetching product information:

Update `Services/ScraperService.cs`:

```csharp
using PriceMonitorBlazor.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PriceMonitorBlazor.Services
{
    public class ScraperService
    {
        private readonly HttpClient _httpClient;
        private List<string> urls = new List<string>();

        public ScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductInfo> GetProductInfoAsync(string url)
        {
            var response = await _httpClient.GetFromJsonAsync<ProductInfo>($"http://localhost:5000/api/scraper/productinfo?url={url}");
            return response;
        }

        public void AddUrl(string url)
        {
            if (!urls.Contains(url))
            {
                urls.Add(url);
            }
        }

        public List<string> GetUrls()
        {
            return urls;
        }

        public void ClearUrls()
        {
            urls.Clear();
        }
    }
}
```

#### 4.2 Updating the Blazor Component for URL Management and Refresh

Next, update `Pages/Index.razor` to add URL management, refresh, and automated periodic refresh:

```razor
@page "/"
@using PriceMonitorBlazor.Models
@using PriceMonitorBlazor.Services
@inject ScraperService ScraperService

<h3>Price Monitor</h3>

<p>
    <label for="urlInput">Product URL:</label>
    <input id="urlInput" @bind="productUrl" />
    <button @onclick="AddUrl">Add URL</button>
</p>

<p>
    <button @onclick="RefreshPrices">Refresh All Prices</button>
</p>

@if (productInfos.Any())
{
    <table class="table">
        <thead>
            <tr>
                <th>Title</th>
                <th>Price</th>
                <th>Date</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var product in productInfos)
            {
                <tr>
                    <td>@product.Title</td>
                    <td>@product.Price</td>
                    <td>@product.Date</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private string productUrl;
    private List<ProductInfo> productInfos = new List<ProductInfo>();
    private Timer _timer;

    protected override void OnInitialized()
    {
        StartTimer();
    }

    private void StartTimer()
    {
        // Set up the timer to call RefreshPrices every minute (60000 ms)
        _timer = new Timer(async _ => await InvokeAsync(RefreshPrices), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    private void AddUrl()
    {
        if (!string.IsNullOrEmpty(productUrl))
        {
            ScraperService.AddUrl(productUrl);
            productUrl = string.Empty;
        }
    }

    private async Task RefreshPrices()
    {
        productInfos.Clear();
        var urls = ScraperService.GetUrls();
        
        foreach (var url in urls)
        {
            var productInfo = await ScraperService.GetProductInfoAsync(url);
            productInfos.Add(productInfo);
        }
        StateHasChanged();
    }
}
```

In this updated component:

- We use a `Timer` to periodically call the `RefreshPrices` method every minute.
- The `StartTimer` method initializes the timer to start immediately and then trigger every 60 seconds.
- The `OnInitialized` lifecycle method calls `StartTimer` when the component is initialized to start the periodic refresh.

#### 4.3 Running the Solution

To run the updated Blazor application with the new features, rebuild and restart your Docker containers:

```bash
docker-compose build
docker-compose up
```

After loading `http://localhost:5001` in your browser, the Blazor app should now automatically refresh product prices every minute in addition to allowing manual refreshes and URL management.

## Conclusion

Building this price monitoring system has been a festive blast! Not only did it save me from the stress of daily price checks, but it also showcased the magic of modern web technologies.

---

# Festive Tech Calendar 2024

<p align="center">
  <img src="https://festivetechcalendar.com/assets/images/Heading.png" />
</p>

I created this post as part of the **Festive Tech Calendar 2024** event, which brings together tech enthusiasts, innovators, and digital dreamers to share knowledge and celebrate the fusion of festive spirit and technological marvels. This initiative is not just about learning and connecting but also about giving back.

**Festive Tech Calendar 2024** is supporting the Beatson Cancer Charity this year. The Beatson Cancer Charity is dedicated to supporting people affected by cancer, their families, and the healthcare professionals who care for them. More information about their incredible work can be found at [https://www.beatsoncancercharity.org/](https://www.beatsoncancercharity.org/).

Check out the Festive Tech Calendar website at [https://festivetechcalendar.com](https://festivetechcalendar.com) for frequently asked questions and more details about the event.

---

# **HO HO HO!**

Creating this project has been a wonderful way to contribute to the festive tech community and support a great cause at the same time.

I hope you found this guide helpful and that it inspires you to explore more ways to use technology to simplify everyday tasks.

If you have any questions or need further assistance, please don't hesitate to reach out.

Happy coding!