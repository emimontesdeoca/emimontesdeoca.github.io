# Building an AI-Powered RSS Feed Aggregator with .NET 9, Semantic Kernel, and Telegram Integration

## Introduction

As a Microsoft MVP and tech enthusiast, I constantly find myself drowning in the ocean of amazing content published across Microsoft's DevBlogs. From .NET announcements to Visual Studio updates, from Azure innovations to Semantic Kernel deep-dives – there's always something new and exciting happening in the Microsoft ecosystem.

The problem? **Keeping up with it all is nearly impossible.**

I wanted to stay on top of the latest announcements and share them with my network, but manually checking seven different RSS feeds, reading articles, crafting engaging social media posts, and keeping track of what I've already shared was becoming a full-time job in itself. Every morning I'd open multiple browser tabs, scan through dozens of articles, try to remember which ones I'd already shared, and then spend precious time writing posts about the ones that caught my attention.

So I did what any developer would do – **I automated it.**

In this comprehensive guide, I'll walk you through how I built an AI-powered RSS feed aggregator that monitors multiple Microsoft DevBlogs RSS feeds for new content, uses Azure OpenAI and Semantic Kernel to analyze articles and generate engaging posts, creates detailed markdown documentation for each analyzed article, sends notifications via Telegram so I can review and share the content, tracks everything to avoid duplicate posts, and runs automatically via GitHub Actions.

Let's dive deep into every aspect of this solution.



## The Story Behind This Project

### Living with Information Overload

Let me paint you a picture of my typical morning before I built this tool. I'd wake up, grab my coffee, and open my laptop to check what's new in the Microsoft developer ecosystem. First, I'd navigate to the main DevBlogs site to see if there were any major announcements. Then I'd check the .NET blog specifically because that's my primary technology stack. After that, I'd hop over to the Semantic Kernel blog since AI is becoming increasingly important. Visual Studio blog was next on the list because IDE updates can significantly impact my daily workflow. Then came the DevOps blog for CI/CD and GitHub-related news, followed by the All Things Azure blog for cloud infrastructure updates, and finally the Azure SQL blog for database innovations.

That's seven different feeds to check. Each of these blogs publishes multiple articles per week, sometimes multiple per day during major announcement periods like .NET Conf or Build. That's potentially dozens of articles to track, read, and share. And here's the thing – as someone who values sharing knowledge with the community, I didn't want to just read these articles. I wanted to share the most valuable ones with my network on LinkedIn, helping other developers stay informed too.

But crafting a good LinkedIn post takes time. You need to read the article thoroughly, understand the key points, think about why it matters to your audience, write an engaging hook, and format everything nicely. Multiply that by several articles per week, and you're looking at hours of work.

### What I Really Wanted

After dealing with this for months, I sat down and thought about what an ideal solution would look like. First and foremost, I never wanted to miss important announcements again. The system should automatically catch new articles as soon as they're published. I also wanted to save time on content creation by letting AI help craft engaging posts – not to replace my voice entirely, but to give me a solid starting point that I could customize.

Consistency was another big factor. I wanted to share content regularly without having to remember to do it manually every single day. The tracking aspect was crucial too – I needed a way to know what I've already shared to avoid posting duplicates and annoying my followers. Finally, I wanted to stay organized with a permanent record of everything I've processed, so I could look back and see what topics I've covered.

### The Solution Takes Shape

The solution I envisioned would run on a schedule using GitHub Actions, completely hands-free. It would fetch all seven feeds automatically without me having to open a single browser tab. The AI component would actually read and understand the content, then summarize it in a way that's useful for my audience. Instead of me having to write posts from scratch, it would create ready-to-share social media content that I could tweak if needed. Everything would be sent to my Telegram for review, so I could quickly glance at my phone and decide what to share. And of course, it would keep a permanent record of everything for future reference.



## Before We Start Building

### What You'll Need on Your Machine

To follow along with this tutorial, you'll need a few things installed on your development machine. The most important one is the .NET SDK version 9.0 or later. This is our runtime and provides all the build tools we need. If you don't have it installed, head over to dot.net and download the latest version. The installation is straightforward on Windows, macOS, or Linux.

You'll also want Git installed for version control. We'll be pushing our code to GitHub and using GitHub Actions for automation, so having Git set up locally is essential. Any recent version will work fine.

For your development environment, I recommend either Visual Studio or VS Code. Personally, I use VS Code for most of my work these days because it's lightweight and has excellent C# support through the C# Dev Kit extension. But if you're more comfortable with full Visual Studio, that works perfectly too.

### Services and Accounts You'll Need

Beyond the local tools, you'll need accounts with a few services. The most important one is Azure OpenAI, which powers our AI analysis. This is a pay-as-you-go service, but the costs are minimal for this use case – we're talking cents per article analyzed. If you don't have an Azure account, you can sign up for a free trial that includes some credits to get started.

For notifications, we'll use a Telegram Bot. The great thing about Telegram is that their bot API is completely free to use. You can create as many bots as you want and send unlimited messages. I'll walk you through the setup process later in this guide.

Finally, you'll need a GitHub account for hosting your code and running GitHub Actions. The free tier is more than sufficient for this project. GitHub gives you 2,000 minutes of Actions runtime per month on private repositories, and unlimited minutes on public repositories.

### The Libraries That Make This Possible

Our project relies on three main NuGet packages, each serving a specific purpose.

The first is HtmlAgilityPack, which is the gold standard for HTML parsing in .NET. When we fetch an article from a blog, we get back the full HTML of the page – including navigation menus, footers, advertisements, and all sorts of elements we don't care about. HtmlAgilityPack lets us parse that HTML and extract just the article content we need.

The second package is Microsoft.SemanticKernel, which is Microsoft's SDK for integrating AI models into applications. Think of it as the bridge between your .NET code and large language models like GPT-4. It handles all the complexity of API calls, token management, and response parsing, letting you focus on what you want the AI to actually do.

