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

### Stream message with SSE

You can stream responses from assistant agents using Server-Sent Events (SSE). This is useful for real-time response processing and providing a better user experience with progressive message display.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.Streaming;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create a conversation with an assistant
Conversation conversation = client.Agents.Assistants.CreateConversation("chef-assistant");

// Stream a message - get updates as they arrive
await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("Tell me a quick recipe"))
{
    switch (message)
    {
        case StreamingAgentMessageStart start:
            Console.WriteLine("Stream started");
            break;

        case StreamingAgentMessageContent content:
            Console.Write(content.Text); // Display content as it arrives
            break;

        case StreamingAgentMessageTaskStart taskStart:
            Console.WriteLine($"Task started: {taskStart.Key}");
            break;

        case StreamingAgentMessageTaskEnd taskEnd:
            Console.WriteLine($"Task completed: {taskEnd.Key} (Duration: {taskEnd.DurationMs}ms)");
            break;

        case StreamingAgentMessageStop stop:
            Console.WriteLine("\nStream completed");
            Console.WriteLine($"Conversation ID: {stop.InstanceId}");
            if (stop.CompletionUsage != null)
            {
                Console.WriteLine($"Tokens used - Input: {stop.CompletionUsage.PromptTokens}, Output: {stop.CompletionUsage.CompletionTokens}");
            }
            break;

        case StreamingAgentMessageError error:
            Console.WriteLine($"Error: {error.Error}");
            break;
    }
}
```

#### Stream subsequent messages

Once a conversation is created, you can stream additional messages using the same conversation instance:

```csharp
// First message stream (creates the conversation)
await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("What's a healthy breakfast?"))
{
    if (message is StreamingAgentMessageContent content)
        Console.Write(content.Text);
}

// Subsequent message stream (uses existing conversation)
await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("Can you add protein options?"))
{
    if (message is StreamingAgentMessageContent content)
        Console.Write(content.Text);
}
```

#### Stream with execution options

You can customize the streaming behaviour with execution options:

```csharp
// Create conversation with options
Conversation conversation = client.Agents.Assistants.CreateConversation(
    "chef-assistant",
    options: new AgentExecutionOptions
    {
        InputParameters = new Dictionary<string, object>
        {
            ["mealType"] = "dinner",
            ["servings"] = 4
        },
        UserIdentifier = "user-456",
        Channel = "mobile-app",
        AgentVersion = 2
    }
);

// Stream message with the configured options
await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("I need a recipe"))
{
    if (message is StreamingAgentMessageContent content)
        Console.Write(content.Text);
}
```

### Message feedback

You can collect user feedback on agent responses to help improve the quality of your assistant.

#### Submit feedback

```csharp
using SerenityStar.Client;
using SerenityStar.Models.MessageFeedback;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create conversation with an assistant
Conversation conversation = client.Agents.Assistants.CreateConversation("chef-assistant");

// Send a message
AgentResult response = await conversation.SendMessageAsync("I would like to get a recipe for parmesan chicken");

// Submit positive feedback (thumbs up)
await conversation.SubmitFeedbackAsync(new SubmitFeedbackOptions
{
    AgentMessageId = response.AgentMessageId!.Value,
    Feedback = true
});

// Or submit negative feedback (thumbs down)
await conversation.SubmitFeedbackAsync(new SubmitFeedbackOptions
{
    AgentMessageId = response.AgentMessageId!.Value,
    Feedback = false
});
```

#### Remove feedback

```csharp
using SerenityStar.Client;
using SerenityStar.Models.MessageFeedback;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create conversation with an assistant
Conversation conversation = client.Agents.Assistants.CreateConversation("chef-assistant");

// Send a message
AgentResult response = await conversation.SendMessageAsync("I would like to get a recipe for parmesan chicken");

// Submit feedback first
await conversation.SubmitFeedbackAsync(new SubmitFeedbackOptions
{
    AgentMessageId = response.AgentMessageId!.Value,
    Feedback = true
});

