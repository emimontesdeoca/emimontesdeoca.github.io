#### In this part, we will make the DirectLine authorization and get the values from the bot's response.

Now that we have done the deserialize, it's time to get the information from the collection and the entires which we will be using to get authorization, make the api calls and assert the result.

**The following explanation won't cover all the basic information of how the Bot Framework works, if you dont undestand, please go and check the official documentation.**

## Making API calls using WebClient

In order to make easier for us to call the api, I created a `utils` class, where we save the functions that we will be using a few times, this class includes `uploadString` for POST and `downloadString` for GET.

```csharp
    public class Utils
    {
        /// <summary>
        /// Uploads to an URL and gets result
        /// </summary>
        /// <typeparam name="T">Type of object you are receiving</typeparam>
        /// <param name="bearer">Token</param>
        /// <param name="url">Url</param>
        /// <param name="serializedJson">Serialized JSON to send</param>
        /// <returns></returns>
        public static T uploadString<T>(string bearer, string url, string serializedJson)
        {
            string serializedResult = "";

            /// Webclient
            using (var client = new WebClient())
            {
                /// Add headers
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Authorization", $"Bearer {bearer}");

                /// Upload string
                serializedResult = client.UploadString(url, serializedJson);
            }

            /// Get result and return it as an object
            return JsonConvert.DeserializeObject<T>(serializedResult);
        }

        /// <summary>
        /// Downloads from URL
        /// </summary>
        /// <typeparam name="T">Type of object you are receiving</typeparam>
        /// <param name="bearer">Token</param>
        /// <param name="url">Url</param>
        /// <returns></returns>
        public static T downloadString<T>(string bearer, string url)
        {
            string serializedResult = "";

            /// Webclient
            using (var client = new WebClient())
            {
                /// Add headers
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Authorization", $"Bearer {bearer}");

                /// Download string
                serializedResult = client.DownloadString(url);
            }

            /// Get result and return it as an object
            return JsonConvert.DeserializeObject<T>(serializedResult);
        }
    }
```

## DirectLine authorization

If you read the official documentation, you can find how to do it, and it is quite easy, using our functions it's even easier. First of all remember that we are inside the foreach statement, since we are doing the authetication for each case, in case we run out of time, which will means that the test will fail.

```csharp
/// Arrange with current requested values
string token, newToken, conversationId;
/// Act

/// 1 - Get token using secret from DirectLine in BotFramework panel
token = Utils.uploadString<DirectLineAuth>(data.Secret, data.DirectLineGenerateTokenEndpoint, "").token;
```

Now we have the token, which will be used to make all the following calls to the conversation endpoint.

## Creating a conversation.

In order to talk to the bot, we first need to create a conversation, this conversation will return a new token which includes the conversation id.

```csharp
/// 2 -Create a new conversation
var createdConversation = Utils.uploadString<DirectLineAuth>(token, data.DirectLineConversationEndpoint, "");

// This returns a new token and a conversationId
newToken = createdConversation.token;
conversationId = createdConversation.conversationId;
```

Also, we do store the `newToken` and `conversationId`, both will be needed for the user to send messages to the bot.

## Send activity to conversation

Now, with the `conversationId` and the `conversationEndpoint`, we can create the final endpoint to send an `Activity` which is the `request` from the json file.

```csharp
/// 3 - Send an activity to the conversation with new token and conversationId
string directlineConversationActivitiesEndpoint = data.DirectLineConversationEndpoint + conversationId + "/activities";
Utils.uploadString<DirectLineAuth>(newToken, directlineConversationActivitiesEndpoint, JsonConvert.SerializeObject(entry.Request));
```

## Get latest message

In the message history, after we sended the activity, the bot should have responded already, so we have to get all the messages with the watermark and then using that watermark, filter the latest message/activity.

```csharp
/// 4 - Get all activities, we get a List<activity> and a watermark
var getLastActivity = Utils.downloadString<ActivityResponse>(newToken, directlineConversationActivitiesEndpoint);

/// 5 - Get the latest activity which is the response we should be expecting
var latestResponse = getLastActivity.activities[Int32.Parse(getLastActivity.watermark)];
```

And that's all for this part, the next part will include the part where we get the text from the `assert` in the json, convert it to code like using `eval()` in Javascript but in C#, and then using the `Assert.isTrue()` to get the final test result.

Remember that all the code is stored in my github in [this](https://github.com/emimontesdeoca/integration-test-directline-bot-framework) repository.
