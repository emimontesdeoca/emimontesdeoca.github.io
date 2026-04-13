---
title: "Building Multi-Agent AI Systems with Microsoft's Agent Framework"
date: 2025-12-01T00:00:00Z
draft: false
tags: ["AI", ".NET", "Agent Framework", "Semantic Kernel"]
slug: "microsoft-agent-framework-multi-agent"
description: "A practical guide to building, orchestrating, and deploying multi-agent AI systems using Microsoft's Agent Framework in .NET."
---

## Introduction

We've entered the era of multi-agent AI systems. Instead of a single monolithic AI handling everything, the industry is moving toward specialized agents that collaborate to solve complex problems — much like a well-organized team of experts. One agent researches, another analyzes, a third writes, and a coordinator keeps everyone on track.

If you've worked with large language models, you've likely hit the ceiling of what a single prompt can do. Context windows fill up, instructions get tangled, and quality degrades. Multi-agent architectures solve this by decomposing complex tasks into focused responsibilities, where each agent is an expert at one thing.

Microsoft's Agent Framework, part of the broader Semantic Kernel ecosystem, gives .NET developers a first-class toolkit for building exactly these kinds of systems. In this post, we'll go from zero to a fully working multi-agent pipeline, covering the core concepts, orchestration patterns, and practical code you need to get started.

## What is Microsoft's Agent Framework?

The Agent Framework is Microsoft's answer to building, orchestrating, and deploying AI agents and multi-agent systems in .NET. It sits alongside — and integrates deeply with — the Semantic Kernel, which has been Microsoft's open-source SDK for AI orchestration since 2023.

Think of it this way: **Semantic Kernel** gives you the primitives (kernels, plugins, memory, planners), while the **Agent Framework** gives you higher-level abstractions specifically designed for agent-to-agent communication and coordination.

The framework supports multiple model providers including Azure OpenAI, OpenAI, and models hosted on Azure AI Foundry. It's model-agnostic in design but deeply integrated with the Azure ecosystem, which makes it particularly compelling for enterprise scenarios.

Key capabilities include:

- **Multiple agent types**: `ChatCompletionAgent`, `OpenAIAssistantAgent`, and `AzureAIAgent` for different backends
- **Orchestration patterns**: Sequential, concurrent, handoff, and group chat workflows
- **Plugin ecosystem**: Extend agents with native C# functions, OpenAPI specs, and Model Context Protocol (MCP) tools
- **Conversation management**: Built-in threading, history management, and termination strategies
- **Observability**: Integration with OpenTelemetry for tracing agent interactions

## Key Concepts

Before we write any code, let's establish the vocabulary. The Agent Framework revolves around a few core abstractions.

### Agents

An agent is an entity backed by an AI model, configured with specific instructions (a system prompt), a name, and optionally a set of plugins or tools. Each agent is a specialist — you define what it knows, what it can do, and how it should behave.

### ChatCompletionAgent

The most straightforward agent type. It wraps a chat completion endpoint (Azure OpenAI, OpenAI, etc.) and maintains a conversation. It's stateless between invocations — you provide the history, and it responds. This makes it lightweight and easy to reason about.

```csharp
ChatCompletionAgent agent = new()
{
    Name = "Reviewer",
    Instructions = "You are a senior code reviewer. Analyze the provided code for bugs, security issues, and style violations. Be concise and actionable.",
    Kernel = kernel
};
```

### OpenAIAssistantAgent

This agent type leverages the OpenAI Assistants API, which provides server-side conversation state, file handling, and code interpretation. It's more heavyweight but gives you persistent threads and built-in tools like code interpreter and file search.

```csharp
OpenAIAssistantAgent agent = await OpenAIAssistantAgent.CreateAsync(
    clientProvider: clientProvider,
    definition: new OpenAIAssistantDefinition("gpt-4o")
    {
        Name = "DataAnalyst",
        Instructions = "You analyze datasets and produce statistical summaries."
    }
);
```

### AgentGroupChat

This is the orchestrator. `AgentGroupChat` manages multi-turn conversations between multiple agents, controlling who speaks next, when the conversation ends, and how history is shared. It's where the magic of multi-agent collaboration happens.

## Orchestration Patterns

The framework supports four primary orchestration patterns, each suited to different problems.

### Sequential

Agents execute one after another in a defined order. Agent A's output feeds into Agent B, whose output feeds into Agent C. This is ideal for pipelines: draft → review → edit → publish.