The third package is System.ServiceModel.Syndication, which provides built-in support for parsing RSS and Atom feeds. RSS might seem like old technology, but it's still the best way to get structured updates from blogs and news sites. This package turns raw XML feeds into strongly-typed C# objects that are easy to work with.



## Understanding the Architecture

### How the Pieces Fit Together

Before we dive into the code, let me explain how all the components work together. Understanding the big picture will make the implementation details much clearer.

At the highest level, we have our main Program.cs file that acts as the orchestrator. This is the entry point of our application, and it coordinates all the other components. When the application runs, it first loads configuration from environment variables – things like API keys and Telegram credentials. Then it goes out and fetches RSS feeds from all seven Microsoft DevBlogs sources. As it processes these feeds, it deduplicates articles to handle cases where the same article appears in multiple feeds. It checks each article against our tracking file to see if we've already processed it. For new articles, it hands them off to the AI analyzer for processing.

The ArticleAnalyzer class is where the AI magic happens. This component receives an article and does several things with it. First, it fetches the full HTML content from the article's URL. Then it extracts clean text from that HTML, removing all the navigation elements, scripts, and styling that we don't need. Once it has clean text, it sends that to Azure OpenAI through Semantic Kernel with a carefully crafted prompt. The AI analyzes the article and returns a structured response that includes a summary, key topics, relevance explanation, and most importantly, a ready-to-use LinkedIn post. The analyzer parses this response and returns an ArticleAnalysis object containing all this information.

The MarkdownGenerator class takes that ArticleAnalysis object and creates a permanent record of it. It generates a nicely formatted markdown file that includes all the article metadata, the AI's analysis, and the generated post. These files are stored in a generated-posts directory, giving you a searchable archive of everything you've processed.

Finally, the Telegram integration sends the generated post content to your phone. This is the point where you, as a human, get to review the AI's work and decide whether to share it. The bot sends you a message with the post content, and you can either copy it directly to LinkedIn or modify it first.

### The Flow of Data

Let me walk you through what happens when a new article is published on the .NET blog. The workflow starts when GitHub Actions triggers our application on its schedule – let's say every six hours. The application wakes up and starts fetching all seven RSS feeds. Each feed returns an XML document containing the most recent articles from that blog.

As we parse each feed, we extract individual articles and store them in a list. But here's a tricky part – the main DevBlogs feed often includes articles that also appear in the individual category feeds. So an article about ".NET 10" might show up in both the main feed and the .NET-specific feed. We handle this by tracking URLs in a HashSet, which automatically prevents duplicates.

Once we have our deduplicated list of articles, we filter it down to just the recent ones – typically articles published in the last day or so. We don't want to process old articles that we've already handled in previous runs. Then we check each recent article against our tracking file. If we've already processed and posted about an article, we skip it.

For each new article, we kick off the AI analysis pipeline. The analyzer fetches the full article HTML, cleans it up, and sends it to GPT-4 with our prompt. The AI reads the article and generates a comprehensive analysis along with a LinkedIn post. We save this analysis to a markdown file for our records.

With the analysis complete, we format a message and send it via Telegram. The message includes the generated post content with the URL and hashtags appended. On my phone, I receive a notification, review the post, and if I like it, I can copy it and share it on LinkedIn with just a few taps.

Finally, we update our tracking file to mark this article as processed, so we won't handle it again in future runs. If any files were created or modified, GitHub Actions commits these changes back to the repository, keeping everything in sync.



## Setting Up the Project From Scratch

### Creating the Solution Structure

Let's start building. Open your terminal and navigate to where you want to create the project. I like to keep my projects organized in a Development folder, but you can put it wherever makes sense for you.

First, we'll create a new solution file. In .NET, a solution is a container that can hold multiple projects. Even though we only have one project for now, starting with a solution makes it easier to add more projects later if needed. Run the command `dotnet new sln -n vs-feed-linkedin` to create a solution named vs-feed-linkedin.

Next, we need to create our console application project. We'll put this in a src subdirectory to keep things organized. Run `dotnet new console -n VsFeedLinkedin -o src` to create a console project named VsFeedLinkedin in the src folder. Then add this project to our solution with `dotnet sln add src/VsFeedLinkedin.csproj`.

Now navigate into the src directory with `cd src`. This is where we'll add our NuGet packages and do most of our development.

### Adding the Required Packages

With our project created, we need to add the three NuGet packages I mentioned earlier. Run each of these commands in sequence:

```bash
dotnet add package System.ServiceModel.Syndication --version 9.0.9
dotnet add package Microsoft.SemanticKernel --version 1.30.0
dotnet add package HtmlAgilityPack --version 1.11.72
```

After running these commands, your project file should look something like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="HtmlAgilityPack" Version="1.11.72" />
    <PackageReference Include="Microsoft.SemanticKernel" Version="1.30.0" />
    <PackageReference Include="System.ServiceModel.Syndication" Version="9.0.9" />
  </ItemGroup>

