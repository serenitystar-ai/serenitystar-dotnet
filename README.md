![Serenity .NET SDK](.github/resources/sdk-banner.png)

<p align="center">
  <a href="https://serenitystar.ai/">
    <h1 align="center">Serenity Star SDK for .NET</h1>
  </a>
</p>

<p align="center">
  <a aria-label="try Serenity Star" href="https://hub.serenitystar.ai/Identity/Account/Register"><b>Try Serenity Star</b></a>
&ensp;•&ensp;
  <a aria-label="Serenity documentation" href="https://docs.serenitystar.ai">Read the Documentation</a>
&ensp;•&ensp;
  <a aria-label="Serenity blog" href="https://docs.serenitystar.ai/blog">Learn more on our blog</a>
&ensp;•&ensp;
  <a aria-label="Serenity Discord channel" href="https://discord.gg/SrT3xP7tS8">Chat with us on Discord</a>
</p>

## Introduction

Official .NET SDK for Serenity Star API. The Serenity Star .NET SDK provides a comprehensive interface for interacting with Serenity's different types of agents, such as activities, assistants, proxies, and chat completions.

## Table of Contents

- [Installation](#-installation)
- [Usage](#-usage)
- [Assistants / Copilots](#assistants--copilots)
- [Activities](#activities)
- [Proxies](#proxies)
- [Chat Completions](#chat-completions)
- [Documentation](#-documentation)

## 🚀 Installation

```bash
dotnet add package SubgenAI.SerenityStar.SDK
```

## 🔧 Usage

### Direct Instantiation

```csharp
using SerenityStar.Client;

SerenityClient client = SerenityClient.Create("your-api-key");

// Execute an activity agent
AgentResult response = await client.Agents.Activities.ExecuteAsync("marketing-campaign");
Console.WriteLine(response.Content);
```

### Using Dependency Injection

```csharp
// In your startup/program.cs
using SerenityStar.Extensions;
services.AddSerenityStar("your-api-key");

// In your service/controller
public class YourService
{
    private readonly ISerenityClient _client;

    public YourService(ISerenityClient client)
    {
        _client = client;
    }

    public async Task DoSomething()
    {
        AgentResult response = await _client.Agents.Activities.ExecuteAsync("marketing-campaign");
        Console.WriteLine(response.Content);
    }
}
```

## Assistants / Copilots

Assistants are conversational agents that maintain context across multiple messages. They're perfect for building chatbots, virtual assistants, and interactive applications.

### Create a conversation and send messages

```csharp
SerenityClient client = SerenityClient.Create("your-api-key");

// Create conversation instance (no API call yet)
Conversation conversation = client.Agents.Assistants.CreateConversation("chef-assistant");

// First message - creates the conversation in Serenity Star automatically
AgentResult response = await conversation.SendMessageAsync("I would like to get a recipe for parmesan chicken");
Console.WriteLine(response.Content);
Console.WriteLine($"Conversation ID: {conversation.ConversationId}");

// Subsequent messages use the conversation ID automatically
AgentResult response2 = await conversation.SendMessageAsync("Can you suggest a side dish?");
Console.WriteLine(response2.Content);
```

### Resume an existing conversation

```csharp
// Resume a conversation using an existing conversation ID
Conversation existingConversation = client.Agents.Assistants.CreateConversation(
    "chef-assistant",
    conversationId: "existing-conversation-id"
);

AgentResult response = await existingConversation.SendMessageAsync("What was the recipe you suggested earlier?");
Console.WriteLine(response.Content);
```

### Get conversation information

You can retrieve detailed information about an assistant agent, including its configuration, capabilities, and metadata. This is useful for understanding what parameters the agent accepts and how it's configured.

```csharp
using SerenityStar.Models.Conversation;
using SerenityStar.Models.Execute;

// Get basic agent information by code
ConversationInfoResult agentInfo = await client.Agents.Assistants.GetInfoByCodeAsync("chef-assistant");

Console.WriteLine($"Initial Message: {agentInfo.Conversation.InitialMessage}"); // "Hello! I'm your personal chef assistant..."
Console.WriteLine($"Starters: {string.Join(", ", agentInfo.Conversation.Starters)}"); // ["What's for dinner tonight?", "Help me plan a meal", ...]
Console.WriteLine($"Version: {agentInfo.Agent.Version}"); // 1
Console.WriteLine($"Vision Enabled: {agentInfo.Agent.VisionEnabled}"); // true/false
Console.WriteLine($"Is Realtime: {agentInfo.Agent.IsRealtime}"); // true/false
if (agentInfo.Channel != null)
    Console.WriteLine($"Channel: {agentInfo.Channel}"); // Optional chat widget configuration
Console.WriteLine($"Image ID: {agentInfo.Agent.ImageId}"); // Agent's profile image ID


// Get information about an assistant agent conversation (advanced example with options)
ConversationInfoResult agentInfoAdvanced = await client.Agents.Assistants.GetInfoByCodeAsync(
    "chef-assistant",
    new AgentExecutionOptions
    {
        AgentVersion = 2, // Target specific version of the agent
        InputParameters = new Dictionary<string, object>
        {
            ["dietaryRestrictions"] = "vegetarian",
            ["cuisinePreference"] = "italian",
            ["skillLevel"] = "beginner"
        },
        UserIdentifier = "user-123",
        Channel = "web"
    }
);

Console.WriteLine($"Initial Message: {agentInfoAdvanced.Conversation.InitialMessage}"); // "Hello! I'm your personalized Italian vegetarian chef assistant for beginners..."
Console.WriteLine($"Starters: {string.Join(", ", agentInfoAdvanced.Conversation.Starters)}"); // ["Show me easy vegetarian pasta recipes", "What Italian herbs should I use?", ...]
Console.WriteLine($"Version: {agentInfoAdvanced.Agent.Version}"); // 2
```

### Get conversation by id

You can retrieve an existing conversation by its ID, including all messages and metadata. This is useful for reviewing conversation history or resuming conversations.

```csharp
using SerenityStar.Models.Conversation;

SerenityClient client = SerenityClient.Create("your-api-key");

// Get conversation by id (basic example)
ConversationDetails conversation = await client.Agents.Assistants.GetConversationByIdAsync(
    "chef-assistant",
    "conversation-id-here"
);

Console.WriteLine($"Conversation ID: {conversation.Id}");
Console.WriteLine($"Is Open: {conversation.Open}");
Console.WriteLine($"Message Count: {conversation.Messages.Count}");

// Display all messages
foreach (ConversationMessage message in conversation.Messages)
    Console.WriteLine($"{message.Role}: {message.Content}");

// Get conversation by id with executor task logs
ConversationResult conversationWithLogs = await client.Agents.Assistants.GetConversationByIdAsync(
    "chef-assistant",
    "conversation-id-here",
    new GetConversationOptions
    {
        ShowExecutorTaskLogs = true
    }
);

Console.WriteLine($"Executor Task Logs: {conversationWithLogs.ExecutorTaskLogs}");
```

### Get conversation by id

## Activities

Activities are single-execution agents designed for one-time tasks like translations, data processing, or content generation. Unlike assistants, they don't maintain conversation history.

### Stream message with SSE

```csharp
using SerenityStar.Models.Execute;

AgentResult response = await client.Agents.Activities.ExecuteAsync(
    "translator-activity",
    new AgentExecutionOptions
    {
        InputParameters = new Dictionary<string, object>
        {
            ["targetLanguage"] = "russian",
            ["textToTranslate"] = "hello world"
        },
        AgentVersion = 1,
        UserIdentifier = "user-123"
    }
);

Console.WriteLine($"Translation: {response.Content}");
Console.WriteLine($"Tokens Used: {response.CompletionUsage?.TotalTokens}");
```

### Get activity information

```csharp
using SerenityStar.Models.Conversation;

// Get activity agent information
ConversationInfoResult activityInfo = await client.Agents.Activities.GetInfoByCodeAsync("translator-activity");

Console.WriteLine($"Activity: {activityInfo.Name}");
Console.WriteLine($"Type: {activityInfo.AgentType}");

// Check supported parameters
if (activityInfo.InputParameters != null)
{
    Console.WriteLine("\nSupported Parameters:");
    foreach (var param in activityInfo.InputParameters)
    {
        Console.WriteLine($"- {param.Key}");
    }
}
```

## Proxies

Proxies provide direct access to AI models with consistent interfaces across different providers. They're ideal when you need fine-grained control over model parameters.

### Execute a proxy

```csharp
using SerenityStar.Models.AIProxy;
using SerenityStar.Models.Execute;

AgentResult response = await client.Agents.Proxies.ExecuteAsync(
    "proxy-agent",
    new ProxyExecutionOptions
    {
        Model = "gpt-4o-mini-2024-07-18",
        Messages = new List<ProxyExecutionMessage>
        {
            new ProxyExecutionMessage
            {
                Role = "system",
                Content = "You are a helpful AI assistant specialized in explaining complex topics."
            },
            new ProxyExecutionMessage
            {
                Role = "user",
                Content = "What is artificial intelligence?"
            }
        },
        Temperature = 0.7,
        MaxTokens = 500
    }
);

Console.WriteLine(response.Content);
```

### Get proxy information

```csharp
using SerenityStar.Models.Conversation;

// Get proxy configuration and supported models
ConversationInfoResult proxyInfo = await client.Agents.Proxies.GetInfoByCodeAsync("proxy-agent");

Console.WriteLine($"Proxy: {proxyInfo.Name}");
Console.WriteLine($"Description: {proxyInfo.Description}");
```

## Chat Completions

Chat Completions provide a simplified interface for single-turn or multi-turn conversations without the overhead of managing conversation instances.

### Execute a chat completion

```csharp
using SerenityStar.Models.ChatCompletion;
using SerenityStar.Models.Execute;

AgentResult response = await client.Agents.ChatCompletions.ExecuteAsync(
    "Health-Coach",
    new ChatCompletionOptions
    {
        Message = "Hi! How can I eat healthier?",
        Messages = new List<ChatMessage>
        {
            new ChatMessage
            {
                Role = "system",
                Content = "You are a professional health coach."
            },
            new ChatMessage
            {
                Role = "assistant",
                Content = "Hi there! How can I assist you?"
            }
        },
        UserIdentifier = "user-123"
    }
);

Console.WriteLine(response.Content);
```

### Multi-turn conversation with chat completions

```csharp
using SerenityStar.Models.ChatCompletion;

List<ChatMessage> conversationHistory = new List<ChatMessage>
{
    new ChatMessage { Role = "system", Content = "You are a helpful fitness advisor." }
};

// First message
ChatCompletionOptions options = new ChatCompletionOptions
{
    Message = "What's a good workout routine for beginners?",
    Messages = conversationHistory
};

AgentResult response1 = await client.Agents.ChatCompletions.ExecuteAsync("Health-Coach", options);
Console.WriteLine($"Assistant: {response1.Content}");

// Add to history
conversationHistory.Add(new ChatMessage { Role = "user", Content = options.Message });
conversationHistory.Add(new ChatMessage { Role = "assistant", Content = response1.Content });

// Follow-up message
options.Message = "How many days per week should I exercise?";
options.Messages = conversationHistory;

AgentResult response2 = await client.Agents.ChatCompletions.ExecuteAsync("Health-Coach", options);
Console.WriteLine($"Assistant: {response2.Content}");
```

### Get chat completion agent information

```csharp
using SerenityStar.Models.Conversation;

ConversationInfoResult chatInfo = await client.Agents.ChatCompletions.GetInfoByCodeAsync("Health-Coach");

Console.WriteLine($"Agent: {chatInfo.Name}");
Console.WriteLine($"Version: {chatInfo.LatestVersion}");
```

## 📚 Documentation

- [Getting Started](https://docs.serenitystar.ai/docs/getting-started/introduction)
- [API Reference](https://docs.serenitystar.ai/docs/api/aihub/serenity-star-api-docs)
- [Agents](https://docs.serenitystar.ai/docs/serenity-aihub/agents)

## 📄 License

This project is licensed under the [MIT License](LICENSE).