```csharp
// Conceptual flow
var draft = await writerAgent.InvokeAsync("Write a blog post about .NET 9");
var reviewed = await reviewerAgent.InvokeAsync($"Review this: {draft}");
var edited = await editorAgent.InvokeAsync($"Edit based on feedback: {reviewed}");
```

### Concurrent

Multiple agents work on the same input simultaneously. You fan out the work and then aggregate results. Great for getting diverse perspectives — like having three reviewers look at the same pull request.

### Handoff

An agent decides to transfer control to another agent based on the conversation context. This mimics how a customer service team works: the front-line agent handles basic queries and escalates to specialists when needed.

### Group Chat

Multiple agents participate in an open conversation, taking turns based on a selection strategy. The `AgentGroupChat` class implements this pattern with configurable turn-taking and termination logic.

## Building Your First Agent

Let's get practical. Here's how to build your first agent step by step.

### Prerequisites

You'll need:

- .NET 9 SDK
- An Azure OpenAI resource with a deployed model (e.g., `gpt-4o`)
- Visual Studio or VS Code

### Setting Up the Project

Create a new console application and install the required packages:

```bash
dotnet new console -n AgentDemo
cd AgentDemo
dotnet add package Microsoft.SemanticKernel --prerelease
dotnet add package Microsoft.SemanticKernel.Agents.Core --prerelease
dotnet add package Azure.AI.OpenAI
dotnet add package Azure.Identity
```

### Creating a Simple Agent

First, set up the kernel with your Azure OpenAI configuration:

```csharp
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o",
    endpoint: "https://your-resource.openai.azure.com/",
    credentials: new DefaultAzureCredential()
);

Kernel kernel = builder.Build();
```

Now create an agent and invoke it:

```csharp
ChatCompletionAgent agent = new()
{
    Name = "TechWriter",
    Instructions = """
        You are a technical writer specializing in software documentation.
        Write clear, concise content aimed at experienced developers.
        Use code examples when appropriate.
        Always structure your output with headings and bullet points.
        """,
    Kernel = kernel
};

ChatHistory history = new();
history.AddUserMessage("Explain dependency injection in .NET in 200 words.");

await foreach (ChatMessageContent response in agent.InvokeAsync(history))
{
    Console.WriteLine(response.Content);
}
```

That's it. You have a working agent. But the real power comes when agents work together.

## Multi-Agent Orchestration with AgentGroupChat

Let's build something more interesting — a group chat where multiple agents collaborate. The `AgentGroupChat` class manages the conversation flow, including who speaks next and when to stop.

### Defining the Agents

We'll create three agents: a writer, a reviewer, and an editor.

```csharp
ChatCompletionAgent writer = new()
{
    Name = "Writer",
    Instructions = """
        You are a content writer. When given a topic, produce a well-structured
        draft. Focus on clarity and technical accuracy.
        When you receive feedback from the Reviewer, incorporate it into a
        revised draft.
        """,
    Kernel = kernel
};

ChatCompletionAgent reviewer = new()
{
    Name = "Reviewer",
    Instructions = """
        You are a content reviewer. Analyze drafts for technical accuracy,
        clarity, and completeness. Provide specific, actionable feedback.
        Do NOT rewrite the content — only provide feedback.
        When the content meets your standards, respond with: APPROVED
        """,
    Kernel = kernel
};

ChatCompletionAgent editor = new()
{
    Name = "Editor",
    Instructions = """
        You are an editor. Once content is approved by the Reviewer,
        polish it for grammar, tone, and formatting.
        Output only the final polished version.
        When you have produced the final version, respond with: COMPLETE
        """,
    Kernel = kernel
};
```

### Setting Up the Group Chat

The `AgentGroupChat` needs two key configurations: a **selection strategy** (who speaks next) and a **termination strategy** (when to stop).

```csharp
AgentGroupChat chat = new(writer, reviewer, editor)
{
    ExecutionSettings = new()
    {
        SelectionStrategy = new SequentialSelectionStrategy(),
        TerminationStrategy = new ApprovalTerminationStrategy()
        {
            Agents = [editor],
            MaximumIterations = 12
        }
    }
};
```

### Custom Termination Strategy

The termination strategy defines when the conversation ends. Here's a custom one that looks for the "COMPLETE" keyword:

```csharp
class ApprovalTerminationStrategy : TerminationStrategy
{
    protected override Task<bool> ShouldAgentTerminateAsync(
        Agent agent,
        IReadOnlyList<ChatMessageContent> history,
        CancellationToken cancellationToken = default)
    {
        bool isComplete = history
            .Last()
            .Content?
            .Contains("COMPLETE", StringComparison.OrdinalIgnoreCase) ?? false;

        return Task.FromResult(isComplete);
    }
}
```