</Project>
```

The project file tells .NET that we're building an executable (OutputType is Exe), targeting .NET 9.0, and using modern C# features like implicit usings and nullable reference types. The ItemGroup section lists our three package dependencies with their exact versions.



## Deep Dive into RSS Feeds

### What Exactly is RSS?

Before we start writing code to fetch feeds, let's make sure we understand what we're working with. RSS stands for Really Simple Syndication, and it's a standardized XML format for distributing content updates. The idea is simple: instead of requiring users to visit your website to see if there's new content, you publish a machine-readable file that lists your recent content. Applications can then poll this file periodically to discover new articles.

RSS has been around since the late 1990s and early 2000s. You might think it's outdated technology, but it's actually still widely used – especially by blogs, news sites, and podcasts. The beauty of RSS is its simplicity. It's just XML with a defined structure, and any application can parse it.

### The Structure of a DevBlogs Feed

When you fetch an RSS feed from Microsoft DevBlogs, you get back an XML document that follows a specific structure. At the top level, there's an rss element that contains a single channel element. The channel represents the blog itself and includes metadata like the blog's title, URL, and description.

Inside the channel, you'll find multiple item elements, each representing an individual blog post. Each item includes a title (the article's headline), a link (the URL where you can read the full article), a pubDate (when the article was published), a dc:creator element (the author's name), one or more category elements (tags for the article), and a description (usually a summary or excerpt of the article).

Here's a simplified example of what this looks like:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<rss version="2.0">
  <channel>
    <title>.NET Blog</title>
    <link>https://devblogs.microsoft.com/dotnet</link>
    <description>The latest news about .NET</description>
    <item>
      <title>Announcing .NET 10</title>
      <link>https://devblogs.microsoft.com/dotnet/announcing-dotnet-10</link>
      <pubDate>Mon, 10 Dec 2025 12:00:00 GMT</pubDate>
      <dc:creator>Microsoft</dc:creator>
      <category>Announcements</category>
      <category>.NET</category>
      <description>Article summary...</description>
    </item>
  </channel>
</rss>
```

The great thing about .NET's System.ServiceModel.Syndication package is that it parses all of this for us. We don't have to manually navigate XML nodes or worry about different RSS versions. We just load the feed and get back strongly-typed objects.

### The Seven Feeds We Monitor

In my implementation, I monitor seven different Microsoft DevBlogs feeds. The main DevBlogs feed at devblogs.microsoft.com/feed gives us a broad view of everything Microsoft is publishing across all their developer blogs. The .NET-specific feed at devblogs.microsoft.com/dotnet/feed focuses specifically on .NET releases, features, and best practices. The Semantic Kernel feed at devblogs.microsoft.com/semantic-kernel/feed covers AI orchestration and integration – increasingly important as AI becomes central to modern development.

The Visual Studio feed at devblogs.microsoft.com/visualstudio/feed keeps me updated on IDE improvements and productivity features. The DevOps feed at devblogs.microsoft.com/devops/feed covers Azure DevOps, GitHub, and CI/CD topics. The All Things Azure feed at devblogs.microsoft.com/all-things-azure/feed focuses on cloud services and architecture patterns. Finally, the Azure SQL feed at devblogs.microsoft.com/azure-sql/feed covers database innovations and features.

You might wonder why I check both the main feed and the individual category feeds. The main feed gives me breadth – I'll see articles from any Microsoft developer blog, including ones I might not know about. The category feeds give me depth – they ensure I don't miss anything important in my core areas of interest, even if those articles get pushed out of the main feed by newer content.



## Building the RSS Fetching Logic

### The Core Fetching Function

Now let's write some code. The foundation of our application is the ability to fetch and parse RSS feeds. Here's the function that handles this:

```csharp
static async Task<SyndicationFeed?> FetchRssFeedAsync(string url)
{
    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("User-Agent", "VsFeedLinkedin/1.0");
    
    var response = await httpClient.GetStringAsync(url);
    
    using var stringReader = new StringReader(response);
    
    var settings = new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Parse,
        MaxCharactersFromEntities = 1024
    };
    
    using var xmlReader = XmlReader.Create(stringReader, settings);
    
    return SyndicationFeed.Load(xmlReader);
}
```

Let me walk through what this code does. We start by creating an HttpClient, which is .NET's built-in class for making HTTP requests. We set a User-Agent header because some servers block requests that don't identify themselves. It's good practice to set this even when servers don't require it.

We then make a GET request to the feed URL and receive the response as a string. This string contains the raw XML of the RSS feed.

To parse this XML, we create a StringReader to wrap our response string, then configure some XmlReaderSettings. The DtdProcessing setting is important – RSS feeds sometimes include DTD (Document Type Definition) declarations that need to be processed. The MaxCharactersFromEntities setting is a security measure that prevents XML bomb attacks by limiting how much entity expansion can occur.

Finally, we create an XmlReader with these settings and use SyndicationFeed.Load to parse the XML into a strongly-typed SyndicationFeed object. This gives us access to the feed's metadata and all its items through nice C# properties instead of raw XML navigation.

### Fetching Multiple Feeds with Error Handling

In the real world, network requests fail. Servers go down, connections time out, and XML can be malformed. We need to handle these cases gracefully. Here's how we fetch all our feeds while being resilient to failures:

```csharp
var allArticles = new List<(SyndicationItem item, string feedUrl)>();
var seenUrls = new HashSet<string>();

foreach (var feedUrl in feedUrls)
{
    try
    {
        Console.WriteLine($"  📡 Fetching {feedUrl}...");
        var feed = await FetchRssFeedAsync(feedUrl);
        
        if (feed?.Items != null && feed.Items.Any())
        {
            foreach (var item in feed.Items)
            {
                var itemUrl = item.Links.FirstOrDefault()?.Uri.ToString() ?? "";
                if (!string.IsNullOrEmpty(itemUrl) && seenUrls.Add(itemUrl))
                {
                    allArticles.Add((item, feedUrl));
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ⚠️  Failed to fetch {feedUrl}: {ex.Message}");
    }
}
```

We maintain two collections here. The allArticles list will hold all the articles we find, along with which feed they came from. The seenUrls HashSet tracks which article URLs we've already seen, helping us avoid duplicates.

We loop through each feed URL and wrap the fetch operation in a try-catch block. If fetching a particular feed fails – maybe the server is temporarily down – we log a warning and continue with the next feed. This way, a problem with one feed doesn't prevent us from processing the others.

