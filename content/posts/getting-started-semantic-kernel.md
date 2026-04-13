---
title: "Getting Started with Semantic Kernel: AI Orchestration in C#"
date: 2025-10-05T00:00:00Z
draft: false
tags: ["AI", ".NET", "Semantic Kernel", "Azure"]
slug: "getting-started-semantic-kernel"
description: "Learn how to use Microsoft's Semantic Kernel to build AI-powered applications in C# — from plugins and planners to memory and function calling."
---

If you've been building .NET applications and watching the AI landscape evolve, you've probably wondered: *what's the best way to integrate large language models into my C# projects without turning my codebase into spaghetti?* That's exactly the problem Microsoft's Semantic Kernel solves, and after spending the last year building production applications with it, I can tell you it's become one of the most important tools in my developer toolkit.

In this post, I'll walk you through everything you need to get started with Semantic Kernel — from understanding the core concepts to building a real-world AI assistant. Whether you're just dipping your toes into AI development or looking for a structured way to orchestrate LLM calls in your existing .NET applications, this guide has you covered.

## What is Semantic Kernel?

Semantic Kernel (SK) is an open-source SDK from Microsoft that acts as an **orchestration layer** between your application code and large language models like GPT-4o, Azure OpenAI, or other AI services. Think of it as a lightweight middleware that lets you combine traditional C# code with AI capabilities in a clean, composable way.

But why not just call the OpenAI API directly? You absolutely can — and for simple use cases, that's fine. But the moment you need to:

- Let the AI decide **which functions** to call based on user input
- Combine **multiple AI calls** with traditional code in a pipeline
- Add **memory and context** so the AI remembers previous interactions
- Build **multi-step agents** that reason through complex tasks

...you'll find yourself reinventing the wheel. Semantic Kernel gives you all of this out of the box, with first-class .NET support, dependency injection integration, and a plugin architecture that feels natural to any C# developer.

The project lives on GitHub under the `microsoft/semantic-kernel` repository and has SDKs for C#, Python, and Java. The C# SDK is the most mature and is the one we'll focus on here.

## Core Concepts

Before we write any code, let's understand the building blocks.

### The Kernel

The `Kernel` is the central object in Semantic Kernel. It's the orchestrator — the thing that ties together your AI services, plugins, and configuration. You create one, register your services and plugins on it, and then use it to execute prompts or invoke functions. If you're familiar with dependency injection in ASP.NET Core, the Kernel will feel very familiar — it's essentially a service container with AI superpowers.

### Plugins and Functions

A **plugin** is a collection of related **functions** that the Kernel can invoke. Functions come in two flavors:

- **Prompt functions** — defined as natural language templates that get sent to the LLM
- **Native functions** — regular C# methods decorated with attributes that the Kernel can discover and call

For example, you might have a `WeatherPlugin` with a native function `GetCurrentWeather(string city)` and a prompt function that summarizes weather data in a friendly way.

### AI Connectors

Connectors are how Semantic Kernel talks to AI services. The most common ones are:

- `AzureOpenAIChatCompletion` — for Azure OpenAI Service
- `OpenAIChatCompletion` — for OpenAI's API directly
- Embedding connectors for vector search and memory

You register these on the Kernel at startup, and everything else just works.

## Setting Up Your Project

Let's get our hands dirty. Start by creating a new console application:

```bash
dotnet new console -n SemanticKernelDemo
cd SemanticKernelDemo
```

Now add the Semantic Kernel NuGet packages:

```bash
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.SemanticKernel.Connectors.AzureOpenAI
```

If you're using OpenAI directly instead of Azure OpenAI:

```bash
dotnet add package Microsoft.SemanticKernel.Connectors.OpenAI
```