// Remove the feedback if the user changes their mind
await conversation.RemoveFeedbackAsync(new RemoveFeedbackOptions
{
    AgentMessageId = response.AgentMessageId!.Value
});
```

### Connector Status

Check the connection status of an agent's connector.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.Connector;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create conversation with an assistant
Conversation conversation = client.Agents.Assistants.CreateConversation("chef-assistant");

// Send a message that might require a connector
AgentResult response = await conversation.SendMessageAsync("I need a summary of my latest meeting notes stored in google drive");

// Here the user should complete the authentication process.

// Check connector status for this conversation (you can use a loop to check every 5 seconds)
ConnectorStatusResponse status = await conversation.GetConnectorStatusAsync(new GetConnectorStatusOptions
{
    AgentInstanceId = Guid.Parse(conversation.ConversationId!),
    // Get the connector id using response.PendingActions[index].ConnectorId
    ConnectorId = Guid.Parse("connector-uuid")
});

Console.WriteLine($"Is Connected: {status.IsConnected}"); // true or false

// You can use this to determine if a connector needs authentication
if (!status.IsConnected)
{
    Console.WriteLine("Connector is not connected. Please authenticate.");
    // After user authenticates the connector...
}

// Once connected, send the message again
// The agent will now have access to Google Drive to retrieve the meeting notes
AgentResult newResponse = await conversation.SendMessageAsync("I need a summary of my latest meeting notes stored in google drive");
Console.WriteLine(newResponse.Content); // Summary of the meeting notes
```

## Activities

Activities are single-execution agents designed for one-time tasks like translations, data processing, or content generation. Unlike assistants, they don't maintain conversation history.

### Execute an activity agent

```csharp
using SerenityStar.Client;
using SerenityStar.Models.Execute;

SerenityClient client = SerenityClient.Create("your-api-key");

// Execute activity (basic example)
AgentResult response = await client.Agents.Activities.ExecuteAsync("translator-activity");

Console.WriteLine(response.Content);

// Execute activity (advanced example)
AgentResult responseAdvanced = await client.Agents.Activities.ExecuteAsync(
    "translator-activity",
    new AgentExecutionOptions
    {
        InputParameters = new Dictionary<string, object>
        {
            ["targetLanguage"] = "russian",
            ["textToTranslate"] = "hello world"
        }
    }
);

Console.WriteLine(responseAdvanced.Content); // Привет, мир!
Console.WriteLine($"Completion Usage: {responseAdvanced.CompletionUsage?.TotalTokens}"); // { completion_tokens: 200, prompt_tokens: 30, total_tokens: 230 }

if (responseAdvanced.ExecutorTaskLogs != null)
    foreach (ExecutorTaskLog log in responseAdvanced.ExecutorTaskLogs)
        Console.WriteLine($"Task: {log.Description}, Duration: {log.Duration}ms");
```


### Stream message with SSE

Activities support streaming responses using Server-Sent Events (SSE). This allows you to process results as they're generated.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.Streaming;
using SerenityStar.Models.Execute;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create an activity for streaming
var options = new AgentExecutionOptions
{
    InputParameters = new Dictionary<string, object>
    {
        ["textToTranslate"] = "hello world",
        ["targetLanguage"] = "spanish"
    }
};

Activity activity = client.Agents.Activities.Create("translator-activity", options);

// Stream the response
await foreach (StreamingAgentMessage message in activity.StreamAsync())
{
    switch (message)
    {
        case StreamingAgentMessageStart:
            Console.WriteLine("Translation started...");
            break;

        case StreamingAgentMessageContent content:
            Console.Write(content.Text); // "Hola" "mundo"
            break;

        case StreamingAgentMessageTaskStart taskStart:
            Console.WriteLine($"Task: {taskStart.Key}");
            break;

        case StreamingAgentMessageTaskEnd taskEnd:
            Console.WriteLine($"Task completed in {taskEnd.DurationMs}ms");
            break;

        case StreamingAgentMessageStop stop:
            Console.WriteLine($"\nTranslation complete!");
            if (stop.CompletionUsage != null)
                Console.WriteLine($"Tokens used: {stop.CompletionUsage.TotalTokens}");
            break;

        case StreamingAgentMessageError error:
            Console.WriteLine($"Error: {error.Error}");
            break;
    }
}
```

## Proxies

Proxies provide direct access to AI models with consistent interfaces across different providers. They're ideal when you need fine-grained control over model parameters.

### Execute a proxy agent

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
Console.WriteLine(response.CompletionUsage);
Console.WriteLine(response.ExecutorTaskLogs);
```

### Stream responses with SSE

Proxies support streaming responses, perfect for real-time AI model interactions. You can process tokens as they arrive.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.AIProxy;
using SerenityStar.Models.Streaming;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create a proxy for streaming
var options = new ProxyExecutionOptions
{
    Model = "gpt-4o-mini-2024-07-18",
    Messages = new List<ChatMessage>
    {
        new() { Role = "user", Content = "Write a short poem about programming" }
    },
    Temperature = 0.7f,
    MaxTokens = 200
};

Proxy proxy = client.Agents.Proxies.Create("openai-proxy", options);