### Running the Conversation

Kick off the conversation with a user message and let the agents collaborate:

```csharp
chat.AddChatMessage(
    new ChatMessageContent(AuthorRole.User,
        "Write a 300-word technical overview of gRPC vs REST for microservices.")
);

await foreach (ChatMessageContent message in chat.InvokeAsync())
{
    Console.WriteLine($"[{message.AuthorName}]: {message.Content}");
    Console.WriteLine("---");
}
```

The flow will look something like this:

1. **Writer** produces a draft
2. **Reviewer** provides feedback
3. **Writer** revises based on feedback
4. **Reviewer** says "APPROVED"
5. **Editor** polishes and says "COMPLETE"
6. Conversation terminates

This back-and-forth continues automatically until the termination condition is met or `MaximumIterations` is reached.

## Plugins and Tools

Agents become truly powerful when they can interact with external systems. The Agent Framework supports three main extension mechanisms.

### Native Functions (Kernel Plugins)

You can give agents access to C# methods as tools. The agent will call these functions when it determines they're needed:

```csharp
public class ContentToolsPlugin
{
    [KernelFunction("word_count")]
    [Description("Counts the number of words in the provided text.")]
    public int WordCount([Description("The text to count words in")] string text)
    {
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    [KernelFunction("check_readability")]
    [Description("Calculates a readability score for the given text.")]
    public string CheckReadability([Description("The text to analyze")] string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var sentences = text.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries).Length;

        if (sentences == 0) return "Unable to calculate — no sentences found.";

        double avgWordsPerSentence = (double)words / sentences;

        return avgWordsPerSentence switch
        {
            < 15 => $"Easy to read (avg {avgWordsPerSentence:F1} words/sentence)",
            < 25 => $"Moderate difficulty (avg {avgWordsPerSentence:F1} words/sentence)",
            _ => $"Difficult to read (avg {avgWordsPerSentence:F1} words/sentence). Consider shorter sentences."
        };
    }
}
```

Register the plugin on the kernel before creating the agent:

```csharp
kernel.Plugins.AddFromType<ContentToolsPlugin>();

ChatCompletionAgent analyst = new()
{
    Name = "ContentAnalyst",
    Instructions = "Analyze content using available tools. Report word count and readability.",
    Kernel = kernel,
    Arguments = new KernelArguments(
        new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        })
};
```

### Model Context Protocol (MCP)

MCP is an open standard for connecting AI models to external tools and data sources. The Agent Framework supports MCP, meaning your agents can use tools exposed by any MCP-compliant server. This opens the door to file systems, databases, APIs, and more — all through a standardized interface.

```csharp
// Example: Adding an MCP server for file operations
kernel.Plugins.AddFromMcpServer("filesystem",
    new Uri("http://localhost:3000/mcp"));
```

This is particularly exciting because it means your agents aren't limited to what you build — they can tap into an ecosystem of MCP tools that others develop and share.

## Real-World Example: Content Review Pipeline

Let's put everything together with a practical scenario. Imagine you're building an internal tool that automates content review for a documentation team. The pipeline has four stages:

1. **Researcher** — Gathers relevant technical information
2. **Writer** — Produces a draft based on research
3. **Reviewer** — Checks for accuracy and completeness
4. **Publisher** — Formats and prepares the final output

Here's a condensed implementation:

```csharp
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

// Build the kernel
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o",
    endpoint: "https://your-resource.openai.azure.com/",
    credentials: new DefaultAzureCredential()
);
Kernel kernel = builder.Build();

// Define specialized agents
ChatCompletionAgent researcher = new()
{
    Name = "Researcher",
    Instructions = """
        You are a technical researcher. Given a topic, identify the key
        concepts, recent developments, and important details that should
        be covered. Output a structured research brief.
        """,
    Kernel = kernel
};

ChatCompletionAgent writer = new()
{
    Name = "Writer",
    Instructions = """
        You are a technical writer. Using the research brief provided,
        write a comprehensive, well-structured article. Include code
        examples where relevant. Target audience: experienced developers.
        """,
    Kernel = kernel
};

ChatCompletionAgent reviewer = new()
{
    Name = "Reviewer",
    Instructions = """
        You are a senior technical reviewer. Check the article for:
        - Technical accuracy
        - Completeness relative to the research brief
        - Code correctness
        - Clarity and structure
        If everything looks good, respond with APPROVED.
        Otherwise, provide specific feedback for revision.
        """,
    Kernel = kernel
};

ChatCompletionAgent publisher = new()
{
    Name = "Publisher",
    Instructions = """
        You are a content publisher. Once the article is approved,
        format it with proper markdown, add a summary at the top,
        and ensure consistent formatting throughout.
        End your response with COMPLETE.
        """,
    Kernel = kernel
};

// Configure the group chat
AgentGroupChat chat = new(researcher, writer, reviewer, publisher)
{
    ExecutionSettings = new()
    {
        SelectionStrategy = new SequentialSelectionStrategy(),
        TerminationStrategy = new ApprovalTerminationStrategy()
        {
            Agents = [publisher],
            MaximumIterations = 15
        }
    }
};

// Start the pipeline
chat.AddChatMessage(new ChatMessageContent(
    AuthorRole.User,
    "Create a technical article about implementing health checks in ASP.NET Core microservices."
));

await foreach (ChatMessageContent message in chat.InvokeAsync())
{
    Console.ForegroundColor = message.AuthorName switch
    {
        "Researcher" => ConsoleColor.Cyan,
        "Writer" => ConsoleColor.Green,
        "Reviewer" => ConsoleColor.Yellow,
        "Publisher" => ConsoleColor.Magenta,
        _ => ConsoleColor.White
    };

    Console.WriteLine($"\n{'='new string('=', 60)}");
    Console.WriteLine($"[{message.AuthorName}]");
    Console.WriteLine(new string('=', 60));
    Console.WriteLine(message.Content);
}

Console.ResetColor();
```

This pipeline produces a fully researched, written, reviewed, and formatted article — all through agent collaboration. Each agent focuses on what it does best, and the `AgentGroupChat` coordinates the workflow.

## Best Practices

After building several multi-agent systems, here are the patterns and practices I've found most valuable.

### Error Handling

Always set `MaximumIterations` on your termination strategy. Without it, agents can enter infinite loops — especially when a reviewer keeps finding issues and a writer keeps revising without improvement.

```csharp
TerminationStrategy = new ApprovalTerminationStrategy()
{
    MaximumIterations = 12 // Safety net
}
```

Wrap your agent invocations in try-catch blocks. API rate limits, network issues, and model errors are all realities of production systems:

```csharp
try
{
    await foreach (var message in chat.InvokeAsync(cancellationToken))
    {
        // Process messages
    }
}
catch (HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
{
    // Implement retry with exponential backoff
    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
}
```

### Observability

The Agent Framework integrates with OpenTelemetry, which means you can trace every agent interaction, tool call, and token usage. This is essential for debugging multi-agent workflows where it's not always obvious which agent caused an issue.

Set up basic telemetry by adding the Semantic Kernel telemetry packages and configuring your preferred exporter (Application Insights, Jaeger, etc.):

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource("Microsoft.SemanticKernel*");
        tracing.AddOtlpExporter();
    });
```

### Cost Management

Multi-agent systems multiply your API costs — every agent turn is an API call, and group chats can generate many turns. Some strategies to keep costs under control:

- **Use cheaper models for simpler agents**: Not every agent needs GPT-4o. A reviewer might work fine with a smaller model, while only the writer needs the heavy hitter.
- **Limit history**: Use `ReducedHistoryCount` in your execution settings to cap how much conversation context each agent receives.
- **Set strict iteration limits**: Prevent runaway conversations with reasonable `MaximumIterations` values.
- **Cache when possible**: If an agent performs the same lookup repeatedly, cache the results in a plugin.

### Agent Design

- **Keep instructions focused**: Each agent should have a single, clear responsibility. Broad instructions lead to mediocre performance across all tasks.
- **Be explicit about output format**: Tell agents exactly how to structure their responses. This makes downstream parsing reliable.
- **Use termination keywords**: Define clear signals (like "APPROVED" or "COMPLETE") that agents use to indicate they're done. This makes termination strategies simple and predictable.

## Conclusion

Multi-agent AI systems represent a fundamental shift in how we build intelligent applications. Instead of wrestling with a single prompt to handle everything, we can decompose problems into specialized roles and let agents collaborate.

Microsoft's Agent Framework makes this practical for .NET developers. The abstractions are clean — agents, group chats, selection and termination strategies — and they compose naturally. Combined with the Semantic Kernel's plugin ecosystem and Azure's model hosting, you have a full stack for building production-grade multi-agent systems.

The framework is still evolving (many packages are in preview), but the core patterns are solid and the direction is clear. If you're building AI-powered applications in .NET, now is the time to start experimenting with multi-agent architectures.

Start small — build two agents that collaborate on a simple task. Once you see the quality improvement over single-agent approaches, you'll want to decompose everything into agent teams.

Happy coding!