For memory and embeddings support (we'll use this later):

```bash
dotnet add package Microsoft.SemanticKernel.Plugins.Memory
dotnet add package Microsoft.Extensions.VectorData.Abstractions
```

Your `.csproj` should target .NET 8 or later. Semantic Kernel's latest versions take full advantage of modern .NET features.

## Your First Kernel

Let's start with the simplest possible example — creating a Kernel, connecting it to an AI service, and asking it a question.

```csharp
using Microsoft.SemanticKernel;

// Build the kernel with Azure OpenAI
var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o",
    endpoint: "https://your-resource.openai.azure.com/",
    apiKey: "your-api-key"
);

var kernel = builder.Build();

// Invoke a simple prompt
var result = await kernel.InvokePromptAsync(
    "Explain dependency injection in C# in three sentences."
);

Console.WriteLine(result);
```

If you're using OpenAI directly, swap the service registration:

```csharp
builder.AddOpenAIChatCompletion(
    modelId: "gpt-4o",
    apiKey: "your-openai-api-key"
);
```

That's it. Run it, and you'll get a concise explanation of dependency injection. But this is just scratching the surface.

### Using Prompt Templates

Prompt templates let you parameterize your prompts with variables using Handlebars-style syntax:

```csharp
var prompt = """
    You are a technical writer. Write a brief summary of {{$topic}} 
    aimed at developers with {{$experienceLevel}} experience.
    Keep it under 200 words.
    """;

var function = kernel.CreateFunctionFromPrompt(prompt);

var arguments = new KernelArguments
{
    ["topic"] = "gRPC in .NET",
    ["experienceLevel"] = "intermediate"
};

var result = await kernel.InvokeAsync(function, arguments);
Console.WriteLine(result);
```

This is where Semantic Kernel starts to shine — you can define reusable prompt templates, version them, and compose them into larger workflows.

## Plugins and Native Functions

Plugins are where Semantic Kernel bridges the gap between AI and your existing C# code. A native function is just a regular method that you expose to the Kernel.

```csharp
using Microsoft.SemanticKernel;
using System.ComponentModel;

public class TimePlugin
{
    [KernelFunction("get_current_time")]
    [Description("Gets the current date and time in UTC")]
    public string GetCurrentTime()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
    }

    [KernelFunction("get_time_in_timezone")]
    [Description("Gets the current time in a specific timezone")]
    public string GetTimeInTimezone(
        [Description("The IANA timezone identifier, e.g. 'America/New_York'")] string timezone)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        return time.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
```

Notice the `[KernelFunction]` and `[Description]` attributes. These are critical — the descriptions are what the AI reads to understand when and how to call your functions. Good descriptions make the difference between an AI that uses your tools effectively and one that's confused.

Register the plugin on your kernel:

```csharp
var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o",
    endpoint: "https://your-resource.openai.azure.com/",
    apiKey: "your-api-key"
);

builder.Plugins.AddFromType<TimePlugin>();

var kernel = builder.Build();
```

You can also build more complex plugins that inject services. Since Semantic Kernel integrates with `Microsoft.Extensions.DependencyInjection`, your plugins can receive constructor dependencies just like any other service:

```csharp
public class OrderPlugin
{
    private readonly IOrderRepository _repository;

    public OrderPlugin(IOrderRepository repository)
    {
        _repository = repository;
    }

    [KernelFunction("get_order_status")]
    [Description("Retrieves the status of an order by its ID")]
    public async Task<string> GetOrderStatus(
        [Description("The order ID to look up")] string orderId)
    {
        var order = await _repository.GetByIdAsync(orderId);
        return order is null
            ? $"No order found with ID {orderId}"
            : $"Order {orderId}: {order.Status}, placed on {order.CreatedAt:d}";
    }
}
```

## Function Calling and Auto-Invocation

This is where things get really interesting. With **function calling** (also known as tool calling), you let the AI model decide which of your registered functions to call based on the conversation context. The model doesn't execute code — it returns a structured request saying "I want to call function X with these arguments," and the Kernel handles the actual invocation.

Here's how to enable auto function calling:

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o",
    endpoint: "https://your-resource.openai.azure.com/",
    apiKey: "your-api-key"
);

builder.Plugins.AddFromType<TimePlugin>();
builder.Plugins.AddFromType<WeatherPlugin>();

var kernel = builder.Build();