For each successfully fetched feed, we iterate through its items. We extract the article URL from the item's Links collection. The HashSet.Add method returns false if the URL is already in the set, which is perfect for our deduplication logic. We only add the article to our list if it's new.

We store the feed URL alongside each article because this information might be useful later – for example, we might want to know which specific feed an article came from for debugging or logging purposes.



## Handling Duplicates and Tracking State

### The Deduplication Challenge

As I mentioned earlier, Microsoft DevBlogs has a hierarchical feed structure that creates an interesting challenge. When a .NET team member publishes an article about, say, performance improvements in .NET 10, that article will likely appear in both the main DevBlogs feed and the .NET-specific feed. Sometimes it might even appear in the Visual Studio feed if it relates to IDE features.

If we naively processed every article from every feed, we'd end up analyzing and posting about the same article multiple times. That would waste API calls to Azure OpenAI, spam our Telegram with duplicate notifications, and potentially annoy our followers if we posted duplicates.

The solution is URL-based deduplication. Each article has a unique URL, so we can use that as an identifier. The HashSet data structure is perfect for this because it provides O(1) lookup time and automatically prevents duplicates. When we try to add a URL that's already in the set, the Add method simply returns false, letting us know we should skip that article.

### Persistent State with Markdown

Deduplication handles duplicates within a single run, but what about across runs? When our application runs every six hours, we need to remember which articles we've already processed so we don't handle them again.

I chose to store this state in a markdown file called posted-articles.md. Why markdown? A few reasons. First, it's human-readable. I can open the file and immediately see which articles I've shared. Second, it's version-controlled. Since this file lives in our Git repository, I have a complete history of when articles were processed. Third, it serves as documentation. Anyone looking at the repository can see what the application has done.

The format of this file is simple. It has a header, a timestamp showing when the application last ran, and then a list of articles in markdown link format:

```markdown
# Posted Articles

*Last run: 2025-12-10 15:30:00*

List of articles posted to LinkedIn:

- [Announcing .NET 10](https://devblogs.microsoft.com/dotnet/announcing-dotnet-10?wt.mc_id=DT-MVP-5004972) - Posted on 2025-12-10 15:30:00 (Published: 2025-12-10)
- [Visual Studio 2026 Preview](https://devblogs.microsoft.com/visualstudio/vs-2026-preview?wt.mc_id=DT-MVP-5004972) - Posted on 2025-12-09 10:15:00 (Published: 2025-12-09)
```

### Loading and Parsing the Tracking File

To check if we've already processed an article, we need to load this file and extract the URLs. Here's the function that does this:

```csharp
static HashSet<string> LoadPostedArticles(string filePath)
{
    var postedUrls = new HashSet<string>();
    
    if (!File.Exists(filePath))
    {
        return postedUrls;
    }

    var lines = File.ReadAllLines(filePath);
    foreach (var line in lines)
    {
        var match = System.Text.RegularExpressions.Regex.Match(line, @"\(([^)]+)\)");
        if (match.Success)
        {
            var url = match.Groups[1].Value;
            
            if (url.Contains("?wt.mc_id="))
            {
                url = url.Substring(0, url.IndexOf("?wt.mc_id="));
            }
            else if (url.Contains("?"))
            {
                url = url.Substring(0, url.IndexOf("?"));
            }
            
            url = url.TrimEnd('/');
            postedUrls.Add(url);
        }
    }

    return postedUrls;
}
```

This function returns a HashSet containing all the URLs we've already processed. We start by checking if the file exists – on first run, it won't, so we return an empty set.

For each line in the file, we use a regex to extract the URL from the markdown link format. The regex `\(([^)]+)\)` matches anything inside parentheses, which is where markdown links store their URLs.

Then comes an important step: URL normalization. URLs for the same article can vary in format. The RSS feed might give us `https://devblogs.microsoft.com/dotnet/article`, but our saved version has a tracking parameter appended: `https://devblogs.microsoft.com/dotnet/article?wt.mc_id=DT-MVP-5004972`. Some URLs have trailing slashes, others don't.

To handle this, we strip off any query parameters (everything after the `?`) and remove trailing slashes. This normalization ensures that we recognize articles as duplicates even if their URLs differ in these superficial ways.

### Saving New Articles

When we successfully process an article, we need to add it to our tracking file:

```csharp
static void SavePostedArticle(string filePath, string url, string title, DateTimeOffset publishDate)
{
    var markdownEntry = $"- [{title}]({url}) - Posted on {DateTime.Now:yyyy-MM-dd HH:mm:ss} (Published: {publishDate:yyyy-MM-dd})\n";
    
    if (!File.Exists(filePath))
    {
        File.WriteAllText(filePath, "# Posted Articles\n\n*Last run: {DateTime.Now:yyyy-MM-dd HH:mm:ss}*\n\nList of articles posted:\n\n");
    }
    
    File.AppendAllText(filePath, markdownEntry);
}
```

This function creates a markdown-formatted entry with the article title as a link, followed by timestamps showing when we posted it and when it was originally published. If the file doesn't exist yet, we create it with a header first.



## The AI Analysis Engine

### Understanding Semantic Kernel

Now we get to the most exciting part of our application – the AI analysis. Semantic Kernel is Microsoft's open-source SDK for integrating large language models into applications. It's more than just a wrapper around API calls. It provides a framework for building sophisticated AI applications with features like plugins, planners, and memory.

For our use case, we're using Semantic Kernel's chat completion capabilities. We'll send a prompt to Azure OpenAI, and the model will analyze our article and generate a response. Semantic Kernel handles all the complexity of API authentication, request formatting, and response parsing.

### Setting Up the Article Analyzer

Let's look at how we set up our analyzer class:

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using HtmlAgilityPack;

namespace VsFeedLinkedin.Services;

