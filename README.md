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

Official .NET SDK for Serenity Star API. This SDK provides a convenient way to interact with Serenity Star services from your .NET applications, allowing you to create conversations, send messages, and execute commands through a clean and intuitive interface.

## 🚀 Installation

Install the Serenity Star SDK via NuGet:

```bash
dotnet add package SerenityStar.SDK.NET
```

## 🔧 Usage

### Direct Instantiation

Create and use the client directly by providing your API key:

```csharp
using SerenityStar.SDK.NET.Core.Client;

var client = SerenityClient.Create("your-api-key");
var conversation = await client.CreateConversation("assistantagent", null);
```

### Using Dependency Injection

Register the client in your service collection:

```csharp
// In your startup/program.cs
using SerenityStar.SDK.NET.Core.Extensions;
services.AddSerenityStar("your-api-key");
```

Inject and use the client in your services:

```csharp
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
        var conversation = await _client.CreateConversation("assistantagent");

        List<ExecuteParameter> input = new List<ExecuteParameter>();
        input.Add(new ExecuteParameter("chatId", conversation.ChatId));
        input.Add(new ExecuteParameter("message", "Hello, how are you?"));
        var response = await _client.Execute("assistantagent", input);
    }
}
```

## 📚 Documentation

<p>Learn more about Serenity Star <a aria-label="serenity documentation" href="https://docs.serenitystar.ai">in our official docs!</a></p>

- [Getting Started](https://docs.serenitystar.ai/docs/getting-started/introduction)
- [API Reference](https://docs.serenitystar.ai/docs/api/aihub/serenity-star-api-docs)
- [Agents](https://docs.serenitystar.ai/docs/serenity-aihub/agents)

## 🗺 Project Structure

- `src/SerenityStar.SDK.NET.Core` - Core SDK package
- `tests` - Test projects for the SDK

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the [MIT License](LICENSE).