// Stream the response
Console.WriteLine("Poem: ");
await foreach (StreamingAgentMessage message in proxy.StreamAsync())
{
    switch (message)
    {
        case StreamingAgentMessageStart:
            break; // Generation started

        case StreamingAgentMessageContent content:
            Console.Write(content.Text); // Print each chunk as it arrives
            break;

        case StreamingAgentMessageTaskStart taskStart:
            Console.WriteLine($"\nStarting: {taskStart.Key}");
            break;

        case StreamingAgentMessageTaskEnd taskEnd:
            Console.WriteLine($"Completed in {taskEnd.DurationMs}ms");
            break;

        case StreamingAgentMessageStop stop:
            Console.WriteLine("\n\nGeneration complete!");
            if (stop.CompletionUsage != null)
            {
                Console.WriteLine($"Input tokens: {stop.CompletionUsage.PromptTokens}");
                Console.WriteLine($"Output tokens: {stop.CompletionUsage.CompletionTokens}");
            }
            break;

        case StreamingAgentMessageError error:
            Console.WriteLine($"Error: {error.Error}");
            break;
    }
}
```

### Proxy Execution Options

The following options can be passed when executing a proxy agent via `ProxyExecutionOptions`:

```csharp
using SerenityStar.Models.AIProxy;

AgentResult response = await client.Agents.Proxies.ExecuteAsync(
    "proxy-agent",
    new ProxyExecutionOptions
    {
        // Specify the model to use
        Model = "gpt-4-turbo",

        // Define conversation messages
        Messages = new List<ProxyExecutionMessage>
        {
            new ProxyExecutionMessage
            {
                Role = "system",
                Content = "You are a knowledgeable AI assistant."
            },
            new ProxyExecutionMessage
            {
                Role = "user",
                Content = "Can you explain the theory of relativity in simple terms?"
            }
        },

        // Model parameters
        Temperature = 0.7,           // Controls randomness (0-1)
        MaxTokens = 500,             // Maximum length of response
        TopP = 0.9,                  // Nucleus sampling parameter
        TopK = 50,                   // Top-k sampling parameter
        FrequencyPenalty = 0.5,      // Reduces repetition (-2 to 2)
        PresencePenalty = 0.2,       // Encourages new topics (-2 to 2)

        // Additional options
        Vendor = "openai",           // AI provider
        UserIdentifier = "user_123", // Unique user ID
        GroupIdentifier = "org_456", // Organization ID
        UseVision = false            // Enable/disable vision features
    }
);
```

## Chat Completions

Chat Completions provide a simplified interface for single-turn or multi-turn conversations without the overhead of managing conversation instances.

### Execute a chat completion

```csharp
using SerenityStar.Client;
using SerenityStar.Models.ChatCompletion;
using SerenityStar.Models.Execute;

SerenityClient client = SerenityClient.Create("your-api-key");

// Execute chat completion (basic example)
AgentResult response = await client.Agents.ChatCompletions.ExecuteAsync("AgentCreator", new ChatCompletionOptions
{
    Message = "Hello!!!"
});

Console.WriteLine(response.Content); // AI-generated response
Console.WriteLine($"Completion Usage: {response.CompletionUsage?.TotalTokens}"); // { completion_tokens: 200, prompt_tokens: 30, total_tokens: 230 }

// Execute chat completion (advanced example)
AgentResult responseAdvanced = await client.Agents.ChatCompletions.ExecuteAsync("Health-Coach", new ChatCompletionOptions
{
    UserIdentifier = "user-123",
    AgentVersion = 2,
    Channel = "web",
    VolatileKnowledgeIds = new List<string> { "knowledge-1", "knowledge-2" },
    Message = "Hi! How can I eat healthier?",
    Messages = new List<ChatMessage>
    {
        new ChatMessage
        {
            Role = "assistant",
            Content = "Hi there! How can I assist you?"
        }
    }
});

Console.WriteLine(responseAdvanced.Content); // AI-generated response
Console.WriteLine($"Completion Usage: {responseAdvanced.CompletionUsage?.TotalTokens}"); // { completion_tokens: 200, prompt_tokens: 30, total_tokens: 230 }
```

### Stream responses with SSE

Chat completions support streaming for real-time chat interactions. Process model responses token-by-token.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.ChatCompletion;
using SerenityStar.Models.Streaming;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create a chat completion for streaming
var options = new ChatCompletionOptions
{
    Message = "Explain quantum computing in simple terms",
    UserIdentifier = "user-123"
};

ChatCompletion chatCompletion = client.Agents.ChatCompletions.Create("assistant-chat", options);

// Stream the response
Console.WriteLine("Answer: ");
await foreach (StreamingAgentMessage message in chatCompletion.StreamAsync())
{
    switch (message)
    {
        case StreamingAgentMessageStart:
            break; // Stream initialized

        case StreamingAgentMessageContent content:
            Console.Write(content.Text); // Print each chunk
            break;

        case StreamingAgentMessageTaskStart taskStart:
            Console.WriteLine($"\n[Processing: {taskStart.Key}]");
            break;

        case StreamingAgentMessageTaskEnd taskEnd:
            Console.WriteLine($"[Task completed in {taskEnd.DurationMs}ms]");
            break;

        case StreamingAgentMessageStop stop:
            Console.WriteLine("\n\n[Response complete]");
            if (stop.CompletionUsage != null)
            {
                Console.WriteLine($"Tokens - Input: {stop.CompletionUsage.PromptTokens}, Output: {stop.CompletionUsage.CompletionTokens}");
            }
            if (stop.TimeToFirstToken.HasValue)
            {
                Console.WriteLine($"Time to first token: {stop.TimeToFirstToken}ms");
            }
            break;

        case StreamingAgentMessageError error:
            Console.WriteLine($"[Error: {error.Error}]");
            break;
    }
}
```

