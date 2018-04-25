---
layout: post
title: "Integration test using Microsoft's Bot Framework and DirectLine for flow cases"
categories: development
tags:
  - botframework
---

## Introduction

In the previous blog posts, we did some integration test for single cases. "Single cases" are those cases that after asking the bot something, he will reply only once and then we compare the results.

**This guide is won't explain how are the authentication and API calls done, if you want to check it out, please check out the single cases guide.**

### What about flow conversations?

Using the current way we dont have any kind of flow in the conversation, for example, if you want to ask for help and then a menu shows with different options, and then after selecting one of them, another menu. This would make like 2 or more `responses`. **So this means that using the integration test solution that we used before, won't work.**

Here is a diagram of how the integration test for single cases work.

[![https://gyazo.com/8915f2653033c1143947ef59196403f4](https://i.gyazo.com/8915f2653033c1143947ef59196403f4.png)](https://gyazo.com/8915f2653033c1143947ef59196403f4)

And here's a diagram of how are we going to adapt the current integration test solution to flow cases.

[![https://gyazo.com/ef77fa168d3b5f8b4a116ff38b5edd83](https://i.gyazo.com/ef77fa168d3b5f8b4a116ff38b5edd83.png)](https://gyazo.com/ef77fa168d3b5f8b4a116ff38b5edd83)

**The following explanation won't cover all the basic information of how the Bot Framework works, if you dont undestand, please go and check the official documentation.**

## Example case

For the following guide, I'll be using a bot with a flow conversation created by me, which is asking for help and then selecting different options.

[![https://gyazo.com/0a1104cce67c07331a6e0fbd8e19b3e2](https://i.gyazo.com/0a1104cce67c07331a6e0fbd8e19b3e2.gif)](https://gyazo.com/0a1104cce67c07331a6e0fbd8e19b3e2)

## New JSON structure

Now that we have more than one `request` when talking to the bot, we need to modify our json structure to add all the `request` that we will be doing.

```json
{
  "secret": "direct-line-secret",
  "directlineGenerateTokenEndpoint":
    "https://directline.botframework.com/v3/directline/tokens/generate",
  "directlineConversationEndpoint":
    "https://directline.botframework.com/v3/directline/conversations/",
  "entries": [
    {
      "name": "PedirAyuda",
      "requests": [
        {
          "type": "message",
          "text": "Ayuda",
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
        {
          "type": "message",
          "text": "Telefono",
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
        {
          "type": "message",
          "text": "Oficina",
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
        {
          "type": "message",
          "text": "Tenerife",
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
        }
      ],
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
        "text": "922920252",
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

As you can see, there's a important change, `request` is now `requests`. That meaning that now we have a `List<Activity>` instead of a single `activity`.

## New objects

Previously we had our objects set for single cases: `TestEntry` and `TestEntryCollection`. For flow cases we will be creating new objects: `TestEntryFlow` and `TestEntryFlowCollection`.

### `TestEntryFlow`

This object is for every entry that we have in the collection, look that the `Requests` object is now a `List<Activity>` instead of a single `Activity` as I mentioned before.

Since we will be asking the bot multiple times, we need to have multiple `activities` which will be sended to the conversation.

```csharp
public class TestEntryFlow
{
    /// <summary>
    /// Entry name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
    /// <summary>
    /// Activity requested by the entry
    /// </summary>
    [JsonProperty("requests")]
    public List<Activity> Requests { get; set; }
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

### `TestEntiresCollection`

This object will contain the relevant information for `DirectLine` like the `secret` and the endpoints, plus the list of `Entries` which we will be testing.

Note that, the `Entries` now it's a list of `TestEntryFlow` and not `TestEntry`.

```csharp
public class TestEntryFlowCollection
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
    public List<TestEntryFlow> Entries { get; set; }
}
```

## Creating the `TestMethod` for flow cases

### New flow

First of all, take a look again at the diagram again(it's the same that I posted above).

[![https://gyazo.com/ef77fa168d3b5f8b4a116ff38b5edd83](https://i.gyazo.com/ef77fa168d3b5f8b4a116ff38b5edd83.png)](https://gyazo.com/ef77fa168d3b5f8b4a116ff38b5edd83)

As you can see, the flow structure in order to make the test is pretty much the same:

1.  Get information
2.  Authenticate
3.  Create conversation

**And here it what is changes, now we have to send multiple times all the request to the conversation. In order to do this, we have to loop for each request, send it to the bot, and then compare the latest response to our expected response.**

4.  Send all requests
5.  Get all messages
6.  Get latest response
7.  Compare with expected response
8.  Assert result

### Code

First of all, we need to get the information from the file, this is the same as we did before with the single cases.

```csharp
// Load entries from file
var path = System.IO.File.ReadAllText(@"C:\dataFlow.json");

// Deserialize to object
var data = JsonConvert.DeserializeObject<TestEntryFlowCollection>(path);
```

Now we have to loop for each `TestEntryFlow`, of the `data.entries`, with that we can follow the same flow that we did in the single cases until the new part, where we loop in the `requests`.

```csharp
/// Arrange with current requested values
string token, newToken, conversationId;
Activity latestResponse = new Activity();

/// Act for step

/// 1 - Get token using secret from DirectLine in BotFramework panel
token = Utils.uploadString<DirectLineAuth>(data.Secret, data.DirectLineGenerateTokenEndpoint, "").token;

/// 2 - Create a new conversation
var createdConversation = Utils.uploadString<DirectLineAuth>(token, data.DirectLineConversationEndpoint, "");

// This returns a new token and a conversationId
newToken = createdConversation.token;
conversationId = createdConversation.conversationId;

/// 3 - Send an activity to the conversation with new token and conversationId
string directlineConversationActivitiesEndpoint = data.DirectLineConversationEndpoint + conversationId + "/activities";
```

The following step is pretty simple, we have to loop in the the `entry.requests` and send every `activity` to the conversation.

```csharp
foreach (Activity step in entry.Requests)
{
    if (step.Type == ActivityTypes.Message)
    {
        /// Step
        Utils.uploadString<DirectLineAuth>(newToken, directlineConversationActivitiesEndpoint, JsonConvert.SerializeObject(step));

        /// 4 - Get all activities, we get a List<activity> and a watermark
        var getLastActivity = Utils.downloadString<ActivityResponse>(newToken, directlineConversationActivitiesEndpoint);

        /// 5 - Get the latest activity which is the response we should be expecting
        latestResponse = getLastActivity.activities[Int32.Parse(getLastActivity.watermark)];
    }
}
```

We use the `watermark` to get th latest message, the `watermark` is a value that the DirectLine API returns when asking for the conversation information.

After that, we just have to fill the `globals` with our `latestReponse` and `expectedResponse`.

```csharp
/// Arrange with new values
var globals = new Objects.Globals { Request = entry.Response, Response = latestResponse };
```

And to finish the case, we evaluate the `assert` string in the `entry`.

```csharp
/// Assert
Assert.IsTrue(await CSharpScript.EvaluateAsync<bool>(entry.Assert, globals: globals));
```

## Final code

```csharp
[TestMethod]
public async Task ShouldTestFlowCases()
{
    // Load entries from file
    var path = System.IO.File.ReadAllText(@"C:\dataFlow.json");

    // Deserialize to object
    var data = JsonConvert.DeserializeObject<TestEntryFlowCollection>(path);

    /// Flow: Arrange -> Act -> arrange -> assert
    foreach (TestEntryFlow entry in data.Entries)
    {
        /// Arrange with current requested values
        string token, newToken, conversationId;
        Activity latestResponse = new Activity();

        /// Act for step

        /// 1 - Get token using secret from DirectLine in BotFramework panel
        token = Utils.uploadString<DirectLineAuth>(data.Secret, data.DirectLineGenerateTokenEndpoint, "").token;

        /// 2 -Create a new conversation
        var createdConversation = Utils.uploadString<DirectLineAuth>(token, data.DirectLineConversationEndpoint, "");

        // This returns a new token and a conversationId
        newToken = createdConversation.token;
        conversationId = createdConversation.conversationId;

        /// 3 - Send an activity to the conversation with new token and conversationId
        string directlineConversationActivitiesEndpoint = data.DirectLineConversationEndpoint + conversationId + "/activities";

        foreach (Activity step in entry.Requests)
        {
            if (step.Type == ActivityTypes.Message)
            {
                /// Step
                Utils.uploadString<DirectLineAuth>(newToken, directlineConversationActivitiesEndpoint, JsonConvert.SerializeObject(step));

                /// 4 - Get all activities, we get a List<activity> and a watermark
                var getLastActivity = Utils.downloadString<ActivityResponse>(newToken, directlineConversationActivitiesEndpoint);

                /// 5 - Get the latest activity which is the response we should be expecting
                latestResponse = getLastActivity.activities[Int32.Parse(getLastActivity.watermark)];
            }
        }

        /// Arrange with new values
        var globals = new Objects.Globals { Request = entry.Response, Response = latestResponse };

        /// Assert
        Assert.IsTrue(await CSharpScript.EvaluateAsync<bool>(entry.Assert, globals: globals));
    }

    await Task.CompletedTask;
}
```

## Improvements

I do belive that a better flow could be possible, but this improvement will means that the JSON structure should be changed aswell. Also, in order to make this happen, the json have to be more filled up.

In order to make this testing better, we should have the a `response` for each `request`, and we should be asserting every time we send a message. The way that we are doing it right now is by storing all the `requests` and the **final `response`** .

I made a diagram to show how this would look.

[![https://gyazo.com/dfd4e9f87ff69159f02a0bcc70ae1edc](https://i.gyazo.com/dfd4e9f87ff69159f02a0bcc70ae1edc.png)](https://gyazo.com/dfd4e9f87ff69159f02a0bcc70ae1edc)

I strongly think that this way is much better overall for the integrity of the test, since you are testing pretty much every behaviour in the flow instead of just testing the final response.

-----

**Well that's all for this guide, please remember that this guide is meant to be a continuation fo the single cases guide, if you feel lost, check that guide which is longer and has more explanation for everything.**

Remember that all the code is stored in my github in [this](https://github.com/emimontesdeoca/integration-test-directline-bot-framework) repository.

Have a good day!