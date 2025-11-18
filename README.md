<p align="center">
  <a href="https://serenitystar.ai/">
    <h1 align="center">Serenity AIHub SDK for .NET</h1>
  </a>
</p>

<p align="center">
  <a aria-label="try Serenity AIHub" href="https://hub.serenitystar.ai/Identity/Account/Register"><b>Try Serenity AIHub</b></a>
&ensp;•&ensp;
  <a aria-label="Serenity documentation" href="https://docs.serenitystar.ai">Read the Documentation</a>
&ensp;•&ensp;
  <a aria-label="Serenity blog" href="https://docs.serenitystar.ai/blog">Learn more on our blog</a>
&ensp;•&ensp;
  <a aria-label="Serenity Discord channel" href="https://discord.gg/SrT3xP7tS8">Chat with us on Discord</a>
</p>

## Introduction

Official .NET SDK for Serenity AIHub API. This SDK provides a convenient way to interact with Serenity AIHub services from your .NET applications, allowing you to create conversations, send messages, and execute commands through a clean and intuitive interface.

## 🚀 Installation

Install the Serenity AIHub SDK via NuGet:

```bash
dotnet add package Serenity.AIHub.SDK.NET.Core
```

If you want to use dependency injection:

```bash
dotnet add package Serenity.AIHub.SDK.NET.Extensions
```

## 🔧 Usage

### Direct Instantiation

Create and use the client directly by providing your API key:

```csharp
using Serenity.AIHub.SDK.NET.Core.Client;

var client = SerenityAIHubClient.Create("your-api-key");
var conversation = await client.CreateConversation("assistantagent", null);
var response = await client.SendMessage("assistantagent", conversation.ChatId, "Hello!");
```

### Using Dependency Injection

Register the client in your service collection:

```csharp
// In your startup/program.cs
using Serenity.AIHub.SDK.NET.Extensions;
services.AddSerenityAIHub("your-api-key");
```

Inject and use the client in your services:

```csharp
// In your service/controller
public class YourService
{
    private readonly ISerenityAIHubClient _client;

    public YourService(ISerenityAIHubClient client)
    {
        _client = client;
    }

    public async Task DoSomething()
    {
        var conversation = await _client.CreateConversation("assistantagent");

        List<ExecuteParameter> input = [];
        input.Add(new(
                "chatId",
                conversation.ChatId
            ));
        input.Add(new(
            "message",
            "Hello, how are you?"
        ));
        var response = await _client.Execute("assistantagent", input);
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

VolatileKnowledge knowledge = await client.UploadVolatileKnowledgeAsync(uploadRequest);
Console.WriteLine($"Uploaded knowledge ID: {knowledge.Id}, Status: {knowledge.Status}");
```

### Upload Volatile Knowledge with Text Content

```csharp
UploadVolatileKnowledgeReq uploadRequest = new()
{
    Content = "This is important information that needs to be processed."
};

VolatileKnowledge knowledge = await client.UploadVolatileKnowledgeAsync(uploadRequest);
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

VolatileKnowledge knowledge = await client.UploadVolatileKnowledgeAsync(
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
VolatileKnowledge status = await client.GetVolatileKnowledgeStatusAsync(knowledge.Id);
Console.WriteLine($"Status: {status.Status}");

// Wait until the knowledge is ready
while (status.Status != VolatileKnowledgeSimpleStatus.Success && status.Status != VolatileKnowledgeSimpleStatus.Error)
{
    await Task.Delay(1000);
    status = await client.GetVolatileKnowledgeStatusAsync(knowledge.Id);
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

VolatileKnowledge knowledge = await client.UploadVolatileKnowledgeAsync(uploadRequest);

// Wait until ready
VolatileKnowledge status = knowledge;
while (status.Status != VolatileKnowledgeSimpleStatus.Success && status.Status != VolatileKnowledgeSimpleStatus.Error)
{
    await Task.Delay(1000);
    status = await client.GetVolatileKnowledgeStatusAsync(knowledge.Id.ToString());
}

// Execute agent with volatile knowledge
List<ExecuteParameter> parameters = new();
parameters.Add(new ExecuteParameter("message", "What does this document say about pricing?"));
parameters.Add(new ExecuteParameter("chatId", "your-chat-id"));
parameters.Add(new ExecuteParameter("volatileKnowledgeIds", new List<string> { knowledge.Id.ToString() }));

AgentResult result = await client.Execute("assistantagent", parameters);
Console.WriteLine($"Agent response: {result.Content}");
```

## 📚 Documentation

<p>Learn more about Serenity AIHub <a aria-label="serenity documentation" href="https://docs.serenitystar.ai">in our official docs!</a></p>

- [Getting Started](https://docs.serenitystar.ai/docs/getting-started/introduction)
- [API Reference](https://docs.serenitystar.ai/docs/api/aihub/serenity-star-api-docs)
- [Agents](https://docs.serenitystar.ai/docs/serenity-aihub/agents)

## 🗺 Project Structure

- `src/Serenity.AIHub.SDK.NET.Core` - Core SDK package
- `src/Serenity.AIHub.SDK.NET.Extensions` - Extension methods for dependency injection
- `tests` - Test projects for the SDK

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the [MIT License](LICENSE).