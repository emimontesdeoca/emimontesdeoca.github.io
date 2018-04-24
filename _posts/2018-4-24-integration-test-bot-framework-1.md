---
layout: post
title: "Integration test with Bot Framework (1)"
---

Since I started working at Intelequia Software Solutions for a short period of time, I've been asigned to work on integration test for the Bot Framework.

Intelequia is working on bot solution for business, so my work is to make the bot to be as stable as posible, but first, what is Bot Framework?

> Build, connect, deploy, and manage intelligent bots to naturally interact with your users on a website, app, Cortana, Microsoft Teams, Skype, Slack, Facebook Messenger, and more. Get started quick with a complete bot building environment, all while only paying for what you use.

You can find more information about Bot Framework right [here](https://dev.botframework.com/).

**The following explanation won't cover all the basic information of how the Bot Framework works, if you dont undestand, please go and check the official documentation.**

## Why do I need integration test?

Integration test are needed because everytime one of my coworkers pushes a fix, a new feature, or even a new bug, this tests will run before pushing the code to production and if any of the test fails, the code won't go to production, meaning that the end user won't have the bug.

## Integration test in bots?

I do belive that in bots, the integration test are quite important. You can't have a bot where some of their menus aren't working, or some of the feautres are returning nothing.

Companies are using bots for their customers because they don't want people to be busy with their problems, if a bot can help an user, the other workers will be able to use their time to do something more important.

## Overview of the solution.

In order to make this work, I used a Test project in Visual Studio, which will use WebClient for the API Rest and a Json file, where we will store our cases.

## JSON file

``` json
{
  "secret": "direct-line-secret",
  "directlineGenerateTokenEndpoint": "https://directline.botframework.com/v3/directline/tokens/generate",
  "directlineConversationEndpoint": "https://directline.botframework.com/v3/directline/conversations/",
  "entries": [
    {
      "name": "DecirHola",
      "request": {
        "type": "message",
        "text": "Hola",
        "from": {
          "id": "default-user",
          "name": "User"
        },
        "locale": "es",
        "textFormat": "plain",
        "timestamp": "2018-04-09T08:04:37.195Z",
        "channelData": {
          "clientActivityId": "1523261059363.6264723268323733.0"
        },
        "entities": [
          {
            "type": "ClientCapabilities",
            "requiresBotState": true,
            "supportsTts": true,
            "supportsListening": true
          }
        ],
        "id": "61hacck8j6jg"
      },
      "response": {
        "type": "message",
        "timestamp": "2018-04-09T08:04:37.901Z",
        "localTimestamp": "2018-04-09T09:04:37+01:00",
        "serviceUrl": "http://localhost:50629",
        "channelId": "emulator",
        "from": {
          "id": "j98bbdf097a",
          "name": "Bot"
        },
        "conversation": {
          "id": "eabcie4be8ak"
        },
        "recipient": {
          "id": "default-user"
        },
        "locale": "es",
        "text": "No tengo respuesta para eso.",
        "attachments": [],
        "entities": [],
        "replyToId": "61hacck8j6jg",
        "id": "47me557ikbf7"
      },
      "assert": "Request.Text == Response.Text"
    }
  ]
}
```

As you can see we have:

- Directline secret -> secret from the published bot
- Directline token generation endpoint -> endpoint to get the token using the secret
- Directline conversation endpoint -> endpoint in order to play with the conversation
- Entry -> the test case
  - Request -> what we send to the conversation
  - Response -> what we expect to get
  - Assert -> what are we comparing

## Deserialization

We have the json file perfectly formatted, now we have to load it into the solution, so we will be using JSON.NET and some classes. First we have the entry collection, which has everything, and then we have, for each collection a list of entries.

``` csharp
    /// <summary>
    /// Object to parse from the file
    /// </summary>
    public class TestEntriesCollection
    {
        /// <summary>
        /// DirectLine Secret
        /// </summary>
        [JsonProperty("secret")]
        public string Secret { get; set; }
        /// <summary>
        /// Endpoint to get the token using the secret for DirectLine
        /// </summary>
        [JsonProperty("directlineGenerateTokenEndpoint")]
        public string DirectLineGenerateTokenEndpoint { get; set; }
        /// <summary>
        /// Endpoint for a conversation in DirectLine
        /// </summary>
        [JsonProperty("directlineConversationEndpoint")]
        public string DirectLineConversationEndpoint { get; set; }
        /// <summary>
        /// Entries list
        /// </summary>
        [JsonProperty("entries")]
        public List<TestEntry> Entries { get; set; }
    }

```



``` csharp

    public class TestEntry
    {
        /// <summary>
        /// Entry name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// Activity requested by the entry
        /// </summary>
        [JsonProperty("request")]
        public Activity Request { get; set; }
        /// <summary>
        /// Activity response expected by the entry
        /// </summary>
        [JsonProperty("response")]
        public Activity Response { get; set; }
        /// <summary>
        /// Assert value in string
        /// </summary>
        [JsonProperty("assert")]
        public string Assert { get; set; }
    }

```

## Parsing from json to object in the test case

Having the classes for the parsing object, it's quite easy as we need to read as the object.

``` csharp
// Load entries from file
var path = System.IO.File.ReadAllText(@"C:\data.json");

// Deserialize to object
var data = JsonConvert.DeserializeObject<TestEntriesCollection>(path);

```

Now with this collection, we will be able to loop through it and get the information using, for example, a foreach.

``` csharp
 foreach (TestEntry entry in data.Entries)
    {
      ....
    }
```

And that's all for this part, the next part will include the authorization for DirectLine, remember that all the code is stored in my github in [this](https://github.com/emimontesdeoca/integration-test-directline-bot-framework) repository.