## Volatile Knowledge

Volatile knowledge allows you to upload temporary documents that can be used in agent executions. These documents are processed and made available for a limited time based on your configuration.

### Upload Volatile Knowledge

```csharp
using FileStream fileStream = File.OpenRead("path/to/document.pdf");
UploadVolatileKnowledgeReq uploadRequest = new()
{
    FileStream = fileStream,
    FileName = "document.pdf"
};

VolatileKnowledge knowledge = await client.Agents.VolatileKnowledge.UploadAsync(uploadRequest);
Console.WriteLine($"Uploaded knowledge ID: {knowledge.Id}, Status: {knowledge.Status}");
```

### Upload Volatile Knowledge with Text Content

```csharp
UploadVolatileKnowledgeReq uploadRequest = new()
{
    Content = "This is important information that needs to be processed."
};

VolatileKnowledge knowledge = await client.Agents.VolatileKnowledge.UploadAsync(uploadRequest);
Console.WriteLine($"Uploaded knowledge ID: {knowledge.Id}, Status: {knowledge.Status}");
```

### Upload with Custom Parameters

```csharp
using FileStream fileStream = File.OpenRead("document.pdf");
UploadVolatileKnowledgeReq uploadRequest = new()
{
    FileStream = fileStream,
    FileName = "document.pdf",
    CallbackUrl = "https://your-app.com/knowledge-callback"
};

VolatileKnowledge knowledge = await client.Agents.VolatileKnowledge.UploadAsync(
    uploadRequest,
    processEmbeddings: true,      // Enable embedding processing (Use `false` for Vision)
    noExpiration: false,           // Document will expire
    expirationDays: 7);            // Expire in 7 days

Console.WriteLine($"Uploaded knowledge ID: {knowledge.Id}");
if (knowledge.ExpirationDate.HasValue)
    Console.WriteLine($"Expires on: {knowledge.ExpirationDate.Value}");
```

### Check Volatile Knowledge Status

```csharp
VolatileKnowledge status = await client.Agents.VolatileKnowledge.GetStatusAsync(knowledge.Id);
Console.WriteLine($"Status: {status.Status}");

// Wait until the knowledge is ready
while (status.Status != VolatileKnowledgeSimpleStatus.Success && status.Status != VolatileKnowledgeSimpleStatus.Error)
{
    await Task.Delay(1000);
    status = await client.Agents.VolatileKnowledge.GetStatusAsync(knowledge.Id);
}

if (status.Status == VolatileKnowledgeSimpleStatus.Error)
    Console.WriteLine($"Processing failed: {status.Error}");
```

### Use Volatile Knowledge in Agent Execution

Once the volatile knowledge is ready, you can use it in agent executions by passing the knowledge IDs:

```csharp
// Upload and wait for processing
using FileStream fileStream = File.OpenRead("document.pdf");
UploadVolatileKnowledgeReq uploadRequest = new()
{
    FileStream = fileStream,
    FileName = "document.pdf"
};

VolatileKnowledge knowledge = await client.Agents.VolatileKnowledge.UploadAsync(uploadRequest);

// Wait until ready
VolatileKnowledge status = knowledge;
while (status.Status != VolatileKnowledgeSimpleStatus.Success && status.Status != VolatileKnowledgeSimpleStatus.Error)
{
    await Task.Delay(1000);
    status = await client.Agents.VolatileKnowledge.GetStatusAsync(knowledge.Id);
}

// Execute agent with volatile knowledge
Conversation conversation = client.Agents.Assistants.CreateConversation("assistantagent");
AgentResult result = await conversation.SendMessageAsync("Based on the uploaded document, what does it say about pricing?");
Console.WriteLine($"Agent response: {result.Content}");
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the [MIT License](LICENSE).