## Introduction

Finding the perfect Christmas presents can be stressful. Between brainstorming gift ideas, comparing prices across stores, and making sure everything arrives on time, holiday shopping quickly becomes overwhelming. What if we could delegate these tasks to specialized AI agents that work together? In this post, we'll explore how to use Microsoft's Agent Framework to build a multi-agent system where each agent specializes in a specific task, from generating gift ideas to comparing prices, all coordinated through workflows.

## Festive Tech Calendar 2025

<p align="center">
<img src="https://sessionize.com/image/49aa-1140o400o3-sdJUGhdR3FCmm1KuPRM3D3.png"/>
</p>

This project is part of my session at the **Festive Tech Calendar 2025**, an amazing community event celebrating technology during the holiday season. You can find more about the event on [Sessionize](https://sessionize.com/festive-tech-calendar-2025/).

## What is Microsoft's Agent Framework?

The Agent Framework is Microsoft's solution for building, orchestrating, and deploying AI agents and multi-agent systems. It provides a flexible foundation for creating agents that can work sequentially, concurrently, or through handoff patterns. The framework supports OpenAI, Azure OpenAI, and Microsoft Foundry models, making it incredibly versatile.

Key features include:
- **Multi-Agent Orchestration**: Group chat, sequential, concurrent, and handoff patterns
- **Plugin Ecosystem**: Extend with native functions, OpenAPI, and Model Context Protocol (MCP)
- **Workflow Support**: Build complex agent pipelines with executors and edges

## Prerequisites

Before diving into the code, make sure you have:

- .NET 9
- Azure OpenAI access (or OpenAI API key)
- Visual Studio or Visual Studio Code

Install the required packages (note the `--prerelease` flag is required while in preview):

```bash
dotnet add package Microsoft.Agents.AI.OpenAI --prerelease
dotnet add package Microsoft.Agents.AI.Workflows --prerelease
dotnet add package Azure.AI.OpenAI
dotnet add package Azure.Identity
dotnet add package Azure.AI.Agents.Persistent --prerelease
```

## The Gift Shopping Agents

Our Christmas present finder will consist of three specialized agents working together:

1. **Gift Idea Agent** - Generates creative gift suggestions based on the recipient's profile
2. **Price Comparison Agent** - Finds the best prices across different stores
3. **Summary Agent** - Compiles the final recommendations

## The Models

Let's start by defining our data models that will flow through the agent pipeline:

```csharp
public class GiftRecipient
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public List<string> Interests { get; set; } = [];
    public decimal Budget { get; set; }
}

public class GiftIdea
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal EstimatedPrice { get; set; }
}

public class PriceResult
{
    public string GiftName { get; set; } = string.Empty;
    public string Store { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class GiftRecommendation
{
    public GiftIdea Gift { get; set; } = new();
    public List<PriceResult> Prices { get; set; } = [];
    public PriceResult? BestDeal { get; set; }
}
```

## Building the Agents

The beauty of the Agent Framework is how easy it is to create specialized agents. Each agent is simply a `ChatClientAgent` with a specific system prompt that defines its expertise.

```csharp
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

public static class ChristmasAgentFactory
{
    public static AIAgent CreateGiftIdeaAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient,
            @"You are a creative Christmas gift advisor. When given information about a person 
            (age, interests, budget), you suggest thoughtful and personalized gift ideas.
            
            For each suggestion, provide:
            - Gift name
            - Brief description of why it's a good fit
            - Category (Electronics, Books, Fashion, Home, Experience, etc.)
            - Estimated price range
            
            Always suggest 3-5 gift options within the specified budget.
            Format your response as a structured list.");
    }

    public static AIAgent CreatePriceComparisonAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient,
            @"You are a price comparison specialist. Given a list of gift ideas, 
            you research and compare prices from different online retailers.
            
            For each gift, provide:
            - Store name (Amazon, Best Buy, Target, Walmart, etc.)
            - Current price
            - Any available discounts or deals
            
            Always highlight the best deal for each item.
            Consider shipping costs and delivery times for Christmas.");
    }

    public static AIAgent CreateSummaryAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient,
            @"You are a gift recommendation summarizer. Take the gift ideas and price 
            comparisons and create a final recommendation report.
            
            Your summary should:
            - Rank gifts by value (quality vs price)
            - Highlight the top pick with reasoning
            - Include total cost estimate
            - Add any tips for holiday shopping
            
            Make the summary cheerful and festive!");
    }
}
```

## Creating the Workflow

Now comes the fun part, connecting our agents into a sequential workflow. The Agent Framework provides `WorkflowBuilder` and `AgentWorkflowBuilder` to compose agents into different patterns.

```csharp
public static class ChristmasGiftWorkflow
{
    public static async Task RunAsync()
    {
        // Set up the Azure OpenAI client
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") 
            ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
        var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") 
            ?? "gpt-4o-mini";
        
        var chatClient = new AzureOpenAIClient(
            new Uri(endpoint), 
            new DefaultAzureCredential())
            .GetChatClient(deploymentName)
            .AsIChatClient();

        // Create our specialized agents
        AIAgent giftIdeaAgent = ChristmasAgentFactory.CreateGiftIdeaAgent(chatClient);
        AIAgent priceAgent = ChristmasAgentFactory.CreatePriceComparisonAgent(chatClient);
        AIAgent summaryAgent = ChristmasAgentFactory.CreateSummaryAgent(chatClient);

        // Build a sequential workflow: Ideas -> Prices -> Summary
        var workflow = AgentWorkflowBuilder.BuildSequential(
            "ChristmasGiftFinder",
            [giftIdeaAgent, priceAgent, summaryAgent]);

        // Define our gift recipient
        var recipient = new GiftRecipient
        {
            Name = "Mom",
            Age = 55,
            Interests = ["gardening", "cooking", "reading", "yoga"],
            Budget = 100
        };

        var prompt = $@"Find Christmas gifts for {recipient.Name}, 
            age {recipient.Age}, who enjoys {string.Join(", ", recipient.Interests)}. 
            Budget: ${recipient.Budget}";

        // Execute the workflow with streaming
        await using StreamingRun run = await InProcessExecution.StreamAsync(
            workflow, 
            new ChatMessage(ChatRole.User, prompt));

        // Send the turn token to start processing
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        // Watch for workflow events
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is AgentRunUpdateEvent agentUpdate)
            {
                Console.Write(agentUpdate.Data);
            }
            else if (evt is WorkflowOutputEvent outputEvent)
            {
                Console.WriteLine("\n🎄 Final Recommendations:");
                Console.WriteLine(outputEvent.Data);
            }
        }
    }
}
```

## Running Agents Concurrently

What if we want to search for gifts for multiple people at once? The Agent Framework supports concurrent execution, which is perfect for this scenario:

```csharp
public static async Task FindGiftsForEveryoneAsync(IChatClient chatClient, List<GiftRecipient> recipients)
{
    // Create an agent for each recipient
    var agents = recipients.Select(r => new ChatClientAgent(
        chatClient,
        $@"Find the perfect Christmas gift for {r.Name} (age {r.Age}), 
           who loves {string.Join(", ", r.Interests)}. Budget: ${r.Budget}.
           Provide one well-researched recommendation with price."
    )).ToList();

    // Build a concurrent workflow - all agents run in parallel
    var workflow = AgentWorkflowBuilder.BuildConcurrent(
        "FamilyGiftFinder",
        agents);

    await using StreamingRun run = await InProcessExecution.StreamAsync(
        workflow, 
        new ChatMessage(ChatRole.User, "Find gifts now!"));

    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

    await foreach (WorkflowEvent evt in run.WatchStreamAsync())
    {
        if (evt is WorkflowOutputEvent output)
        {
            Console.WriteLine("🎁 All gift recommendations ready!");
            Console.WriteLine(output.Data);
        }
    }
}
```

## Custom Executors for More Control

For more complex scenarios, you can create custom executors that give you fine-grained control over the workflow:

```csharp
public sealed class GiftValidatorExecutor : Executor<List<ChatMessage>, List<ChatMessage>>
{
    public GiftValidatorExecutor() : base("GiftValidator") { }

    public override async ValueTask<List<ChatMessage>> HandleAsync(
        List<ChatMessage> messages,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var lastMessage = messages.LastOrDefault()?.Text ?? "";
        
        // Validate that suggestions are within budget
        if (lastMessage.Contains("over budget", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("⚠️ Some suggestions exceeded budget, filtering...");
            // Add validation logic here
        }

        Console.WriteLine("✅ Gift suggestions validated!");
        return messages;
    }
}
```

You can then insert this executor into your workflow:

```csharp
var validator = new GiftValidatorExecutor();

var workflow = new WorkflowBuilder(giftIdeaAgent)
    .AddEdge(giftIdeaAgent, validator)
    .AddEdge(validator, priceAgent)
    .AddEdge(priceAgent, summaryAgent)
    .WithOutputFrom(summaryAgent)
    .Build();
```

## Adding Real Web Search with Bing Grounding

So far, our agents generate responses based on the AI model's knowledge. But what if we want to search the web for actual product prices and availability? This is where **Grounding with Bing Search** comes in. It's a tool available in Microsoft Foundry (formerly Azure AI Foundry) that allows your agents to incorporate real-time public web data when generating responses.

First, you'll need to create a **Grounding with Bing Search** resource in the [Azure Portal](https://portal.azure.com/#create/Microsoft.BingGroundingSearch). Make sure to create it in the same resource group as your AI project.

### Setting Up Grounding with Bing Search

The beauty of Grounding with Bing Search is that it integrates directly with Azure AI Agents. The agent decides when to use the search tool based on the user's query, searches the web, and uses the results to generate a grounded response.

```csharp
using Azure.AI.Agents.Persistent;
using Azure.Identity;

public static class BingGroundingSetup
{
    public static async Task<PersistentAgent> CreateAgentWithBingGroundingAsync(
        string projectEndpoint,
        string modelDeploymentName,
        string bingConnectionId)
    {
        // Create the Persistent Agents client
        var agentClient = new PersistentAgentsClient(projectEndpoint, new DefaultAzureCredential());

        // Configure the Bing Grounding tool
        var bingGroundingTool = new BingGroundingToolDefinition(
            new BingGroundingSearchToolParameters(
                [new BingGroundingSearchConfiguration(bingConnectionId)]
            )
        );

        // Create the agent with Bing Grounding enabled
        var agent = await agentClient.Administration.CreateAgentAsync(
            model: modelDeploymentName,
            name: "ChristmasPriceHunter",
            instructions: @"You are a Christmas gift price comparison specialist.
                
                When given gift ideas, use Bing to search for:
                - Current prices at major retailers (Amazon, Best Buy, Target, Walmart)
                - Available discounts and holiday deals
                - Shipping times to ensure delivery before Christmas
                
                Always provide URLs to the products you find.
                Highlight the best deals and recommend where to buy.",
            tools: [bingGroundingTool]
        );

        return agent;
    }
}
```

### Creating a Price Comparison Agent with Real Search

Now let's create a complete price comparison agent that searches the web for real product information:

```csharp
using Azure.AI.Agents.Persistent;
using Azure.Identity;

public class ChristmasPriceAgent
{
    private readonly PersistentAgentsClient _client;
    private readonly string _modelDeployment;
    private readonly string _bingConnectionId;

    public ChristmasPriceAgent(string projectEndpoint, string modelDeployment, string bingConnectionId)
    {
        _client = new PersistentAgentsClient(projectEndpoint, new DefaultAzureCredential());
        _modelDeployment = modelDeployment;
        _bingConnectionId = bingConnectionId;
    }

    public async Task<string> FindGiftPricesAsync(string giftIdeas)
    {
        // Create agent with Bing Grounding
        var bingTool = new BingGroundingToolDefinition(
            new BingGroundingSearchToolParameters(
                [new BingGroundingSearchConfiguration(_bingConnectionId)]
            )
        );

        var agent = await _client.Administration.CreateAgentAsync(
            model: _modelDeployment,
            name: "PriceHunter",
            instructions: @"Search the web for current prices on the given gift ideas. 
                For each gift, find prices from at least 2-3 different stores.
                Include direct links to the products.
                Note any Christmas sales or discounts available.",
            tools: [bingTool]
        );

        try
        {
            // Create a thread for the conversation
            var thread = await _client.Threads.CreateThreadAsync();

            // Add the gift ideas as a message
            await _client.Messages.CreateMessageAsync(
                thread.Id,
                MessageRole.User,
                $"Find current prices for these gift ideas: {giftIdeas}"
            );

            // Run the agent
            var run = await _client.Runs.CreateRunAsync(thread.Id, agent.Id);

            // Wait for completion
            do
            {
                await Task.Delay(500);
                run = await _client.Runs.GetRunAsync(thread.Id, run.Id);
            }
            while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress);

            // Get the response
            var messages = _client.Messages.GetMessages(thread.Id);
            var response = messages
                .Where(m => m.Role == MessageRole.Agent)
                .SelectMany(m => m.ContentItems)
                .OfType<MessageTextContent>()
                .FirstOrDefault();

            // Clean up
            await _client.Threads.DeleteThreadAsync(thread.Id);

            return response?.Text ?? "No results found.";
        }
        finally
        {
            // Clean up the agent
            await _client.Administration.DeleteAgentAsync(agent.Id);
        }
    }
}
```

### Integrating Bing Grounding into the Workflow

Here's how to use the Bing-powered price agent in a complete gift-finding workflow:

```csharp
public static async Task RunWithBingGroundingAsync()
{
    var projectEndpoint = Environment.GetEnvironmentVariable("PROJECT_ENDPOINT")!;
    var modelDeployment = Environment.GetEnvironmentVariable("MODEL_DEPLOYMENT_NAME") ?? "gpt-4o";
    var bingConnectionId = Environment.GetEnvironmentVariable("BING_CONNECTION_ID")!;
    // Connection ID format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.CognitiveServices/accounts/{account}/projects/{project}/connections/{connection}

    Console.WriteLine("🔍 Searching the web for real prices with Bing Grounding...\n");

    var priceAgent = new ChristmasPriceAgent(projectEndpoint, modelDeployment, bingConnectionId);

    // First, generate gift ideas (could come from another agent)
    var giftIdeas = "1. Milwaukee cordless drill set, 2. Weber portable grill, 3. Carhartt beanie";

    // Search for real prices
    var priceResults = await priceAgent.FindGiftPricesAsync(giftIdeas);

    Console.WriteLine("📊 Price Comparison Results:");
    Console.WriteLine(priceResults);
    Console.WriteLine("\n🎁 Happy Shopping! 🎁");
}
```

### Processing Citations from Bing Results

One important aspect of Grounding with Bing Search is that responses include citations with links to the source websites. Here's how to extract and display them:

```csharp
public static void ProcessBingGroundingResponse(IEnumerable<PersistentThreadMessage> messages)
{
    foreach (var message in messages.Where(m => m.Role == MessageRole.Agent))
    {
        foreach (var content in message.ContentItems)
        {
            if (content is MessageTextContent textContent)
            {
                var response = textContent.Text;

                // Process URL citations
                if (textContent.Annotations != null)
                {
                    foreach (var annotation in textContent.Annotations)
                    {
                        if (annotation is MessageTextUriCitationAnnotation uriAnnotation)
                        {
                            // Replace citation placeholder with markdown link
                            response = response.Replace(
                                uriAnnotation.Text,
                                $" [{uriAnnotation.UriCitation.Title}]({uriAnnotation.UriCitation.Uri})"
                            );
                        }
                    }
                }

                Console.WriteLine(response);
            }
        }
    }
}
```

Now when the price comparison agent runs, it uses **Grounding with Bing Search** to find real product listings, current prices, and available deals from the live web. The agent automatically decides when to search based on the query and returns grounded responses with proper citations.

## The Complete Program

Here's how everything comes together in `Program.cs`:

```csharp
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

Console.WriteLine("🎄 Christmas Gift Finder - Powered by AI Agents 🎄\n");

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

var chatClient = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
    .GetChatClient(deployment)
    .AsIChatClient();

// Create the agent team
var ideaAgent = ChristmasAgentFactory.CreateGiftIdeaAgent(chatClient);
var priceAgent = ChristmasAgentFactory.CreatePriceComparisonAgent(chatClient);
var summaryAgent = ChristmasAgentFactory.CreateSummaryAgent(chatClient);

// Build the workflow
var workflow = AgentWorkflowBuilder.BuildSequential(
    "ChristmasGiftWorkflow",
    [ideaAgent, priceAgent, summaryAgent]);

Console.Write("Who are you shopping for? ");
var recipientName = Console.ReadLine() ?? "Someone special";

Console.Write("What are their interests? ");
var interests = Console.ReadLine() ?? "general";

Console.Write("What's your budget? $");
var budget = Console.ReadLine() ?? "50";

var prompt = $"Find Christmas gifts for {recipientName} who enjoys {interests}. Budget: ${budget}";

Console.WriteLine("\n🔍 Searching for the perfect gifts...\n");

await using var run = await InProcessExecution.StreamAsync(
    workflow,
    new ChatMessage(ChatRole.User, prompt));

await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

await foreach (var evt in run.WatchStreamAsync())
{
    switch (evt)
    {
        case AgentRunUpdateEvent update:
            Console.Write(update.Update.Text);
            break;
        case WorkflowOutputEvent output:
            Console.WriteLine("\n\n🎁 Happy Shopping! 🎁");
            break;
    }
}
```

## Conclusion

Microsoft's Agent Framework makes it surprisingly easy to build multi-agent systems where each agent specializes in a specific task. By coordinating these agents through workflows, we can tackle complex problems like Christmas shopping in a structured and efficient way.

What makes this even more powerful is the ability to add real-world capabilities through tools. By integrating **Grounding with Bing Search** from Microsoft Foundry, our price comparison agent can actually search the web for current prices, deals, and availability—turning a simple AI chatbot into a truly useful shopping assistant with proper citations.

The framework's support for sequential, concurrent, and handoff patterns means you can design agent systems that match your exact needs. Whether it's finding gifts, planning trips, or any other multi-step task, the Agent Framework provides the building blocks to make it happen.

This holiday season, let the AI agents handle the research while you focus on wrapping presents and enjoying time with family!

## Source Code

The concepts shown in this post are based on the official Agent Framework samples. You can explore more examples on the [Microsoft Agent Framework GitHub repository](https://github.com/microsoft/agent-framework).

Happy holidays and happy coding! 🎄