// Enable automatic function calling
var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var result = await kernel.InvokePromptAsync(
    "What time is it in Tokyo and what's the weather like there?",
    new KernelArguments(settings)
);

Console.WriteLine(result);
```

With `FunctionChoiceBehavior.Auto()`, the Kernel will:

1. Send your prompt to the AI along with descriptions of all available functions
2. The AI decides it needs to call `get_time_in_timezone` and `get_weather`
3. The Kernel automatically executes those functions
4. The results are sent back to the AI
5. The AI composes a natural language response using the function results

This loop can happen multiple times in a single invocation — the AI might call several functions in sequence to gather all the information it needs. You can also use `FunctionChoiceBehavior.Required()` to force the AI to call at least one function, or provide a specific list of functions it's allowed to use.

### Chat Completion with History

For conversational applications, you'll want to use the `ChatCompletionService` directly with a `ChatHistory` object:

```csharp
using Microsoft.SemanticKernel.ChatCompletion;

var chatService = kernel.GetRequiredService<IChatCompletionService>();
var history = new ChatHistory();

history.AddSystemMessage("""
    You are a helpful developer assistant. You have access to tools 
    for checking the time and weather. Be concise and friendly.
    """);

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    history.AddUserMessage(input);

    var response = await chatService.GetChatMessageContentAsync(
        history,
        executionSettings: settings,
        kernel: kernel
    );

    history.AddAssistantMessage(response.Content ?? "");
    Console.WriteLine($"Assistant: {response.Content}");
}
```

This gives you a fully interactive chatbot that maintains conversation history and can call your plugins as needed.

## Memory and Embeddings

One of the most powerful patterns in AI applications is **Retrieval-Augmented Generation (RAG)** — giving the AI access to your own data by embedding it into vector space and retrieving relevant chunks at query time.

Semantic Kernel provides abstractions for working with vector stores and embeddings. Here's how to set up an in-memory vector store for development:

```csharp
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Embeddings;

#pragma warning disable SKEXP0010

// Create an embedding generation service
var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAITextEmbeddingGeneration(
    deploymentName: "text-embedding-ada-002",
    endpoint: "https://your-resource.openai.azure.com/",
    apiKey: "your-api-key"
);

var kernel = builder.Build();
var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

// Generate embeddings for your documents
var documents = new[]
{
    "Semantic Kernel is an open-source SDK for AI orchestration.",
    "Azure OpenAI provides enterprise-grade AI models.",
    "Plugins in SK allow you to expose C# methods to AI models."
};

var embeddings = await embeddingService.GenerateEmbeddingsAsync(documents);
```

For production scenarios, you'd store these embeddings in a dedicated vector database like Azure AI Search, Qdrant, or Pinecone. Semantic Kernel has connectors for all of these through the `Microsoft.Extensions.VectorData` abstractions.

A typical RAG flow looks like this:

1. **Ingest**: Chunk your documents, generate embeddings, store them in a vector database
2. **Retrieve**: When a user asks a question, embed the query and find the most similar documents
3. **Generate**: Pass the retrieved documents as context to the LLM along with the user's question

```csharp
[KernelFunction("search_knowledge_base")]
[Description("Searches the internal knowledge base for relevant information")]
public async Task<string> SearchKnowledgeBase(
    [Description("The search query")] string query)
{
    var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

    var results = await _vectorStore.SearchAsync(queryEmbedding, limit: 3);

    return string.Join("\n\n", results.Select(r => r.Text));
}
```

By exposing your RAG pipeline as a kernel function, the AI can automatically decide when it needs to search your knowledge base — keeping the orchestration clean and letting the model do what it does best.

## Planners and Agents

As your AI applications grow more complex, you'll need the AI to **plan and execute multi-step tasks**. This is where Semantic Kernel's agent framework comes in.

### The Basics: Chat Completion Agent

The simplest agent type wraps a chat completion model with instructions and plugins:

```csharp
using Microsoft.SemanticKernel.Agents;

#pragma warning disable SKEXP0110

