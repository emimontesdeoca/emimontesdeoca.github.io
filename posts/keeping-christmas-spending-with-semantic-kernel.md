# Controlling Christmas Spending with Semantic Kernel

<p align="center">
<img src="https://s5.ezgif.com/tmp/ezgif-5-5801850393.gif"/>
</p>

## Introduction

As the holiday season approaches, managing expenses can become a challenge, especially with the flurry of shopping and gift purchases. In this blog post, we'll explore how to leverage artificial intelligence to help keep track of your Christmas spending using .NET technologies. By analyzing receipts with the power of the Semantic Kernel and AI, we can efficiently extract key details such as store names, dates, item lists, and total amounts. This solution allows you to effortlessly monitor and manage your Christmas spending, ensuring you stay on top of your budget without the hassle of manually reviewing receipts.

## Calendario de Adviento de Inteligencia Artificial 2024 en Español

<p align="center">
<img src="https://media2.dev.to/dynamic/image/width=800%2Cheight=%2Cfit=scale-down%2Cgravity=auto%2Cformat=auto/https%3A%2F%2Fdev-to-uploads.s3.amazonaws.com%2Fuploads%2Farticles%2Fsdkovc8lzahgm8pvsblk.png"/>
</p>

This project was inspired by my participation in the **Calendario de Adviento de Inteligencia Artificial 2024 en Español**, an online event dedicated to AI. You can find more about the event on [this Dev.to link](https://dev.to/roberto_navarro_mate/calendario-de-adviento-de-inteligencia-artificial-2024-en-espanol-bdb).

## The project

For this project, we will be using **Azure OpenAI**, a service that allows us to utilize powerful AI models such as GPT-4 to process and analyze images. The process involves several steps, from setting up the backend API service to integrating with a Blazor front end for image uploads. We will also be using **.NET Aspire**, a component that helps connect everything seamlessly.

## Prerequisites

Before we dive into the code, make sure you have the following prerequisites:

- .NET 9
- Azure OpenAI access (API key)
- Visual Studio or Visual Studio Code
- Basic knowledge of Blazor, HTTP clients, and API development

## The Visual Studio solution

We will end up having something like this, I like to keep stuff separated and with cool names so here's how it looks:

<p align="center">
<img src="https://imgur.com/gAnGLhM.png">
</p>

But let's go step by step creating stuff!

## Step 0: The models

The core of the Receipt Scanner application relies on several key models that facilitate the interaction between the front-end, API, and AI services. Below are the main models used in this project:

- **AnalyzeReceiptRequest**  
   This model represents the request structure for analyzing a receipt. It contains the `ImageBytes` property, which holds the byte array of the receipt image that will be processed.

   ```csharp
   public class AnalyzeReceiptRequest
   {
       public byte[] ImageBytes { get; set; }
   }
   ```

- **ReceiptAnalyzeResult**  
   This model captures the result after processing a receipt. It holds the structured data extracted from the receipt, such as the store name, date, items, and total amount.

   ```csharp
   public class ReceiptAnalyzeResult
   {
       public DateTime CreatedAt { get; set; }
       public ReceiptData Result { get; set; }
   }
   ```

- **ReceiptData**  
   This is the model that holds the structured receipt data. It includes properties for the store name, date, a list of items (with each item’s name and price), and the total amount on the receipt.

   ```csharp
   public class ReceiptData
   {
       public string Store { get; set; }
       public DateTime? Date { get; set; }
       public List<ReceiptItem> Items { get; set; }
       public decimal? Total { get; set; }
   }
   ```

-  **ReceiptItem**  
   Each item on the receipt is represented by this model. It holds the item name and its price.

   ```csharp
   public class ReceiptItem
   {
       public string Name { get; set; }
       public decimal? Price { get; set; }
   }
   ```

These models serve as the foundation for passing data between the client and server, ensuring a smooth flow of information. The API receives the receipt image, and in return, it processes and returns a structured JSON object that can be easily consumed by the front-end.

## Step 1: Setting Up the Backend API Service

The first step in building this application is setting up an API service to analyze receipt images. We'll use the **Azure OpenAI** API to extract information from the receipt images. Here’s a breakdown of how everything fits together:

### AI Service - A Deep Dive

The AI service is at the core of our receipt analysis system. It’s responsible for communicating with Azure OpenAI’s API to process the image data and return meaningful insights. The **AiApiClient** class is the client that will handle all interactions with the Azure OpenAI API.

#### AI Client Implementation

The `AiApiClient` is the key component responsible for sending the receipt image (in byte array format) to the Azure OpenAI API. It handles the communication, error logging, and data parsing:

```csharp
public class AiApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiApiClient> _logger;

    public AiApiClient(HttpClient httpClient, ILogger<AiApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ReceiptAnalyzeResult?> AnalyzeAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            _logger.LogWarning("ImageBytes is null or empty.");
            return null;
        }

        _logger.LogInformation("Sending analyze request with image bytes of length: {Length}", imageBytes.Length);

        var request = new AnalyzeReceiptRequest
        {
            ImageBytes = imageBytes
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/analyze-receipt", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to analyze receipt. StatusCode: {StatusCode}", response.StatusCode);
                return null;
            }

            var analyzeResult = await response.Content.ReadFromJsonAsync<ReceiptAnalyzeResult>(cancellationToken: cancellationToken);

            if (analyzeResult == null)
            {
                _logger.LogWarning("No content received from AI API service.");
                return null;
            }

            _logger.LogInformation("Analysis result received: {AnalyzeResult}", analyzeResult);

            return analyzeResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while analyzing the receipt.");
            return null;
        }
    }
}
```

In this part of the code, we define the `AnalyzeAsync` method, which is responsible for:
1. Sending the image byte array to the Azure OpenAI API.
2. Handling any errors or unsuccessful responses from the API.
3. Parsing the returned JSON data into a structured result (`ReceiptAnalyzeResult`).

The benefits of separating this functionality into a dedicated service (AiApiClient) include:
- **Error Handling:** Centralized handling of errors like network issues or invalid responses.
- **Logging:** Proper logging of requests and responses to monitor system behavior.

<p align="center">
<img src="https://imgur.com/q3EpCSy.png"/>
</p>

### API Service - Handling Requests and Responses

The **API Service** acts as the intermediary between the frontend Blazor application and the AI service. This service is responsible for accepting the image data, passing it to the AI service, and returning the analysis results to the client.

#### API Endpoint

In this step, we define a simple API endpoint to accept receipt images, forward them to the AI service for processing, and return the results to the client:

```csharp
using ReceiptScanner.Shared.Clients;
using ReceiptScanner.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Register AiApiClient with HttpClient
builder.Services.AddHttpClient<AiApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://aiservice");
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// POST endpoint to analyze receipt
app.MapPost("/api/analyze-receipt", async (AnalyzeReceiptRequest request, AiApiClient aiApiClient, ILogger<Program> logger) =>
{
    if (request.ImageBytes == null || request.ImageBytes.Length == 0)
    {
        logger.LogWarning("ImageBytes is null or empty.");
        return Results.BadRequest("ImageBytes is required.");
    }

    logger.LogInformation("Received analyze receipt request with image bytes of length: {Length}", request.ImageBytes.Length);

    try
    {
        var result = await aiApiClient.AnalyzeAsync(request.ImageBytes);

        if (result == null)
        {
            logger.LogWarning("Failed to analyze the receipt.");
            return Results.Problem("Unable to process the receipt at this time. Please try again later.");
        }

        logger.LogInformation("Analysis completed successfully. Result: {Result}", result);

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while processing the receipt.");
        return Results.Problem("An error occurred while processing the receipt. Please try again later.");
    }
});

app.MapDefaultEndpoints();

app.Run();

```

This endpoint:
1. Accepts the receipt image as part of the request body.
2. Calls the `AiService` endopint method to send the image to Azure OpenAI for processing.
3. Returns the analysis result back to the client.

<p align="center">
<img src="https://imgur.com/u9mQrpq.png"/>
</p>

## Step 2: Setting Up the Blazor Frontend

Now that we have the backend set up, let’s turn our attention to the **Blazor frontend**. This is where users can upload their receipt images for analysis and see the results.

### Blazor Page Implementation

The Blazor page provides a simple interface where users can upload multiple receipt images and then see the analysis results displayed in a table. Here’s the code for the page:

```razor
@page "/analyzer"
@using ReceiptScanner.Shared.Clients
@using ReceiptScanner.Shared.Models
@using System.Globalization
@inject ApiServiceClient ApiClient
@inject ILogger<Program> Logger

@attribute [StreamRendering]
@rendermode InteractiveServer

<PageTitle>Receipt Analyzer</PageTitle>

<h1 class="text-center my-4">Receipt Analyzer</h1>

<div class="container">
    <p class="lead text-center mb-4">Upload receipt images below to extract their data.</p>

    <!-- File Upload Section -->
    <div class="card mb-4">
        <div class="card-body">
            <InputFile OnChange="HandleFileSelected" multiple class="form-control mb-3" />
            <button class="btn btn-primary w-100" @onclick="ProcessReceipts" disabled="@(!hasFiles)" type="button">
                <span class="@(!processing ? "d-none" : "") spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                @if (processing)
                {
                    <span>Processing...</span>
                }
                else
                {
                    <span>Process Receipts</span>
                }
            </button>
        </div>
    </div>

    <!-- Uploaded Images Preview -->
    @if (fileBytesList.Any())
    {
        <div class="card mb-4">
            <div class="card-header">
                <h5 class="mb-0">Uploaded Receipt Images</h5>
            </div>
            <div class="card-body">
                <div class="row">
                    @foreach (var fileBytes in fileBytesList)
                    {
                        <div class="col-12 col-md-4 mb-3">
                            <img src="@($"data:image/jpeg;base64,{Convert.ToBase64String(fileBytes)}")" class="img-fluid rounded" alt="Uploaded receipt" />
                        </div>
                    }
                </div>
            </div>
        </div>
    }

    <!-- Processing Indicator -->
    @if (processing)
    {
        <div class="alert alert-info text-center" role="alert">
            <strong>Processing receipts...</strong> Please wait while we analyze the uploaded files.
        </div>
    }

    <!-- Analysis Results Section -->
    @if (analyzedReceipts != null && analyzedReceipts.Any())
    {
        <div class="card">
            <div class="card-header">
                <h5 class="mb-0">Analysis Results</h5>
            </div>
            <div class="card-body">
                <table class="table table-striped table-bordered">
                    <thead>
                        <tr>
                            <th>Store</th>
                            <th>Date</th>
                            <th>Total</th>
                            <th>Items</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var receipt in analyzedReceipts)
                        {
                            <tr>
                                <td>@(receipt.Result?.Store ?? "Unknown")</td>
                                <td>@(receipt.Result?.Date?.ToString() ?? "Unknown")</td>
                                <td>@(receipt.Result?.Total?.ToString("C", ci) ?? "Unknown")</td>
                                <td>
                                    <ul class="list-unstyled">
                                        @if (receipt.Result?.Items is not null)
                                        {
                                            @foreach (var item in receipt.Result?.Items!)
                                            {
                                                <li><strong>@item.Name</strong> - @item.Price?.ToString("C", ci)</li>
                                            }
                                        }
                                    </ul>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    }
    else if (processed && (analyzedReceipts == null || !analyzedReceipts.Any()))
    {
        <div class="alert alert-warning text-center" role="alert">
            <strong>No results found.</strong> Please try again with different images or ensure they are clear and legible.
        </div>
    }
</div>

@code {
    private bool hasFiles;
    private bool processing;
    private bool processed;
    private List<byte[]> fileBytesList = new();
    private List<ReceiptAnalyzeResult> analyzedReceipts = new();
    CultureInfo ci = new CultureInfo("es-es");

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        try
        {
            fileBytesList.Clear();

            foreach (var file in e.GetMultipleFiles())
            {
                var memoryStream = new MemoryStream();
                await file.OpenReadStream().CopyToAsync(memoryStream);
                fileBytesList.Add(memoryStream.ToArray());
            }

            hasFiles = fileBytesList.Any();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while handling file upload.");
        }
    }

    private async Task ProcessReceipts()
    {
        if (!hasFiles)
            return;

        processing = true;
        analyzedReceipts.Clear();

        try
        {
            foreach (var fileBytes in fileBytesList)
            {
                var result = await ApiClient.AnalyzeReceiptAsync(fileBytes);
                if (result != null)
                {
                    analyzedReceipts.Add(result);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while processing receipts.");
        }
        finally
        {
            processing = false;
            processed = true;
        }
    }
}

```

This page allows users to upload receipts and shows the analysis results in a table with store names, dates, total amounts, and the list of items.

<p align="center">
<img src="https://imgur.com/BLswKhm.png">
</p>

## Step 3: .NET Aspire

<p align="center">
<img src="https://imgur.com/ja56RWN.png">
</p>

### What is .NET Aspire?

.NET Aspire is a set of powerful tools, templates, and packages for building observable, production ready apps. .NET Aspire is delivered through a collection of NuGet packages that handle specific cloud-native concerns. Cloud-native apps often consist of small, interconnected pieces or microservices rather than a single, monolithic code base. Cloud-native apps generally consume a large number of services, such as databases, messaging, and caching. For information on support, see the .NET Aspire Support Policy.

A distributed application is one that uses computational resources across multiple nodes, such as containers running on different hosts. Such nodes must communicate over network boundaries to deliver responses to users. A cloud-native app is a specific type of distributed app that takes full advantage of the scalability, resilience, and manageability of cloud infrastructures.

Using **.NET Aspire** for this project provides several benefits that improve the overall system quality, such as:

### 1. **Centralized Logging**

.NET Aspire automatically integrates logging across the entire application, which means you don't have to manually configure logging for each service. This ensures that logs are consistent and stored in a centralized location, making debugging and monitoring much easier.

For instance, the `AiApiClient` class uses logging to record the image bytes sent to the AI service, the API responses, and any errors that occur during the analysis process.

```csharp
_logger.LogInformation("Sending analyze request with image bytes of length: {Length}",

 imageBytes.Length);
```
<p align="center">
<img src="https://imgur.com/5NS416X.png">
</p>

### 2. **Automatic Metrics Collection**

.NET Aspire also automatically tracks and reports important application metrics such as response times, request counts, and error rates. This helps you understand how the application is performing and quickly detect any bottlenecks or issues.

<p align="center">
<img src="https://imgur.com/SGawOY3.png">
</p>

### 3. **Improved Performance**

.NET Aspire optimizes HTTP calls, which helps to keep response times low and reduce unnecessary resource consumption. It provides features like connection pooling, request retries, and intelligent routing.

### 4. **Seamless Integration**

.NET Aspire simplifies the integration of various services (like the AI and API services in this project) and streamlines the deployment process. You don't need to worry about low-level configurations, as Aspire takes care of the infrastructure-related tasks for you.

<p align="center">
<img src="https://imgur.com/OSHhWVb.png">
</p>

### Conclusion

AI is no longer just a buzzword or something we see in sci-fi movies. It's actively solving real-world problems today, like the one we tackled in this project—extracting structured data from receipts. With the help of **Azure OpenAI**, **.NET Aspire**, and **Blazor**, we can automate what would otherwise be a time-consuming and error-prone manual task. AI doesn't just chat or respond to prompts like ChatGPT; it interprets images, extracts valuable information, and gives us actionable insights in seconds.

By using **Azure OpenAI** for receipt analysis and **.NET Aspire** for seamless integration with logging and metrics, we’ve created a solution that is both powerful and scalable. The potential of AI to streamline business processes, automate tedious tasks, and improve accuracy is enormous, and this is just one example of how it can be applied.

This post is part of the **Calendario de Adviento de Inteligencia Artificial 2024 en Español**, an event that showcases real-world AI applications and educates the Spanish-speaking tech community on the latest trends. If you’re looking to dive deeper into AI and its possibilities, this event is a great place to start.

AI is transforming how we work, and this project is just a glimpse of what’s possible. The real power of AI is in its ability to solve real problems—whether it's processing receipts, analyzing images, or predicting trends. We're just scratching the surface.

### Source Code

The full source code for this project is available on [GitHub](https://github.com/emimontesdeoca/ReceiptScannerPoc). Feel free to download it, explore how the AI and API services work together, and adapt it for your own use cases. If you run into any issues, or if you have ideas for improvements, don't hesitate to create an issue or submit a pull request. Contributions are always welcome, and your feedback will help make this project even better!

Happy coding!