public class ArticleAnalyzer
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;

    public ArticleAnalyzer(string endpoint, string apiKey, string deploymentName)
    {
        var builder = Kernel.CreateBuilder();
        
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: deploymentName,
            endpoint: endpoint,
            apiKey: apiKey
        );
        
        _kernel = builder.Build();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }
```

Semantic Kernel uses a builder pattern for configuration. We create a KernelBuilder, add our Azure OpenAI chat completion service with the necessary credentials, then build the kernel. From the built kernel, we retrieve the IChatCompletionService interface, which we'll use to send prompts and receive responses.

The constructor takes three parameters: the Azure OpenAI endpoint (something like `https://your-resource.openai.azure.com/`), your API key, and the deployment name (like `gpt-4o`). These are passed in from environment variables, keeping our credentials secure.

### Crafting the Perfect Prompt

The prompt we send to the AI is crucial. A well-crafted prompt produces consistent, high-quality outputs. A vague or poorly structured prompt produces inconsistent, mediocre results. I spent considerable time iterating on this prompt to get outputs that I'm happy with:

```csharp
var prompt = $"""
    You are a professional tech content analyst and LinkedIn content creator. 
    Analyze the following Microsoft DevBlogs article and create an engaging LinkedIn post.

    Article Title: {title}
    Author: {author}
    URL: {url}
    Tags: {string.Join(", ", tags)}
    
    Article Content:
    {cleanContent}

    Please provide:
    1. A brief summary (2-3 sentences) of the key points
    2. The main technologies or topics covered
    3. Why this is relevant for developers/tech professionals
    4. An engaging LinkedIn post (max 1300 characters) that:
       - Starts with a hook or attention-grabbing statement
       - Highlights the key value for readers
       - Includes a call to action
       - Uses appropriate emojis (but not too many)
       - Maintains a professional yet approachable tone
       - DO NOT include hashtags in the post (they will be added separately)
       - DO NOT include the URL in the post (it will be added separately)

    Format your response as follows:
    ## Summary
    [Your summary here]

    ## Key Topics
    [List of main topics/technologies]

    ## Relevance
    [Why this matters]

    ## LinkedIn Post
    [Your engaging LinkedIn post here]
    """;
```

Let me explain the design decisions here. We start by giving the AI a clear role: "You are a professional tech content analyst and LinkedIn content creator." This primes the model to respond in the appropriate style and voice.

We provide all the context the AI needs: the article title, author, URL, tags from the RSS feed, and the full article content. The more context we give, the better the analysis will be.

Then we specify exactly what we want back. I ask for four things: a summary, key topics, relevance explanation, and a LinkedIn post. For the LinkedIn post specifically, I give detailed instructions about what makes a good post – it should have a hook, highlight value, include a call to action, use emojis appropriately, and maintain a professional tone.

The negative instructions are equally important. I explicitly tell the AI NOT to include hashtags or the URL in the post. Why? Because I add these separately, and if the AI included them, I'd have duplicates. This kind of explicit instruction prevents common mistakes.

Finally, I specify the exact output format. By asking for sections marked with ## headers, I make the response easy to parse programmatically. The AI is very good at following formatting instructions, and this consistency makes our parsing code simpler and more reliable.

### Executing the Analysis

Here's how we put it all together:

```csharp
public async Task<ArticleAnalysis> AnalyzeArticleAsync(
    string title, 
    string url, 
    string htmlContent, 
    string author, 
    List<string> tags)
{
    var cleanContent = ExtractTextFromHtml(htmlContent);
    
    if (cleanContent.Length > 8000)
    {
        cleanContent = cleanContent.Substring(0, 8000) + "...";
    }

    var chatHistory = new ChatHistory();
    chatHistory.AddUserMessage(prompt);

    var response = await _chatService.GetChatMessageContentAsync(chatHistory);
    var responseText = response.Content ?? "";

    return ParseAnalysisResponse(responseText, title, url, author, tags);
}
```