var agent = new ChatCompletionAgent
{
    Name = "DevAssistant",
    Instructions = """
        You are a senior .NET developer assistant. Help users with code 
        reviews, architecture decisions, and debugging. Always provide 
        code examples when relevant. Use your available tools to look up 
        current information when needed.
        """,
    Kernel = kernel,
    Arguments = new KernelArguments(settings)
};

var history = new ChatHistory();

history.AddUserMessage("How should I structure a clean architecture project in .NET 8?");

await foreach (var message in agent.InvokeAsync(history))
{
    Console.WriteLine(message.Content);
    history.Add(message);
}
```

### Multi-Agent Collaboration

Where things get really powerful is when you have **multiple agents working together**. Semantic Kernel supports group chat patterns where agents with different specializations collaborate:

```csharp
#pragma warning disable SKEXP0110

var codeReviewer = new ChatCompletionAgent
{
    Name = "CodeReviewer",
    Instructions = """
        You review C# code for bugs, performance issues, and best practices.
        Be specific about what you find and suggest concrete fixes.
        """,
    Kernel = kernel
};

var securityAuditor = new ChatCompletionAgent
{
    Name = "SecurityAuditor",
    Instructions = """
        You focus exclusively on security vulnerabilities in code.
        Look for injection attacks, authentication issues, data exposure,
        and OWASP Top 10 violations.
        """,
    Kernel = kernel
};

var groupChat = new AgentGroupChat(codeReviewer, securityAuditor)
{
    ExecutionSettings = new AgentGroupChatSettings
    {
        TerminationStrategy = new MaximumIterationTerminationStrategy(4)
    }
};

groupChat.AddChatMessage(
    new ChatMessageContent(AuthorRole.User, "Review this code: ...")
);

await foreach (var message in groupChat.InvokeAsync())
{
    Console.WriteLine($"[{message.AuthorName}]: {message.Content}");
}
```

This pattern is incredibly useful for complex tasks where different perspectives or expertise areas need to weigh in. Each agent operates with its own system prompt and can have its own set of plugins.

## Real-World Example: Building a Project Documentation Assistant

Let's tie everything together with a practical example — an AI assistant that helps developers understand a codebase by reading files and answering questions.

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

// Define our plugins
public class FileSystemPlugin
{
    private readonly string _rootPath;

    public FileSystemPlugin(string rootPath)
    {
        _rootPath = rootPath;
    }

    [KernelFunction("read_file")]
    [Description("Reads the contents of a file from the project directory")]
    public async Task<string> ReadFile(
        [Description("Relative path to the file")] string path)
    {
        var fullPath = Path.Combine(_rootPath, path);

        if (!File.Exists(fullPath))
            return $"File not found: {path}";

        var content = await File.ReadAllTextAsync(fullPath);

        // Truncate very large files
        if (content.Length > 8000)
            content = content[..8000] + "\n... [truncated]";

        return content;
    }

    [KernelFunction("list_files")]
    [Description("Lists files in a directory, optionally filtered by extension")]
    public string ListFiles(
        [Description("Relative directory path")] string directory,
        [Description("File extension filter like '.cs' or '.json'")] string? extension = null)
    {
        var fullPath = Path.Combine(_rootPath, directory);

        if (!Directory.Exists(fullPath))
            return $"Directory not found: {directory}";

        var files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(_rootPath, f))
            .Where(f => extension is null || f.EndsWith(extension))
            .Take(50);

        return string.Join("\n", files);
    }
}

public class DocumentationPlugin
{
    [KernelFunction("generate_summary")]
    [Description("Generates a structured markdown summary for documentation")]
    public string GenerateSummaryTemplate(
        [Description("Name of the component")] string componentName,
        [Description("Brief description")] string description,
        [Description("Key responsibilities as comma-separated values")] string responsibilities)
    {
        var items = responsibilities.Split(',', StringSplitOptions.TrimEntries);
        var bullets = string.Join("\n", items.Select(r => $"- {r}"));

        return $"""
            ## {componentName}

            {description}

            ### Responsibilities
            {bullets}

            ---
            *Generated documentation — review and expand as needed.*
            """;
    }
}

// Wire it all up
var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o",
    endpoint: "https://your-resource.openai.azure.com/",
    apiKey: "your-api-key"
);

builder.Plugins.AddFromObject(new FileSystemPlugin("./src"));
builder.Plugins.AddFromType<DocumentationPlugin>();

var kernel = builder.Build();

var chatService = kernel.GetRequiredService<IChatCompletionService>();
var history = new ChatHistory();

history.AddSystemMessage("""
    You are a codebase documentation assistant. You help developers understand 
    projects by reading source files and explaining architecture, patterns, 
    and design decisions. 
    
    When asked about code, use your tools to read the actual files rather 
    than guessing. Be specific and reference actual code when possible.
    Generate documentation artifacts when asked.
    """);

var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

Console.WriteLine("Documentation Assistant ready. Ask me about your codebase!");
Console.WriteLine("Type 'exit' to quit.\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    history.AddUserMessage(input);

    var response = await chatService.GetChatMessageContentAsync(
        history,
        executionSettings: settings,
        kernel: kernel
    );

    Console.WriteLine($"\nAssistant: {response.Content}\n");
    history.AddAssistantMessage(response.Content ?? "");
}
```

This assistant can:

- **List and read files** from your project directory
- **Answer questions** about the codebase by reading actual source files
- **Generate documentation** in markdown format
- **Maintain conversation context** so follow-up questions work naturally

The AI automatically decides when to call `read_file`, `list_files`, or `generate_summary` based on what you ask. Ask it "What does the OrderService do?" and it'll read the file, analyze it, and explain it. Ask it to "Generate documentation for the authentication module" and it'll explore the files, understand the structure, and produce a formatted summary.

## Tips for Production

Before you ship your Semantic Kernel application, a few things I've learned the hard way:

**Use dependency injection properly.** In ASP.NET Core apps, register the Kernel and services in your DI container rather than creating them inline:

```csharp
builder.Services.AddKernel()
    .AddAzureOpenAIChatCompletion(
        deploymentName: "gpt-4o",
        endpoint: configuration["AzureOpenAI:Endpoint"]!,
        apiKey: configuration["AzureOpenAI:ApiKey"]!
    )
    .Plugins.AddFromType<TimePlugin>()
    .AddFromType<OrderPlugin>();
```

**Handle errors gracefully.** LLM calls can fail, timeout, or return unexpected results. Wrap your invocations in try-catch blocks and implement retry policies with Polly or the built-in resilience features.

**Monitor token usage.** Every prompt, every function description, and every piece of chat history consumes tokens. Use filters to log and track usage:

```csharp
kernel.FunctionInvocationFilters.Add(new LoggingFilter());
```

**Keep your function descriptions precise.** Vague descriptions lead to the AI calling functions incorrectly. Test your descriptions by asking: "If I only read the description, would I know exactly when and how to use this function?"

## Conclusion

Semantic Kernel is one of those libraries that fundamentally changes how you think about building applications. It's not just an API wrapper — it's an orchestration framework that lets you compose AI capabilities with traditional code in a way that's maintainable, testable, and production-ready.

What I love most about it is that it respects the .NET ecosystem. It uses patterns you already know — dependency injection, attributes, async/await, interfaces — and extends them into the AI world. You don't have to learn a completely new paradigm; you just add AI as another capability in your toolkit.

If you're building .NET applications and haven't explored Semantic Kernel yet, now is the time. The SDK is stable, the community is active, and the patterns it enables — from simple prompt orchestration to multi-agent collaboration — are becoming essential skills for modern developers.

Start small. Create a Kernel, register a plugin, and watch the AI call your code. Once that clicks, you'll start seeing opportunities to add intelligence everywhere in your applications.

The [official documentation](https://learn.microsoft.com/semantic-kernel/overview/) and the [GitHub repository](https://github.com/microsoft/semantic-kernel) are excellent resources to continue your journey. Happy building!