We first extract clean text from the HTML content (I'll explain this in the next section). Then we truncate the content if it's too long. Large language models have token limits, and very long articles might exceed them. By capping at 8000 characters, we ensure we stay within limits while still providing substantial context.

We create a ChatHistory object and add our prompt as a user message. This is Semantic Kernel's abstraction for chat-based interactions. We send this to the chat completion service and get back a response. Finally, we parse the response to extract the individual sections.

### Parsing the AI Response

The AI returns its response as text formatted with our requested structure. We need to parse this into individual fields:

```csharp
private static ArticleAnalysis ParseAnalysisResponse(
    string response, 
    string title, 
    string url, 
    string author, 
    List<string> tags)
{
    var analysis = new ArticleAnalysis
    {
        Title = title,
        Url = url,
        Author = author,
        Tags = tags,
        RawAnalysis = response
    };

    var sections = response.Split("##", StringSplitOptions.RemoveEmptyEntries);
    
    foreach (var section in sections)
    {
        var lines = section.Trim().Split('\n', 2);
        if (lines.Length < 2) continue;
        
        var sectionTitle = lines[0].Trim().ToLower();
        var sectionContent = lines[1].Trim();

        switch (sectionTitle)
        {
            case "summary":
                analysis.Summary = sectionContent;
                break;
            case "key topics":
                analysis.KeyTopics = sectionContent;
                break;
            case "relevance":
                analysis.Relevance = sectionContent;
                break;
            case "linkedin post":
                analysis.LinkedInPost = sectionContent;
                break;
        }
    }

    return analysis;
}
```

We split the response by the `##` markers, which gives us each section. For each section, we split by newline to separate the header from the content. We then use a switch statement to assign each section's content to the appropriate property.

We also store the raw, unparsed response. This is useful for debugging – if something goes wrong with parsing, we can look at what the AI actually returned.



## Extracting Content from HTML

### Why We Need to Clean HTML

When we fetch an article from a blog, we get the full HTML of the page. This includes much more than just the article content – there's navigation menus, headers, footers, sidebars, related article widgets, comment sections, scripts for analytics and tracking, stylesheets, and all sorts of other elements.

If we sent all of this to our AI, several bad things would happen. The AI would have to process a lot of irrelevant text, wasting tokens and potentially confusing the analysis. The navigation and footer text might get included in the summary. Scripts and CSS would be treated as content, further polluting the analysis.

We need to extract just the article content – the part that a human reader would actually read.

### Using HtmlAgilityPack

HtmlAgilityPack is a robust HTML parsing library for .NET. Unlike XML, HTML is often malformed – tags might not be properly closed, attributes might not be quoted correctly. HtmlAgilityPack handles all of this gracefully, giving us a DOM-like structure we can query and manipulate.

Here's our extraction function:

```csharp
private static string ExtractTextFromHtml(string html)
{
    if (string.IsNullOrWhiteSpace(html))
        return string.Empty;

    var doc = new HtmlDocument();
    doc.LoadHtml(html);

    var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//nav|//footer|//header");
    if (nodesToRemove != null)
    {
        foreach (var node in nodesToRemove)
        {
            node.Remove();
        }
    }

    var text = doc.DocumentNode.InnerText;
    
    text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
    return text.Trim();
}
```

We load the HTML into an HtmlDocument, which parses it into a tree structure. Then we use XPath to select all the nodes we want to remove. The XPath expression `//script|//style|//nav|//footer|//header` selects all script elements (JavaScript code we don't need), style elements (CSS we don't need), nav elements (navigation menus), footer elements, and header elements.

After removing these nodes, we get the InnerText property, which extracts all the text content while stripping HTML tags. This gives us the plain text of the article.

Finally, we clean up whitespace. HTML often has lots of extra whitespace for formatting purposes – multiple spaces, tabs, newlines. We use a regex to replace any sequence of whitespace characters with a single space, then trim the result.

### Fetching the Full Article

The RSS feed only gives us summaries, not full article content. To get the complete text, we need to fetch the article's web page:

```csharp
public static async Task<string> FetchArticleContentAsync(string url)
{
    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("User-Agent", "VsFeedLinkedin/1.0");
    
    try
    {
        return await httpClient.GetStringAsync(url);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Failed to fetch article content: {ex.Message}");
        return string.Empty;
    }
}
```

This is straightforward – we make an HTTP GET request to the article URL and return the HTML response. We wrap it in a try-catch because network requests can fail, and we'd rather return an empty string than crash the whole application.



## Creating Permanent Documentation

### Why Generate Markdown Files

Every time we analyze an article, we generate a detailed markdown file documenting that analysis. This serves several purposes.

First, it creates a searchable archive. Over time, you'll build up a collection of analyzed articles. You can search through these files to find past content on specific topics.

Second, it provides transparency. You can see exactly what the AI generated for each article, including the full analysis and the LinkedIn post.

Third, it's useful for debugging. If something goes wrong with a post, you can look at the markdown file to understand what happened.

### The Markdown Generator Class

```csharp
public class MarkdownGenerator
{
    private readonly string _outputDirectory;

    public MarkdownGenerator(string outputDirectory)
    {
        _outputDirectory = outputDirectory;
        
        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    public string GenerateMarkdownFile(ArticleAnalysis analysis)
    {
        var sb = new StringBuilder();
        
        var safeTitle = GenerateSafeFileName(analysis.Title);
        var fileName = $"{analysis.AnalyzedAt:yyyy-MM-dd}_{safeTitle}.md";
        var filePath = Path.Combine(_outputDirectory, fileName);

        sb.AppendLine($"# {analysis.Title}");
        sb.AppendLine();
        sb.AppendLine("## Article Information");
        sb.AppendLine();
        sb.AppendLine($"- **Author:** {analysis.Author}");
        sb.AppendLine($"- **URL:** [{analysis.Url}]({analysis.Url})");
        sb.AppendLine($"- **Published:** {analysis.PublishDate:yyyy-MM-dd}");
        sb.AppendLine($"- **Analyzed:** {analysis.AnalyzedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"- **Tags:** {string.Join(", ", analysis.Tags)}");
        sb.AppendLine();
        sb.AppendLine("");
        sb.AppendLine();
        sb.AppendLine("## AI Analysis");
        sb.AppendLine();
        sb.AppendLine("### Summary");
        sb.AppendLine();
        sb.AppendLine(analysis.Summary);
        sb.AppendLine();
        sb.AppendLine("### Key Topics");
        sb.AppendLine();
        sb.AppendLine(analysis.KeyTopics);
        sb.AppendLine();
        sb.AppendLine("### Relevance for Developers");
        sb.AppendLine();
        sb.AppendLine(analysis.Relevance);
        sb.AppendLine();
        sb.AppendLine("");
        sb.AppendLine();
        sb.AppendLine("## Generated LinkedIn Post");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine(analysis.LinkedInPost);
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("");
        sb.AppendLine();
        sb.AppendLine("*This analysis was generated using AI (Semantic Kernel with Azure OpenAI)*");

        File.WriteAllText(filePath, sb.ToString());
        return filePath;
    }
```

The constructor takes an output directory path and creates it if it doesn't exist. The GenerateMarkdownFile method takes an ArticleAnalysis object and produces a nicely formatted markdown document.

The filename includes the date and a sanitized version of the title. This makes files easy to sort chronologically and identify at a glance.

### Handling Unsafe Filenames

Article titles can contain characters that aren't allowed in filenames – things like colons, slashes, question marks, and quotes. We need to sanitize these:

```csharp
private static string GenerateSafeFileName(string title)
{
    var invalidChars = Path.GetInvalidFileNameChars();
    var safeTitle = new string(title
        .Where(c => !invalidChars.Contains(c))
        .ToArray());
    
    safeTitle = safeTitle.Replace(" ", "-").Replace("--", "-");
    
    if (safeTitle.Length > 50)
    {
        safeTitle = safeTitle.Substring(0, 50);
    }
    
    return safeTitle.TrimEnd('-').ToLowerInvariant();
}
```

We use Path.GetInvalidFileNameChars() to get a list of characters that can't appear in filenames on the current operating system. We filter these out, replace spaces with hyphens for readability, limit the length to 50 characters, and convert to lowercase for consistency.



## Setting Up Telegram Notifications

### Why I Chose Telegram

For the notification component, I considered several options – email, SMS, Slack, Discord, and Telegram. I ultimately chose Telegram for several reasons.

The API is completely free with no rate limits for reasonable usage. Many notification services have limits on how many messages you can send for free, but Telegram doesn't restrict bot messages to individual users.

The bot API is incredibly simple. It's just HTTP requests with JSON payloads. No complex authentication flows, no webhooks required for basic functionality.

Telegram works everywhere – on my phone, on my desktop, in my web browser. I can receive notifications wherever I am and respond immediately.

Messages support rich formatting. I can use bold text, italics, and even code blocks to make my notifications more readable.

### Creating Your Telegram Bot

Setting up a Telegram bot is surprisingly easy. Open Telegram and search for @BotFather – this is Telegram's official bot for creating and managing bots. Start a conversation with BotFather and send the command /newbot. BotFather will ask you for a name for your bot (this is the display name) and a username (this must be unique and end in "bot"). Once you've provided these, BotFather will create your bot and give you an API token. This token is like a password – keep it secret and don't commit it to public repositories.

To find your chat ID so the bot knows where to send messages, start a conversation with your new bot by searching for it and pressing Start. Then access the URL `https://api.telegram.org/bot<YOUR_TOKEN>/getUpdates` in your browser or using curl. Look for the `chat` object in the response – the `id` field is your chat ID.

### Sending Messages via the API

Here's our function for sending Telegram messages:

```csharp
static async Task SendToTelegramAsync(string botToken, string chatId, string message)
{
    using var httpClient = new HttpClient();
    
    var telegramApiUrl = $"https://api.telegram.org/bot{botToken}/sendMessage";
    
    var payload = new
    {
        chat_id = chatId,
        text = message,
        parse_mode = "HTML"
    };
    
    var jsonContent = JsonSerializer.Serialize(payload);
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    
    var response = await httpClient.PostAsync(telegramApiUrl, content);
    
    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        throw new Exception($"Telegram API error: {response.StatusCode} - {errorContent}");
    }
}
```

The Telegram Bot API is REST-based. We make a POST request to the sendMessage endpoint with a JSON body containing the chat ID (where to send), the message text (what to send), and optionally a parse mode (for formatting).

Setting parse_mode to "HTML" lets us use basic HTML tags in our messages – things like `<b>bold</b>` and `<i>italic</i>`. This can make notifications more readable, though for our current use case we send plain text.

If the request fails, we throw an exception with details about what went wrong. This helps with debugging if something isn't working.



## Configuring the Application

### Environment Variables

Our application needs several pieces of sensitive information – API keys, bot tokens, and endpoint URLs. We should never hard-code these or commit them to version control. Instead, we use environment variables, which can be set securely in each environment where the application runs.

For Telegram, we need TELEGRAM_BOT_TOKEN (the token BotFather gave you) and TELEGRAM_CHAT_ID (your chat ID where messages should be sent).

For Azure OpenAI, we need AZURE_OPENAI_ENDPOINT (your resource's URL), AZURE_OPENAI_API_KEY (your API key), and AZURE_OPENAI_DEPLOYMENT (the name of your deployed model, like "gpt-4o").

### Loading Configuration in Code

Here's how we load these values at application startup:

```csharp
var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
var telegramChatId = Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID");

var azureOpenAiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
var azureOpenAiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
var azureOpenAiDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o";

var aiAnalysisEnabled = !string.IsNullOrWhiteSpace(azureOpenAiEndpoint) && 
                        !string.IsNullOrWhiteSpace(azureOpenAiKey);
```

We use Environment.GetEnvironmentVariable to read each value. For the deployment name, we provide a default of "gpt-4o" if no value is set.

We then check whether AI analysis should be enabled by verifying that we have both an endpoint and an API key. This allows the application to run in a degraded mode if Azure OpenAI isn't configured – it'll still fetch feeds and track articles, just without the AI analysis.

### Graceful Degradation

This concept of graceful degradation is important. We don't want the application to crash just because one optional feature isn't configured:

```csharp
ArticleAnalyzer? articleAnalyzer = null;
MarkdownGenerator? markdownGenerator = null;

if (aiAnalysisEnabled)
{
    Console.WriteLine("🤖 AI Analysis enabled - Using Azure OpenAI with Semantic Kernel");
    articleAnalyzer = new ArticleAnalyzer(azureOpenAiEndpoint!, azureOpenAiKey!, azureOpenAiDeployment);
    markdownGenerator = new MarkdownGenerator(articlesOutputDir);
}
else
{
    Console.WriteLine("ℹ️  AI Analysis disabled - Set AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_API_KEY to enable");
}
```

If AI is enabled, we create the analyzer and markdown generator. If not, we leave them null and skip the AI-related steps during processing. The application still provides value by fetching feeds and sending basic notifications, even without the AI enhancement.



## Automating with GitHub Actions

### Why GitHub Actions

The real power of this solution comes from automation. We don't want to manually run the application every few hours – we want it to run automatically in the background.

GitHub Actions is perfect for this. It's built into GitHub, so there's no additional service to set up. It's free for public repositories and includes generous free minutes for private repositories. It can run on a schedule, triggering our application at regular intervals. It has built-in secrets management for storing our API keys securely. And it can commit changes back to the repository, keeping our tracking file up to date.

### The Workflow File

Create a file at .github/workflows/fetch-and-notify.yml with the following content:

```yaml
name: Fetch DevBlogs and Notify

on:
  schedule:
    - cron: '0 */6 * * *'
  workflow_dispatch:

jobs:
  fetch-and-notify:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore src/VsFeedLinkedin.csproj
      
      - name: Build
        run: dotnet build src/VsFeedLinkedin.csproj --no-restore
      
      - name: Run application
        env:
          TELEGRAM_BOT_TOKEN: ${{ secrets.TELEGRAM_BOT_TOKEN }}
          TELEGRAM_CHAT_ID: ${{ secrets.TELEGRAM_CHAT_ID }}
          AZURE_OPENAI_ENDPOINT: ${{ secrets.AZURE_OPENAI_ENDPOINT }}
          AZURE_OPENAI_API_KEY: ${{ secrets.AZURE_OPENAI_API_KEY }}
          AZURE_OPENAI_DEPLOYMENT: ${{ secrets.AZURE_OPENAI_DEPLOYMENT }}
        run: dotnet run --project src/VsFeedLinkedin.csproj
      
      - name: Commit and push changes
        run: |
          git config user.name "GitHub Actions Bot"
          git config user.email "actions@github.com"
          
          if [[ -n $(git status --porcelain posted-articles.md generated-posts/) ]]; then
            TIMESTAMP=$(date +%Y%m%d_%H%M%S)
            git add posted-articles.md generated-posts/
            git commit -m "chore($TIMESTAMP): processed new articles"
            git push
          else
            echo "No changes to commit"
          fi
```

Let me explain each part. The on section defines when the workflow runs. The schedule trigger uses cron syntax – `0 */6 * * *` means "at minute 0 of every 6th hour." So the workflow runs at midnight, 6 AM, noon, and 6 PM UTC. The workflow_dispatch trigger allows manual runs from the GitHub UI, which is useful for testing.

The job runs on ubuntu-latest, which is a Linux virtual machine. We check out our repository, set up .NET 9, restore NuGet packages, and build the project.

The Run application step is where the magic happens. We pass our secrets as environment variables using the ${{ secrets.SECRET_NAME }} syntax. These secrets are stored securely in GitHub and never exposed in logs.

Finally, we commit any changes back to the repository. We configure Git with a bot identity, check if there are any changes to our tracking file or generated posts directory, and if so, create a commit and push it.

### Setting Up Secrets

To add secrets to your GitHub repository, go to your repository's Settings, then Secrets and variables, then Actions. Click "New repository secret" and add each of your environment variables. The names must match exactly what we reference in the workflow file.



## Wrapping Up

### What We've Built

Looking back at everything we've covered, we've built a comprehensive, AI-powered RSS feed aggregator that automates what used to be a tedious manual process. The application monitors seven Microsoft DevBlogs feeds automatically, catching every new article as soon as it's published. It handles the complexities of deduplication, recognizing when the same article appears in multiple feeds.

The AI analysis powered by Semantic Kernel and Azure OpenAI reads and understands article content, generating summaries, identifying key topics, and explaining relevance – all automatically. Most importantly, it creates engaging LinkedIn posts that I can share with minimal editing.

The Telegram integration means I get notified on my phone whenever there's new content to review. I can glance at the message, decide if I want to share it, and act immediately.

And because it runs on GitHub Actions on a schedule, I don't have to remember to do anything. The system works in the background, and I only engage when there's something worth sharing.

### The Technologies That Made It Possible

This project brought together several technologies that each played a crucial role. .NET 9 provided a solid foundation with its modern language features and excellent performance. Semantic Kernel made AI integration straightforward, handling all the complexity of API calls and response management. Azure OpenAI provided the intelligence – the ability to actually understand and analyze technical content. HtmlAgilityPack solved the messy problem of extracting clean text from web pages. System.ServiceModel.Syndication made RSS parsing a breeze. The Telegram Bot API gave us free, reliable notifications. And GitHub Actions tied it all together with automated, scheduled execution.

### Thinking About Costs

One question you might have is: how much does this cost to run? The answer is: not much at all.

Telegram is completely free – no charges for sending messages through your bot.

GitHub Actions is free for public repositories. For private repositories, you get 2,000 minutes per month on the free tier, which is more than enough for our use case.

Azure OpenAI is the only paid component, and the costs are minimal. Using GPT-4o, analyzing a typical blog article costs somewhere between one and three cents. Even if you're processing dozens of articles per month, you're looking at less than a dollar in AI costs.

### Where You Could Take This Next

While this solution works great for my needs, there are plenty of ways you could extend it. You could add support for multiple social platforms – maybe post to Twitter/X, Mastodon, or Bluesky in addition to LinkedIn. You could implement sentiment analysis to track the tone of articles over time and spot trends. You could allow different prompt templates for different feeds, generating different styles of posts for different topics. You could build a web dashboard for reviewing and managing posts instead of using Telegram. You could track engagement metrics for posted content to see which topics resonate most with your audience.

### Final Thoughts

What I love most about this project is that it embodies a philosophy I believe in strongly: automation should handle the tedious parts while leaving the creative and decision-making parts to humans. The system does all the grunt work – fetching, parsing, analyzing, generating – but I still review everything before sharing. The AI-generated posts are starting points that I can customize and personalize.

By combining the power of .NET, Semantic Kernel, and Azure OpenAI, we've created a tool that saves hours of manual work each week while maintaining quality and consistency. It's the kind of practical automation that makes a real difference in daily life.

If you build something similar or have ideas for improvements, I'd love to hear about it. Feel free to reach out on LinkedIn!

Happy coding, and merry Christmas! 🎄
