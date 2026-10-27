![Serenity .NET SDK](https://raw.githubusercontent.com/serenitystar-ai/serenity-dotnet/main/.github/resources/sdk-banner.png)

# Serenity Star SDK for .NET

**[Try Serenity Star](https://hub.serenitystar.ai/Identity/Account/Register)** • **[Documentation](https://docs.serenitystar.ai)** • **[Blog](https://docs.serenitystar.ai/blog)** • **[Discord](https://discord.gg/SrT3xP7tS8)**

## Introduction

Official .NET SDK for Serenity Star API. The Serenity Star .NET SDK provides a comprehensive interface for interacting with Serenity's different types of agents, such as activities, assistants, proxies, and chat completions, and with Serenity's AI services, such as anonymizer, decisions, embeddings, image generation, OCR, speech generation and transcription.

[![License: MIT](https://img.shields.io/github/license/serenitystar-ai/serenitystar-dotnet)](https://github.com/serenitystar-ai/serenitystar-dotnet/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/SubgenAI.SerenityStar.SDK)](https://www.nuget.org/packages/SubgenAI.SerenityStar.SDK)

## Table of Contents

- [Installation](#-installation)
- [Usage](#-usage)
- [Assistants / Copilots](#assistants--copilots)
- [Skill Approvals](#skill-approvals)
- [Request User Choice](#request-user-choice)
- [Agent TODOs](#agent-todos)
- [Activities](#activities)
- [AI Proxy](#ai-proxy)
- [Chat Completions](#chat-completions)
- [Audio Input for Agents](#audio-input-for-agents)
- [Volatile Knowledge](#volatile-knowledge)
- [AI Services](#ai-services)
  - [Anonymizer](#anonymizer)
  - [Decisions](#decisions)
  - [Embeddings](#embeddings)
  - [Image Generation](#image-generation)
  - [OCR](#ocr)
  - [Speech Generation](#speech-generation)
  - [Transcription](#transcription)
- [Error handling](#error-handling)
- [Migrating to 3.0](#migrating-to-30)
- [Documentation](#-documentation)

## 🚀 Installation

The SDK is split into packages, so you only install what you use:

| Package | Contents |
| --- | --- |
| `SubgenAI.SerenityStar.SDK` | Agents: assistants, copilots, activities, AI proxies and chat completions |
| `SubgenAI.SerenityStar.AIServices` | AI services: anonymizer, decisions, embeddings, image generation, OCR, speech generation and transcription |
| `SubgenAI.SerenityStar.Core` | `SerenityClient`, authentication and error handling. Installed automatically by the packages above |

```bash
# Agents
dotnet add package SubgenAI.SerenityStar.SDK

# AI services (does not require the agents package)
dotnet add package SubgenAI.SerenityStar.AIServices
```

> **Note:** All packages are published together and must be used with the same version. NuGet enforces it:
> each package depends on the exact same version of `SubgenAI.SerenityStar.Core`.

Every feature is obtained from a single `SerenityClient`: `client.GetAgents()` for agents and
`client.Get<Service>Client()` (e.g. `client.GetAnonymizerClient()`) for each AI service. These methods are
in the `SerenityStar.Client` namespace, next to `SerenityClient`.

## 🔧 Usage

### Direct Instantiation with Factory Method

```csharp
using SerenityStar.Client;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create and execute an activity agent
Activity activity = client.GetAgents().Activities.Create("marketing-campaign");
AgentResult response = await activity.ExecuteAsync();
Console.WriteLine(response.Content);
```

### Direct Instantiation with Builder Pattern

```csharp
using SerenityStar.Client;
using SerenityStar.Agents.System;

SerenityClient client = new SerenityClientBuilder()
    .WithApiKey("your-api-key")
    .WithBaseUrl("https://api.serenitystar.ai") // Optional
    .WithTimeout(30)                             // Optional
    .Build();

// Create and execute an activity agent
Activity activity = client.GetAgents().Activities.Create("marketing-campaign");
AgentResult response = await activity.ExecuteAsync();
Console.WriteLine(response.Content);
```

### Using a Custom HttpClient with Builder

```csharp
using SerenityStar.Client;
using SerenityStar.Agents.System;

var customHttpClient = new HttpClient();
// Configure your custom HttpClient as needed (e.g. timeout, handlers)
customHttpClient.Timeout = TimeSpan.FromSeconds(30);

SerenityClient client = new SerenityClientBuilder()
    .WithApiKey("your-api-key")
    .WithHttpClient(customHttpClient)
    .Build();

// Create and execute an activity agent
Activity activity = client.GetAgents().Activities.Create("marketing-campaign");
AgentResult response = await activity.ExecuteAsync();
Console.WriteLine(response.Content);
```

> **Note:** When you supply your own `HttpClient`, the SDK does not modify it. The API key is
> attached per request (not as a default header), and `BaseAddress` is not overwritten, so the
> client remains safe to share with other code. Because of this, `WithTimeout(...)` is ignored when
> a custom `HttpClient` is provided — configure the timeout on your own client instead.

### Using Dependency Injection

```csharp
// In your startup/program.cs
using SerenityStar.Extensions;
services.AddSerenityStar("your-api-key")   // Registers SerenityClient
        .AddSerenityStarAgents()           // Optional: registers AgentsScope (SubgenAI.SerenityStar.SDK)
        .AddSerenityStarAIServices();      // Optional: registers every AI service interface (SubgenAI.SerenityStar.AIServices)

// With custom base URL and timeout
services.AddSerenityStar(
    apiKey: "your-api-key",
    timeoutSeconds: 60,
    baseUrl: "https://custom-api.serenitystar.ai"
);

// In your service/controller
using SerenityStar.Agents;
using SerenityStar.Agents.System;
using SerenityStar.AIServices;

public class YourService
{
    private readonly AgentsScope _agents;
    private readonly IAnonymizerClient _anonymizer;

    // Inject SerenityClient, AgentsScope or any AI service interface
    public YourService(AgentsScope agents, IAnonymizerClient anonymizer)
    {
        _agents = agents;
        _anonymizer = anonymizer;
    }

    public async Task DoSomething()
    {
        Activity activity = _agents.Activities.Create("marketing-campaign");
        AgentResult response = await activity.ExecuteAsync();
        Console.WriteLine(response.Content);
    }
}
```

## Assistants / Copilots

Assistants and Copilots are conversational agents that maintain context across multiple messages. They're perfect for building chatbots, virtual assistants, and interactive applications. Both provide the same functionality - choose the terminology that best fits your use case.

### Create a conversation and send messages

```csharp
SerenityClient client = SerenityClient.Create("your-api-key");

// Using Assistants
Conversation conversation = client.GetAgents().Assistants.CreateConversation("chef-assistant");

// Or using Copilots (same functionality)
Conversation copilotConversation = client.GetAgents().Copilots.CreateConversation("chef-assistant");

// First message - creates the conversation in Serenity Star automatically
AgentResult response = await conversation.SendMessageAsync("I would like to get a recipe for parmesan chicken");
Console.WriteLine(response.Content);
Console.WriteLine($"Conversation ID: {conversation.ConversationId}");

// Subsequent messages use the conversation ID automatically
AgentResult response2 = await conversation.SendMessageAsync("Can you suggest a side dish?");
Console.WriteLine(response2.Content);
```

### Resume an existing conversation

Use this when you don't already have a Conversation instance in memory, for example, when you load a stored conversation ID from your database.

```csharp
// Resume a conversation using an existing conversation ID with Assistants
Conversation existingConversation = client.GetAgents().Assistants.CreateConversation(
    "chef-assistant",
    conversationId: "existing-conversation-id"
);

// Or with Copilots
Conversation existingCopilot = client.GetAgents().Copilots.CreateConversation(
    "chef-assistant",
    conversationId: "existing-conversation-id"
);

AgentResult response = await existingConversation.SendMessageAsync("What was the recipe you suggested earlier?");
Console.WriteLine(response.Content);
```

### Get conversation information

You can retrieve detailed information about an assistant/copilot agent, including its configuration, capabilities, and metadata. This is useful for understanding what parameters the agent accepts and how it's configured.

```csharp
using SerenityStar.Models.Conversation;
using SerenityStar.Models.Execute;

// Get basic agent information by code (works with both Assistants and Copilots)
ConversationInfoResult agentInfo = await client.GetAgents().Assistants.GetInfoByCodeAsync("chef-assistant");
// Or: ConversationInfoResult agentInfo = await client.GetAgents().Copilots.GetInfoByCodeAsync("chef-assistant");

Console.WriteLine($"Initial Message: {agentInfo.Conversation.InitialMessage}"); // "Hello! I'm your personal chef assistant..."
Console.WriteLine($"Starters: {string.Join(", ", agentInfo.Conversation.Starters)}"); // ["What's for dinner tonight?", "Help me plan a meal", ...]
Console.WriteLine($"Version: {agentInfo.Agent.Version}"); // 1
Console.WriteLine($"Vision Enabled: {agentInfo.Agent.VisionEnabled}"); // true/false
Console.WriteLine($"Is Realtime: {agentInfo.Agent.IsRealtime}"); // true/false
if (agentInfo.Channel != null)
    Console.WriteLine($"Channel: {agentInfo.Channel}"); // Optional chat widget configuration
Console.WriteLine($"Image ID: {agentInfo.Agent.ImageId}"); // Agent's profile image ID


// Get information about an assistant agent conversation (advanced example with specific version)
ConversationInfoResult agentInfoAdvanced = await client.GetAgents().Assistants.GetInfoByCodeAsync(
    "chef-assistant",
    2, // Target specific version of the agent
    new AgentExecutionReq
    {
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
ConversationRes conversation = await client.GetAgents().Assistants.GetConversationByIdAsync(
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
ConversationRes conversationWithLogs = await client.GetAgents().Assistants.GetConversationByIdAsync(
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
Conversation conversation = client.GetAgents().Assistants.CreateConversation("chef-assistant");

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
            if (content.Citations != null)
            {
                foreach (var citation in content.Citations)
                    Console.WriteLine($"\n[{citation.CitationIndex}] {citation.CitedText}");
            }
            break;

        case StreamingAgentMessageReasoning reasoning:
            Console.Write(reasoning.Text); // Display reasoning as it arrives
            break;

        case StreamingAgentMessageTaskStart taskStart:
            Console.WriteLine($"Task started: {taskStart.TaskKey}");
            break;

        case StreamingAgentMessageTaskStop taskStop:
            Console.WriteLine($"Task completed: {taskStop.TaskKey} (Duration: {taskStop.Duration.TotalMilliseconds}ms)");
            break;

        case StreamingAgentMessageStop stop:
            Console.WriteLine("\nStream completed");
            Console.WriteLine($"Conversation ID: {stop.Result?.InstanceId}");
            if (stop.Result?.CompletionUsage != null)
            {
                Console.WriteLine($"Tokens used - Input: {stop.Result.CompletionUsage.PromptTokens}, Output: {stop.Result.CompletionUsage.CompletionTokens}");
            }
            break;

        case StreamingAgentMessagePing:
            // Keep-alive heartbeat sent to hold the connection open; no action needed
            break;

        case StreamingAgentMessageError error:
            Console.WriteLine($"Error: {error.Message}");
            break;

        case StreamingAgentMessageUnsupported unsupported:
            // Forward-compatibility: a message type this SDK version doesn't model yet
            Console.WriteLine($"Unsupported message type: {unsupported.OriginalType}");
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
// Create conversation with specific version and options
Conversation conversation = client.GetAgents().Assistants.CreateConversation(
    "chef-assistant",
    2, // Target specific version of the agent
    options: new AgentExecutionReq
    {
        InputParameters = new Dictionary<string, object>
        {
            ["mealType"] = "dinner",
            ["servings"] = 4
        },
        UserIdentifier = "user-456",
        Channel = "mobile-app"
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

Feedback is tied to a conversation message, so it is only available for the conversational agent types — **Assistants and Copilots**. Activities, Chat Completions and AI Proxies do not create conversations and therefore cannot receive feedback.

#### Submit feedback

```csharp
using SerenityStar.Client;
using SerenityStar.Models.MessageFeedback;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create conversation with an assistant
Conversation conversation = client.GetAgents().Assistants.CreateConversation("chef-assistant");

// Send a message
AgentResult response = await conversation.SendMessageAsync("I would like to get a recipe for parmesan chicken");

// Submit positive feedback (thumbs up)
await conversation.SubmitFeedbackAsync(new SubmitFeedbackReq
{
    AgentMessageId = response.AgentMessageId!.Value,
    Feedback = true
});

// Or submit negative feedback (thumbs down)
await conversation.SubmitFeedbackAsync(new SubmitFeedbackReq
{
    AgentMessageId = response.AgentMessageId!.Value,
    Feedback = false
});
```

#### Submit feedback with a comment

Feedback can carry an optional free-text comment explaining the rating.

```csharp
await conversation.SubmitFeedbackAsync(new SubmitFeedbackReq
{
    AgentMessageId = response.AgentMessageId!.Value,
    Feedback = false,
    Comment = "The recipe skipped the cooking temperature."
});
```

Notes on `Comment`:

- It is limited to **1000 characters**. Longer values are rejected by the API with an HTTP 400 response, surfaced as a `SerenityApiException` (see [Error handling](#error-handling)).
- Leading and trailing whitespace is trimmed, and blank values are stored as no comment.
- Submitting feedback again for the same message **overwrites** the stored comment. Omitting `Comment` on a later submission clears the one submitted before it.

> [!IMPORTANT]
> The comment is free text written by your end users and it is persisted and displayed in the Serenity Star back office. Do not pre-fill it with sensitive data, and make sure your UI discourages users from entering personal or confidential information. Avoid logging comments verbatim on your side.

#### Remove feedback

```csharp
using SerenityStar.Client;
using SerenityStar.Models.MessageFeedback;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create conversation with an assistant
Conversation conversation = client.GetAgents().Assistants.CreateConversation("chef-assistant");

// Send a message
AgentResult response = await conversation.SendMessageAsync("I would like to get a recipe for parmesan chicken");

// Submit feedback first
await conversation.SubmitFeedbackAsync(new SubmitFeedbackReq
{
    AgentMessageId = response.AgentMessageId!.Value,
    Feedback = true
});

// Remove the feedback if the user changes their mind
await conversation.RemoveFeedbackAsync(new RemoveFeedbackReq
{
    AgentMessageId = response.AgentMessageId!.Value
});
```

#### Retrieve submitted feedback

You can page through all the feedback submitted for an agent, including the comments, to analyze response quality.

Unlike submitting or removing feedback, this operation needs an API key with the **Audit** permission for the agent. An execution key gets an HTTP 403 response. See the warning at the end of this section before using it.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.MessageFeedback;

// Requires an API key with the Audit permission for the agent, not an execution key
SerenityClient client = SerenityClient.Create("your-audit-api-key");

// Most recent feedback first (default page size is 20)
MessageFeedbackPage page = await client.GetAgents().Assistants.GetMessageFeedbackAsync("chef-assistant");

Console.WriteLine($"{page.Total} feedback entries in total");

foreach (MessageFeedbackRes feedback in page.Items)
{
    string rating = feedback.Feedback ? "👍" : "👎";
    Console.WriteLine($"{feedback.DateUtc:u} {rating} {feedback.Comment ?? "(no comment)"}");
}
```

Filter by date, page size and sort direction:

```csharp
MessageFeedbackPage negativeLastWeek = await client.GetAgents().Assistants.GetMessageFeedbackAsync(
    "chef-assistant",
    new GetMessageFeedbackReq
    {
        Page = 1,
        PageSize = 100,
        StartDate = DateTime.UtcNow.AddDays(-7),
        EndDate = DateTime.UtcNow,
        SortDirection = "asc"
    });
```

The same operation is available on Copilots:

```csharp
MessageFeedbackPage page = await client.GetAgents().Copilots.GetMessageFeedbackAsync("support-copilot");
```

Notes:

- `PageSize` is capped at **1000** and `SortDirection` must be `"asc"` or `"desc"`. Other values are rejected by the API with an HTTP 400 response.
- Paginate with `Page` while `page.Items.Count` is non-empty and `page.Page * page.PageSize < page.Total`.

> [!WARNING]
> This endpoint requires an API key with the **Audit** permission for the agent — a stronger permission than the **Execution** one needed to submit or remove feedback. The response exposes end-user conversation content (`UserMessage`, `AgentMessage`, `Comment`) and `UserIdentifier` across every conversation of the agent. Treat it as personal data: use a dedicated audit key rather than your public execution key, never call it from a browser or mobile client where the key would be exposed, and do not log the entries verbatim.

### Connector Status

Check the connection status of an agent's connector.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.Connector;
using SerenityStar.Models.Execute;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create conversation with an assistant
Conversation conversation = client.GetAgents().Assistants.CreateConversation("chef-assistant");

// Send a message that might require a connector
AgentResult response = await conversation.SendMessageAsync("I need a summary of my latest meeting notes stored in google drive");

// Check if there are pending actions (e.g., authentication required)
if (response.PendingActions.Count > 0)
{
    foreach (PendingAction action in response.PendingActions)
    {
        if (action is ConnectionPendingAction connectionAction)
        {
            Console.WriteLine($"Authentication required for {connectionAction.ConnectorName}");
            Console.WriteLine($"Please visit: {connectionAction.Url}");

            // Here the user should complete the authentication process.

            // Check connector status for this conversation (you can use a loop to check every 5 seconds)
            ConnectorStatusRes status = await conversation.GetConnectorStatusAsync(connectionAction.ConnectorId);

            Console.WriteLine($"Is Connected: {status.IsConnected}"); // true or false

            // You can use this to determine if a connector needs authentication
            if (!status.IsConnected)
            {
                Console.WriteLine("Connector is not connected. Please authenticate.");
                // Wait and check again...
            }
        }
    }
}

// Once connected, send the message again
// The agent will now have access to Google Drive to retrieve the meeting notes
AgentResult newResponse = await conversation.SendMessageAsync("I need a summary of my latest meeting notes stored in google drive");
Console.WriteLine(newResponse.Content); // Summary of the meeting notes
```

## Skill Approvals

When an agent invokes a skill configured to require approval, the execution pauses and returns an `ApprovalPendingAction` in `PendingActions`. It describes the skill (`SkillCode`, `SkillType`, `Tool`) and the `Arguments` the agent wants to use, so the user can review them before deciding. Answer it with `Approve()` or `Reject(reason)` to continue the execution. Every pending approval must be answered.

See the [Skill Approvals documentation](https://docs.serenitystar.ai/docs/serenity-aihub/capabilities/human-in-the-loop/skill-approvals/) for more details.

> [!NOTE]
> `Arguments` are generated by the agent and can contain personal or sensitive data. Show them to the user, but avoid logging them.

### Approve or reject in a conversation (Assistants / Copilots)

The conversation history stays on the server, so only the decisions are sent back.

```csharp
using SerenityStar.Agents.Conversational;
using SerenityStar.Client;
using SerenityStar.Models.Execute;

SerenityClient client = SerenityClient.Create("your-api-key");
Conversation conversation = client.GetAgents().Assistants.CreateConversation("email-assistant");

AgentResult response = await conversation.SendMessageAsync("Send the weekly report to john@example.com");

List<ApprovalPendingAction> approvals = response.PendingActions.OfType<ApprovalPendingAction>().ToList();
if (approvals.Count > 0)
{
    List<ApprovalDecision> decisions = new();
    foreach (ApprovalPendingAction approval in approvals)
    {
        Console.WriteLine($"The agent wants to run {approval.Tool ?? approval.SkillCode}");
        foreach (KeyValuePair<string, object> argument in approval.Arguments)
            Console.WriteLine($"  {argument.Key}: {argument.Value}");

        bool approved = AskTheUser(); // Your own confirmation logic
        decisions.Add(approved ? approval.Approve() : approval.Reject("Send it to the whole team instead"));
    }

    // Continues the execution. A message can optionally be sent in the same turn.
    response = await conversation.SendApprovalsAsync(decisions);
}

Console.WriteLine(response.Content);
```

If a pending approval is left unanswered, the API rejects the request with an HTTP 400 response. The approvals still pending can be read from the exception:

```csharp
using SerenityStar.Exceptions;
using SerenityStar.Extensions;

try
{
    response = await conversation.SendApprovalsAsync(decisions);
}
catch (SerenityApiException ex)
{
    List<PendingAction> stillPending = ex.GetPendingActions();
}
```

### Stream with approvals

The approval is surfaced as soon as it is requested through a `task_stop` event, and it is also listed in the pending actions of the `stop` event result. Use `StreamApprovalsAsync` to answer and stream the continued execution.

```csharp
using SerenityStar.Models.Streaming;

List<ApprovalPendingAction> approvals = new();

await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("Send the weekly report to john@example.com"))
{
    switch (message)
    {
        case StreamingAgentMessageContent content:
            Console.Write(content.Text);
            break;
        case StreamingAgentMessageTaskStop taskStop when taskStop.TryGetApprovalRequest(out ApprovalPendingAction? approval):
            Console.WriteLine($"Approval requested for {approval.SkillCode}");
            break;
        case StreamingAgentMessageStop stop:
            approvals.AddRange(stop.Result?.PendingActions.OfType<ApprovalPendingAction>() ?? Enumerable.Empty<ApprovalPendingAction>());
            break;
    }
}

if (approvals.Count > 0)
{
    await foreach (StreamingAgentMessage message in conversation.StreamApprovalsAsync(approvals.Select(a => a.Approve())))
    {
        if (message is StreamingAgentMessageContent content)
            Console.Write(content.Text);
        else if (message is StreamingAgentMessageError error && error.PendingActions?.Count > 0)
            Console.WriteLine("Some approvals are still pending");
    }
}
```

### Approve or reject in an AI Proxy

AI Proxy skills are sent with each request through `Skills`, and each one sets whether it requires approval. AI Proxy keeps the chat history on the client, so the approval request and the decision are echoed back in the messages. `AddApprovalDecision` appends both, then execute the proxy again with the same options (including the same `Skills`).

```csharp
using SerenityStar.Agents.System;
using SerenityStar.Models.AIProxy;

ProxyExecutionReq options = new ProxyExecutionReq
{
    Model = "gpt-4o-mini-2024-07-18",
    Messages = new List<ProxyExecutionMessage>
    {
        new ProxyExecutionMessage { Role = "user", Content = "Generate an image of a red apple on a white table" }
    },
    Skills = new ProxySkillsOptions
    {
        ImageGeneration = new ProxySkillsOptions.ImageGenerationSkillOptions { RequiresApproval = true }
    }
};

Proxy proxy = client.GetAgents().AIProxies.Create("proxy-agent", options);
AgentResult response = await proxy.ExecuteAsync();

foreach (ApprovalPendingAction approval in response.PendingActions.OfType<ApprovalPendingAction>())
    options.AddApprovalDecision(approval, approval.Approve());

response = await proxy.ExecuteAsync();
Console.WriteLine(response.Content);
```

### Auto mode

Auto mode approves every skill invocation automatically, so the agent never pauses for approval. It is disabled by default and must be enabled explicitly per conversation. It applies to Assistants and Copilots: in an AI Proxy, the skills and their `RequiresApproval` setting are sent by the client on each request.

> [!WARNING]
> Auto mode bypasses the human review configured for the agent's skills. Only enable it in trusted or fully automated contexts where nobody is available to review the invocations.

```csharp
Conversation conversation = client.GetAgents().Assistants.CreateConversation(
    "email-assistant",
    options: new AgentExecutionReq
    {
        Skills = new SkillsOptions { AutoMode = true }
    });
```

## Request User Choice

When an agent has the Request User Choice skill enabled, it can pause and ask the user to choose from a set of options. The response then carries a `UserChoicePendingAction` in `PendingActions`, with one or more `Questions`. Each question has an `Id`, a short `Header`, the question `Text`, whether it `IsMultiselect`, and its `Options` (`Id`, `Title` and an optional `Description`).

The user can answer all, some or none of the questions, picking options and/or writing a free-text answer when none of them fit. The agent continues with whatever it receives, and the unanswered questions stay in the history as context. Build each answer from its question:

- `question.Select(optionIds)`: selects one option, or several on a multi-select question.
- `question.AnswerOther(text)`: answers with free text only.
- `question.Answer(optionIds, text)`: selects options and adds free text.

These helpers validate the answer against the question: the options must belong to it, and a single-select question accepts only one option. The API doesn't validate answers, so an answer built by hand is passed to the agent as is.

See the [Request User Choice documentation](https://docs.serenitystar.ai/docs/serenity-aihub/capabilities/human-in-the-loop/request-user-choice/) for more details.

> [!NOTE]
> The answers, including the free-text ones, are stored in the conversation history and can contain personal data. Avoid logging them.

### Answer in a conversation (Assistants / Copilots)

```csharp
using SerenityStar.Agents.Conversational;
using SerenityStar.Client;
using SerenityStar.Models.Execute;

SerenityClient client = SerenityClient.Create("your-api-key");
Conversation conversation = client.GetAgents().Assistants.CreateConversation("travel-assistant");

AgentResult response = await conversation.SendMessageAsync("Help me plan a trip");

UserChoicePendingAction? userChoice = response.PendingActions.OfType<UserChoicePendingAction>().FirstOrDefault();
if (userChoice != null)
{
    List<UserChoiceAnswer> answers = new();
    foreach (UserChoiceQuestion question in userChoice.Questions)
    {
        Console.WriteLine(question.Text);
        foreach (UserChoiceOption option in question.Options)
            Console.WriteLine($"  [{option.Id}] {option.Title}");

        // Your own selection logic. For example:
        answers.Add(question.IsMultiselect
            ? question.Select(question.Options[0].Id, question.Options[1].Id)
            : question.Answer(new[] { question.Options[0].Id }, "Preferably in spring"));
    }

    // Continues the conversation. A message can optionally be sent in the same turn.
    response = await conversation.SendUserChoicesAsync(answers);
}

Console.WriteLine(response.Content);
```

To answer only some of the questions, send just those answers, optionally with a message:

```csharp
UserChoiceQuestion destination = userChoice.Questions[0];
response = await conversation.SendUserChoicesAsync(
    new[] { destination.AnswerOther("Somewhere with snow") },
    "Choose the rest for me");
```

To skip every question, send a regular message, or `SendUserChoicesAsync` with no answers and a message:

```csharp
response = await conversation.SendMessageAsync("I'd rather not choose, just give me a suggestion");
```

### Stream with user choices

The questions are surfaced as soon as they are asked through a `task_stop` event, and they are also listed in the pending actions of the `stop` event result. Use `StreamUserChoicesAsync` to answer and stream the continued conversation.

```csharp
using SerenityStar.Models.Streaming;

UserChoicePendingAction? userChoice = null;

await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("Help me plan a trip"))
{
    switch (message)
    {
        case StreamingAgentMessageContent content:
            Console.Write(content.Text);
            break;
        case StreamingAgentMessageTaskStop taskStop when taskStop.TryGetUserChoiceRequest(out UserChoicePendingAction? requested):
            userChoice = requested;
            Console.WriteLine($"The agent asked {requested.Questions.Count} question(s)");
            break;
    }
}

if (userChoice != null)
{
    IEnumerable<UserChoiceAnswer> answers = userChoice.Questions.Select(q => q.Select(q.Options[0].Id));

    await foreach (StreamingAgentMessage message in conversation.StreamUserChoicesAsync(answers))
    {
        if (message is StreamingAgentMessageContent content)
            Console.Write(content.Text);
    }
}
```

### Answer in an AI Proxy

AI Proxy enables the skill per request with `ProxySkillsOptions.UserChoice` and keeps the chat history on the client. `AddUserChoiceAnswers` echoes the questions and appends the answers, plus an optional message, to the messages. Then execute the proxy again with the same options.

```csharp
using SerenityStar.Agents.System;
using SerenityStar.Models.AIProxy;

ProxyExecutionReq options = new ProxyExecutionReq
{
    Model = "gpt-4o-mini-2024-07-18",
    Messages = new List<ProxyExecutionMessage>
    {
        new ProxyExecutionMessage { Role = "user", Content = "Help me plan a trip" }
    },
    Skills = new ProxySkillsOptions { UserChoice = true }
};

Proxy proxy = client.GetAgents().AIProxies.Create("proxy-agent", options);
AgentResult response = await proxy.ExecuteAsync();

UserChoicePendingAction? userChoice = response.PendingActions.OfType<UserChoicePendingAction>().FirstOrDefault();
if (userChoice != null)
{
    UserChoiceQuestion first = userChoice.Questions[0];
    options.AddUserChoiceAnswers(userChoice, new[] { first.Select(first.Options[0].Id) }, "Choose the rest for me");

    response = await proxy.ExecuteAsync();
}

Console.WriteLine(response.Content);
```

## Agent TODOs

When an agent has Agent TODOs enabled, it breaks complex requests into its own TODO list and keeps iterating until no item is left pending. It needs no answer from the user, but the progress of the list is reported while streaming (Assistants and Copilots).

Every TODO skill invocation, and every new iteration of the agent (`StreamingTaskKeys.AgentTodoIterate`), ends with a `task_stop` event that carries the whole list. `TryGetAgentTodos` reads it, so the latest list received holds the current progress. Each `AgentTodoItem` has an `Id`, a `Description` and a `Status` (`AgentTodoStatus.Pending`, `Completed` or `Canceled`).

See the [Agent TODOs documentation](https://docs.serenitystar.ai/docs/serenity-aihub/capabilities/agent-todos) for more details.

```csharp
using SerenityStar.Models.Streaming;

IReadOnlyList<AgentTodoItem> todos = Array.Empty<AgentTodoItem>();

await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("Research three destinations and compare them"))
{
    switch (message)
    {
        case StreamingAgentMessageContent content:
            Console.Write(content.Text);
            break;
        case StreamingAgentMessageTaskStop taskStop when taskStop.TryGetAgentTodos(out IReadOnlyList<AgentTodoItem>? current):
            todos = current;
            int resolved = todos.Count(t => t.Status != AgentTodoStatus.Pending);
            Console.WriteLine($"Progress: {resolved}/{todos.Count}");
            break;
    }
}
```

## Activities

Activities are single-execution agents designed for one-time tasks like translations, data processing, or content generation. Unlike assistants, they don't maintain conversation history.

### Execute an activity agent

```csharp
using SerenityStar.Client;
using SerenityStar.Models.Execute;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create and execute activity (basic example)
Activity activity = client.GetAgents().Activities.Create("translator-activity");
AgentResult response = await activity.ExecuteAsync();

Console.WriteLine(response.Content);

// Create and execute activity (advanced example)
Activity activityAdvanced = client.GetAgents().Activities.Create(
    "translator-activity",
    new AgentExecutionReq
    {
        InputParameters = new Dictionary<string, object>
        {
            ["targetLanguage"] = "russian",
            ["textToTranslate"] = "hello world"
        }
    }
);

AgentResult responseAdvanced = await activityAdvanced.ExecuteAsync();

Console.WriteLine(responseAdvanced.Content); // Привет, мир!
Console.WriteLine($"Completion Usage: {responseAdvanced.CompletionUsage?.TotalTokens}"); // { completion_tokens: 200, prompt_tokens: 30, total_tokens: 230 }

if (responseAdvanced.ExecutorTaskLogs != null)
    foreach (AgentResult.Task log in responseAdvanced.ExecutorTaskLogs)
        Console.WriteLine($"Task: {log.Description}, Duration: {log.Duration}ms, Success: {log.Success}");
```
### Stream message with SSE

Activities support streaming responses using Server-Sent Events (SSE). This allows you to process results as they're generated.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.Streaming;
using SerenityStar.Models.Execute;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create an activity with options
Activity activity = client.GetAgents().Activities.Create(
    "translator-activity",
    new AgentExecutionReq
    {
        InputParameters = new Dictionary<string, object>
        {
            ["textToTranslate"] = "hello world",
            ["targetLanguage"] = "spanish"
        }
    }
);

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

        case StreamingAgentMessageReasoning reasoning:
            Console.Write(reasoning.Text); // Model's reasoning tokens
            break;

        case StreamingAgentMessageTaskStart taskStart:
            Console.WriteLine($"Task: {taskStart.TaskKey}");
            break;

        case StreamingAgentMessageTaskStop taskStop:
            Console.WriteLine($"Task completed in {taskStop.Duration.TotalMilliseconds}ms");
            break;

        case StreamingAgentMessageStop stop:
            Console.WriteLine($"\nTranslation complete!");
            if (stop.Result?.CompletionUsage != null)
                Console.WriteLine($"Tokens used: {stop.Result.CompletionUsage.TotalTokens}");
            break;

        case StreamingAgentMessageError error:
            Console.WriteLine($"Error: {error.Message}");
            break;
    }
}
```

## AI Proxy

AI Proxy provides direct access to AI models with consistent interfaces across different providers. They're ideal when you need fine-grained control over model parameters.

### Execute a proxy agent

```csharp
using SerenityStar.Client;
using SerenityStar.Models.AIProxy;
using SerenityStar.Models.Execute;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create and execute a proxy agent
Proxy proxy = client.GetAgents().AIProxies.Create(
    "proxy-agent",
    new ProxyExecutionReq
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

AgentResult response = await proxy.ExecuteAsync();

Console.WriteLine(response.Content);
Console.WriteLine(response.CompletionUsage);
Console.WriteLine(response.ExecutorTaskLogs);
```

### Stream responses with SSE

AI Proxy supports streaming responses, perfect for real-time AI model interactions. You can process tokens as they arrive.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.AIProxy;
using SerenityStar.Models.Streaming;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create a proxy for streaming
ProxyExecutionReq options = new()
{
    Model = "gpt-4o-mini-2024-07-18",
    Messages = new List<ChatCompletionMessage>
    {
        new() { Role = "user", Content = "Write a short poem about programming" }
    },
    Temperature = 0.7f,
    MaxTokens = 200
};

Proxy proxy = client.GetAgents().AIProxies.Create("openai-proxy", options);

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

        case StreamingAgentMessageReasoning reasoning:
            Console.Write(reasoning.Text); // Model's reasoning tokens
            break;

        case StreamingAgentMessageTaskStart taskStart:
            Console.WriteLine($"\nStarting: {taskStart.Key}");
            break;

        case StreamingAgentMessageTaskStop taskStop:
            Console.WriteLine($"Completed in {taskStop.Duration.TotalMilliseconds}ms");
            break;

        case StreamingAgentMessageStop stop:
            Console.WriteLine("\n\nGeneration complete!");
            if (stop.Result?.CompletionUsage != null)
            {
                Console.WriteLine($"Input tokens: {stop.Result.CompletionUsage.PromptTokens}");
                Console.WriteLine($"Output tokens: {stop.Result.CompletionUsage.CompletionTokens}");
            }
            break;

        case StreamingAgentMessageError error:
            Console.WriteLine($"Error: {error.Message}");
            break;
    }
}
```

### Proxy Execution Options

The following options can be passed when executing a proxy agent via `ProxyExecutionReq`:

```csharp
using SerenityStar.Client;
using SerenityStar.Models.AIProxy;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

Proxy proxy = client.GetAgents().AIProxies.Create(
    "proxy-agent",
    new ProxyExecutionReq
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
        UseVision = false,           // Enable/disable vision features

        // Skills available to the model (see Skill Approvals)
        Skills = new ProxySkillsOptions
        {
            WebSearch = new ProxySkillsOptions.WebSearchSkillOptions(),
            ImageGeneration = new ProxySkillsOptions.ImageGenerationSkillOptions { RequiresApproval = true },
            UserChoice = true        // Let the model ask the user to choose (see Request User Choice)
        }
    }
);

AgentResult response = await proxy.ExecuteAsync();
```

## Chat Completions

Chat Completions provide a simplified interface for single-turn or multi-turn conversations without the overhead of managing conversation instances.

### Execute a chat completion

```csharp
using SerenityStar.Client;
using SerenityStar.Models.ChatCompletion;
using SerenityStar.Models.Execute;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create and execute chat completion (basic example)
ChatCompletion chatCompletion = client.GetAgents().ChatCompletions.Create("AgentCreator", new ChatCompletionReq
{
    Message = "Hello!!!"
});

AgentResult response = await chatCompletion.ExecuteAsync();

Console.WriteLine(response.Content); // AI-generated response
Console.WriteLine($"Completion Usage: {response.CompletionUsage?.TotalTokens}"); // { completion_tokens: 200, prompt_tokens: 30, total_tokens: 230 }

// Create and execute chat completion (advanced example with specific version)
ChatCompletion chatCompletionAdvanced = client.GetAgents().ChatCompletions.Create("Health-Coach", 2, new ChatCompletionReq
{
    UserIdentifier = "user-123",
    Channel = "web",
    VolatileKnowledgeIds = new List<string> { "knowledge-1", "knowledge-2" },
    Message = "Hi! How can I eat healthier?",
    Messages = new List<ChatCompletionMessage>
    {
        new ChatCompletionMessage
        {
            Role = "assistant",
            Content = "Hi there! How can I assist you?"
        }
    }
});

AgentResult responseAdvanced = await chatCompletionAdvanced.ExecuteAsync();

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
ChatCompletion chatCompletion = client.GetAgents().ChatCompletions.Create(
    "assistant-chat",
    new ChatCompletionReq
    {
        Message = "Explain quantum computing in simple terms",
        UserIdentifier = "user-123"
    }
);

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

        case StreamingAgentMessageReasoning reasoning:
            Console.Write(reasoning.Text); // Model's reasoning tokens
            break;

        case StreamingAgentMessageTaskStart taskStart:
            Console.WriteLine($"\n[Processing: {taskStart.TaskKey}]");
            break;

        case StreamingAgentMessageTaskStop taskStop:
            Console.WriteLine($"[Task completed in {taskStop.Duration.TotalMilliseconds}ms]");
            break;

        case StreamingAgentMessageStop stop:
            Console.WriteLine("\n\n[Response complete]");
            if (stop.Result?.CompletionUsage != null)
            {
                Console.WriteLine($"Tokens - Input: {stop.Result.CompletionUsage.PromptTokens}, Output: {stop.Result.CompletionUsage.CompletionTokens}");
            }
            if (stop.Result?.TimeToFirstToken.HasValue == true)
            {
                Console.WriteLine($"Time to first token: {stop.Result.TimeToFirstToken}ms");
            }
            break;

        case StreamingAgentMessageError error:
            Console.WriteLine($"[Error: {error.Message}]");
            break;
    }
}
```

## Audio Input for Agents

Agents that have audio input support enabled can receive audio file streams. The SDK handles the file upload internally and passes the resulting file ID to the agent, which automatically transcribes the audio and uses the result as input.

### Send audio to a conversational agent

For assistants and copilots, use the audio overloads of `SendMessageAsync` or `StreamMessageAsync` by passing a `Stream` and the file name. The SDK uploads the file, and the agent transcribes the audio internally and processes the transcription as the user message.

```csharp
using SerenityStar.Client;
using SerenityStar.Agents.Conversational;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create a conversation with an audio-enabled agent
Conversation conversation = client.GetAgents().Assistants.CreateConversation("voice-assistant");

// Send audio — uses the audio overload (separate from the text overload)
using FileStream audioStream = File.OpenRead("recording.wav");
AgentResult response = await conversation.SendMessageAsync(audioStream, "recording.wav");

Console.WriteLine(response.Content); // Agent's response based on the transcribed audio
Console.WriteLine($"Conversation ID: {conversation.ConversationId}");
```

### Stream audio message responses

You can also stream the response when sending audio to a conversational agent.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.Streaming;
using SerenityStar.Agents.Conversational;

SerenityClient client = SerenityClient.Create("your-api-key");

Conversation conversation = client.GetAgents().Assistants.CreateConversation("voice-assistant");

using FileStream audioStream = File.OpenRead("recording.wav");

await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync(audioStream, "recording.wav"))
{
    switch (message)
    {
        case StreamingAgentMessageContent content:
            Console.Write(content.Text);
            break;

        case StreamingAgentMessageStop stop:
            Console.WriteLine($"\nConversation ID: {stop.Result?.InstanceId}");
            break;
    }
}
```

### Send audio input to an activity agent

For activity agents, set the `AudioFileStream` and `AudioFileName` properties in the execution options. The SDK uploads the file, and the agent transcribes the audio and uses it as input.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.Execute;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

// Create an activity with audio input
using FileStream audioStream = File.OpenRead("recording.wav");
Activity activity = client.GetAgents().Activities.Create(
    "audio-analyzer",
    new AgentExecutionReq
    {
        AudioFileStream = audioStream,
        AudioFileName = "recording.wav",
        InputParameters = new Dictionary<string, object>
        {
            ["analysisType"] = "sentiment"
        }
    }
);

AgentResult result = await activity.ExecuteAsync();
Console.WriteLine(result.Content);
```

### Send audio input to a chat completion agent

Chat completion agents can also receive audio input via the `AudioFileStream` and `AudioFileName` properties.

> **Note:** `Message` and `AudioFileStream` are mutually exclusive. Provide one or the other, but not both. Passing both (or neither) will throw an `ArgumentException`.

```csharp
using SerenityStar.Client;
using SerenityStar.Models.ChatCompletion;
using SerenityStar.Agents.System;

SerenityClient client = SerenityClient.Create("your-api-key");

using FileStream audioStream = File.OpenRead("recording.wav");
ChatCompletion chatCompletion = client.GetAgents().ChatCompletions.Create(
    "voice-chat",
    new ChatCompletionReq
    {
        AudioFileStream = audioStream,
        AudioFileName = "recording.wav"
    }
);

AgentResult result = await chatCompletion.ExecuteAsync();
Console.WriteLine(result.Content);
```

## Volatile Knowledge

Volatile knowledge allows you to upload temporary documents that can be used in agent executions. These documents are processed and made available for a limited time based on your configuration. The volatile knowledge is scoped to the conversation or activity it's uploaded to and is automatically included in the next message.

### Upload and Use Volatile Knowledge in a Conversation

```csharp
using SerenityStar.Client;
using SerenityStar.Agents.Conversational;
using SerenityStar.Models.VolatileKnowledge;

SerenityClient client = SerenityClient.Create("your-api-key");

// Step 1: Create a conversation
Conversation conversation = client.GetAgents().Assistants.CreateConversation("chef-assistant");

// Step 2: Upload volatile knowledge to the conversation
using FileStream fileStream = File.OpenRead("document.pdf");
UploadVolatileKnowledgeReq uploadRequest = new()
{
    FileStream = fileStream,
    FileName = "document.pdf"
};

VolatileKnowledgeRes knowledge = await conversation.VolatileKnowledge.UploadAsync(uploadRequest);
Console.WriteLine($"Uploaded knowledge ID: {knowledge.Id}, Status: {knowledge.Status}");

// Step 3: Check status until ready
VolatileKnowledgeRes response = await conversation.VolatileKnowledge.GetStatusAsync(knowledge.Id);

// Wait until the knowledge is ready
while (response.Status != "success" && response.Status != "error")
{
    await Task.Delay(1000);
    response = await conversation.VolatileKnowledge.GetStatusAsync(knowledge.Id);
}

if (response.Status == "error")
    Console.WriteLine($"Processing failed: {response.Error}");
else
{
    // Step 4: Send a message - the volatile knowledge is automatically included
    AgentResult result = await conversation.SendMessageAsync("What does the document say about cooking techniques?");
    Console.WriteLine(result.Content);

    // The volatile knowledge is automatically cleared after sending the message
    // Subsequent messages won't include it unless you upload new knowledge
}
```

### Upload Volatile Knowledge with Text Content

```csharp
Conversation conversation = client.GetAgents().Assistants.CreateConversation("research-assistant");

UploadVolatileKnowledgeReq uploadRequest = new()
{
    Content = "https://serenitystar.ai"
};

VolatileKnowledgeRes knowledge = await conversation.VolatileKnowledge.UploadAsync(uploadRequest);
Console.WriteLine($"Uploaded knowledge ID: {knowledge.Id}, Status: {knowledge.Status}");
```

### Upload with Custom Parameters

```csharp
Conversation conversation = client.GetAgents().Assistants.CreateConversation("data-analyst");

using FileStream fileStream = File.OpenRead("document.pdf");
UploadVolatileKnowledgeReq uploadRequest = new()
{
    FileStream = fileStream,
    FileName = "document.pdf",
    CallbackUrl = "https://your-app.com/knowledge-callback"
};

VolatileKnowledgeRes knowledge = await conversation.VolatileKnowledge.UploadAsync(
    uploadRequest,
    processEmbeddings: true,         // Process embeddings for semantic search
    noExpiration: false,             // Allow expiration
    expirationDays: 7);              // Expire in 7 days

Console.WriteLine($"Uploaded knowledge ID: {knowledge.Id}");
if (knowledge.ExpirationDate.HasValue)
    Console.WriteLine($"Expires on: {knowledge.ExpirationDate.Value}");
```

### Use Volatile Knowledge with Activities

```csharp
using SerenityStar.Agents.System;

// Create an activity
Activity activity = client.GetAgents().Activities.Create("document-analyzer");

// Upload volatile knowledge to the activity
using FileStream fileStream = File.OpenRead("report.pdf");
UploadVolatileKnowledgeReq uploadRequest = new()
{
    FileStream = fileStream,
    FileName = "report.pdf"
};

VolatileKnowledgeRes knowledge = await activity.VolatileKnowledge.UploadAsync(uploadRequest);

// Wait for processing
VolatileKnowledgeRes response = await activity.VolatileKnowledge.GetStatusAsync(knowledge.Id);
while (response.Status != "success" && response.Status != "error")
{
    await Task.Delay(1000);
    response = await activity.VolatileKnowledge.GetStatusAsync(knowledge.Id);
}

if (response.Status == "success")
{
    // Execute the activity - volatile knowledge is automatically included
    AgentResult result = await activity.ExecuteAsync();
    Console.WriteLine(result.Content);
    // Volatile knowledge is cleared after execution
}
```

### Multiple Knowledge Documents

You can upload multiple documents to the same conversation or activity. All uploaded documents will be included in the next message and then automatically cleared.

```csharp
Conversation conversation = client.GetAgents().Assistants.CreateConversation("multi-doc-assistant");

// Upload first document
using (FileStream file1 = File.OpenRead("doc1.pdf"))
{
    VolatileKnowledgeRes knowledge1 = await conversation.VolatileKnowledge.UploadAsync(new()
    {
        FileStream = file1,
        FileName = "doc1.pdf"
    });

    // Wait for processing
    while ((await conversation.VolatileKnowledge.GetStatusAsync(knowledge1.Id)).Status == "analyzing")
        await Task.Delay(1000);
}

// Upload second document
using (FileStream file2 = File.OpenRead("doc2.pdf"))
{
    VolatileKnowledgeRes knowledge2 = await conversation.VolatileKnowledge.UploadAsync(new()
    {
        FileStream = file2,
        FileName = "doc2.pdf"
    });

    // Wait for processing
    while ((await conversation.VolatileKnowledge.GetStatusAsync(knowledge2.Id)).Status == "analyzing")
        await Task.Delay(1000);
}

// Both documents are included in this message
AgentResult result = await conversation.SendMessageAsync("What can you say me about doc1 and doc2?");
Console.WriteLine(result.Content);

// Both documents are cleared after the message is sent
```

## AI Services

AI services are available in the `SubgenAI.SerenityStar.AIServices` package, which does not require the agents package:

```bash
dotnet add package SubgenAI.SerenityStar.AIServices
```

Each AI service has its own interface, obtained from `SerenityClient`. The interfaces live in the `SerenityStar.AIServices` namespace, so they can be injected (see [Using Dependency Injection](#using-dependency-injection)) and mocked in tests.

| Service | Interface | Entry point |
| --- | --- | --- |
| [Anonymizer](#anonymizer) | `IAnonymizerClient` | `client.GetAnonymizerClient()` |
| [Decisions](#decisions) | `IDecisionClient` | `client.GetDecisionClient()` |
| [Embeddings](#embeddings) | `IEmbeddingClient` | `client.GetEmbeddingClient()` |
| [Image Generation](#image-generation) | `IImageGenerationClient` | `client.GetImageGenerationClient()` |
| [OCR](#ocr) | `IOcrClient` | `client.GetOcrClient()` |
| [Speech Generation](#speech-generation) | `ISpeechGenerationClient` | `client.GetSpeechGenerationClient()` |
| [Transcription](#transcription) | `ITranscriptionClient` | `client.GetTranscriptionClient()` |

Services that work with files accept the file directly (stream + file name) or the id of a file previously uploaded to Serenity Star (`*ByFileIdAsync` methods). Every result includes the instance id (when the service creates one), usage and cost.

### Anonymizer

Detect and obfuscate sensitive data (PII) in a text. Each detected entity is replaced with a placeholder.

```csharp
using SerenityStar.AIServices;
using SerenityStar.Models.Anonymizer;

IAnonymizerClient anonymizer = client.GetAnonymizerClient();

AnonymizerResult result = await anonymizer.ObfuscateAsync(new AnonymizerReq
{
    Inputs = "My name is John Smith and my email is john@example.com",
    Categories = new List<string> { "private_person", "private_email" },  // Optional: all categories when empty
    CategoryOptions = new List<AnonymizerCategoryOption>
    {
        new() { Category = "private_person", Key = "Person", Threshold = 0.8 }  // Optional
    }
});

Console.WriteLine(result.Text);  // "My name is Person-1 and my email is ..."
foreach (AnonymizerSpan span in result.Spans)
    Console.WriteLine($"{span.EntityGroup}: '{span.Word}' -> {span.Placeholder}");
```

> **Note:** The detected spans include the original sensitive values (`Word`). Avoid logging them.

### Decisions

Evaluate an input against one or more questions: predicates (yes/no), choices (closed set of options) or scores (ordered levels). A model is required: set `AIModelId`, or both `ModelCode` and `Vendor`.

```csharp
using SerenityStar.AIServices;
using SerenityStar.Models.Decisions;

IDecisionClient decisions = client.GetDecisionClient();

DecisionResult result = await decisions.EvaluateAsync(new DecisionReq
{
    AIModelId = Guid.Parse("your-decision-model-id"),
    Input = "My invoice was charged twice and nobody answers my emails!",
    Questions =
    {
        new DecisionQuestion { Type = DecisionQuestionType.Predicate, Name = "is_complaint", Instructions = "Is the customer complaining?" },
        new DecisionQuestion
        {
            Type = DecisionQuestionType.Choice, Name = "topic", Instructions = "What is the message about?",
            Choices = new List<DecisionChoice> { new() { Value = "billing" }, new() { Value = "support" }, new() { Value = "other" } }
        },
        new DecisionQuestion
        {
            Type = DecisionQuestionType.Score, Name = "urgency", Instructions = "How urgent is it?",
            Levels = new List<DecisionScoreLevel> { new() { Label = "low" }, new() { Label = "medium" }, new() { Label = "high" } }
        }
    }
});

foreach (DecisionAnswer answer in result.Answers)
{
    switch (answer)
    {
        case PredicateDecisionAnswer predicate:
            Console.WriteLine($"{predicate.Name}: {predicate.Probability:P0}");
            break;
        case ChoiceDecisionAnswer choice:
            Console.WriteLine($"{choice.Name}: {choice.Choice} (confidence {choice.Confidence:P0})");
            break;
        case ScoreDecisionAnswer score:
            Console.WriteLine($"{score.Name}: {score.Score}");
            break;
        case RefusalDecisionAnswer refusal:
            Console.WriteLine($"{refusal.Name}: the model declined to answer");
            break;
    }
}
```

The input can also be an ordered list of messages, function calls and function call outputs, such as an agent trace, to evaluate how an agent behaved. User messages and function call outputs can include images, sent as base64 data URLs (HTTP(S) image URLs are not supported). Function calls are evidence only: they are never executed.

```csharp
DecisionResult traceResult = await decisions.EvaluateAsync(new DecisionReq
{
    ModelCode = "your-decision-model-code",
    Vendor = "OpenAI",
    Input = DecisionInput.FromItems(
        DecisionInputItem.Message(DecisionMessageRole.User, DecisionContent.FromParts(
            DecisionInputPart.InputText("This is the product I received. Where is my order 42?"),
            DecisionInputPart.InputImage(File.ReadAllBytes("product.jpg"), "image/jpeg"))),
        DecisionInputItem.FunctionCall("call_1", "get_order", """{"orderId":42}"""),
        DecisionInputItem.FunctionCallOutput("call_1", """{"orderId":42,"status":"shipped"}"""),
        DecisionInputItem.Message(DecisionMessageRole.Assistant, "Your order 42 has been shipped.")),
    Questions =
    {
        new DecisionQuestion
        {
            Type = DecisionQuestionType.Predicate, Name = "grounded",
            Instructions = "Is the assistant's answer supported by the tool output?"
        }
    }
});
```

### Embeddings

Generate vector embeddings for one or more texts.

```csharp
using SerenityStar.AIServices;
using SerenityStar.Models.Embeddings;

IEmbeddingClient embeddings = client.GetEmbeddingClient();

EmbeddingsResult result = await embeddings.GenerateAsync(new GenerateEmbeddingsReq
{
    Content = { "First text", "Second text" }
});

foreach (EmbeddingData item in result.Data)
    Console.WriteLine($"#{item.Index}: {item.Embedding.Length} dimensions");
```

### Image Generation

Generate images from a text prompt, optionally based on source images (e.g. to edit one image or combine several). Source images can be uploaded directly or referenced by the id of a file previously uploaded to Serenity Star. Supported formats: jpg, jpeg, png, gif, webp. Maximum size per file: 20 MB. Up to 16 source images per request; only models that accept source images support them, each up to its own number of images.

```csharp
using SerenityStar.AIServices;
using SerenityStar.Models.AIServices;
using SerenityStar.Models.ImageGeneration;

IImageGenerationClient images = client.GetImageGenerationClient();

ImageGenerationResult result = await images.GenerateAsync(new GenerateImageReq
{
    Prompt = "A watercolor painting of a lighthouse at sunset"
});
Console.WriteLine(result.ImageUrl);

// Based on source images uploaded directly (the streams are disposed when the request completes)
using FileStream photo = File.OpenRead("photo.png");
ImageGenerationResult edited = await images.GenerateWithSourceImagesAsync(new GenerateImageWithSourceImagesReq
{
    Prompt = "Make it look like a night scene",
    Files = { new AIServiceFile { FileStream = photo, FileName = "photo.png" } }
});

// Or based on source images previously uploaded to Serenity Star
ImageGenerationResult editedById = await images.GenerateByFileIdAsync(new GenerateImageByFileIdReq
{
    Prompt = "Make it look like a night scene",
    FileIds = { Guid.Parse("uploaded-file-id") }
});
```

### OCR

Extract text, tables and images from documents. Supported formats: png, jpg, jpeg, pdf. Maximum size per file: 50 MB. Documents can be uploaded directly, sent as base64, referenced by URL, or by the id of a file previously uploaded to Serenity Star. A failure on one document does not abort the others.

```csharp
using SerenityStar.AIServices;
using SerenityStar.Models.AIServices;
using SerenityStar.Models.Ocr;

IOcrClient ocr = client.GetOcrClient();

using FileStream invoice = File.OpenRead("invoice.pdf");
OcrExecutionResult result = await ocr.ExtractAsync(new OcrReq
{
    Files = { new AIServiceFile { FileStream = invoice, FileName = "invoice.pdf" } },
    Instructions = "Keep the table layout"   // Optional
});

foreach (OcrFileResult file in result.Files)
{
    if (file.Success)
        Console.WriteLine(file.Result!.Markdown);
    else
        Console.WriteLine($"{file.Source} failed: {file.Error}");
}

// Other sources
await ocr.ExtractFromUrlsAsync(new OcrUrlReq { Urls = { "https://example.com/contract.pdf" } });
await ocr.ExtractFromBase64Async(new OcrBase64Req
{
    Base64Files = { new OcrBase64File { ContentBase64 = base64Content, MimeType = "application/pdf" } }
});
await ocr.ExtractByFileIdAsync(new OcrByFileIdReq { FileIds = { Guid.Parse("uploaded-file-id") } });
```

### Speech Generation

Generate speech (text-to-speech) from text. A model is required: set `AIModelId`, or both `ModelCode` and `Vendor`.

```csharp
using SerenityStar.AIServices;
using SerenityStar.Models.SpeechGeneration;

ISpeechGenerationClient speech = client.GetSpeechGenerationClient();

GenerateSpeechReq request = new()
{
    ModelCode = "tts-1",
    Vendor = "OpenAI",
    Text = "Welcome to Serenity Star!",
    Voice = "alloy",   // Optional: vendor default voice when not set
    Speed = 1.2f       // Optional
};

// Get a public URL to the generated audio
SpeechGenerationResult result = await speech.GenerateAsync(request);
Console.WriteLine(result.AudioUrl);

// Or stream the audio in chunks as it is generated
using FileStream output = File.Create("welcome.mp3");
await foreach (StreamingSpeechGenerationMessage message in speech.StreamAsync(request))
{
    switch (message)
    {
        case StreamingSpeechGenerationMessageStart start:
            Console.WriteLine($"Format: {start.MimeType}");
            break;
        case StreamingSpeechGenerationMessageAudio audio:
            byte[] bytes = audio.GetAudioBytes();
            await output.WriteAsync(bytes, 0, bytes.Length);
            break;
        case StreamingSpeechGenerationMessageStop stop:
            Console.WriteLine($"Done: {stop.AudioUrl}");
            break;
        case StreamingSpeechGenerationMessageError error:
            Console.WriteLine($"Error: {error.Message}");
            break;
    }
}
```

### Transcription

Convert audio and video files into text. Supported formats: mp3, mp4, mpeg, mpga, m4a, wav, webm. Maximum file size: 25 MB.

> To send audio to an agent instead, see [Audio Input for Agents](#audio-input-for-agents).

```csharp
using SerenityStar.AIServices;
using SerenityStar.Client;
using SerenityStar.Models.Transcription;

SerenityClient client = SerenityClient.Create("your-api-key");
ITranscriptionClient transcription = client.GetTranscriptionClient();

using FileStream audioFile = File.OpenRead("interview.wav");
TranscribeResult result = await transcription.TranscribeAsync(new TranscribeAudioReq
{
    FileStream = audioFile,
    FileName = "interview.wav",
    ModelId = Guid.Parse("your-model-id"),   // Optional: specific AI model
    Prompt = "Interview about artificial intelligence and machine learning", // Optional: guide terminology
    UserIdentifier = "user-123",             // Optional: for tracking
    Channel = "web-app"                      // Optional: execution channel
});

Console.WriteLine(result.Transcript);
Console.WriteLine($"Language: {result.Metadata?.Language}, duration: {result.Metadata?.Duration}");
Console.WriteLine($"Cost: {result.Cost?.Total} {result.Cost?.Currency}");

// Or transcribe a file previously uploaded to Serenity Star
TranscribeResult byId = await transcription.TranscribeByFileIdAsync(new TranscribeAudioByFileIdReq
{
    FileId = Guid.Parse("uploaded-file-id")
});
```

## Error handling

When the API returns an unsuccessful HTTP response, the SDK throws a `SerenityApiException`. It derives from `HttpRequestException`, so existing `catch (HttpRequestException)` handlers keep working, and it also exposes the response details:

- `StatusCode` — the `HttpStatusCode` returned by the API.
- `ReasonPhrase` — the HTTP reason phrase, if any.
- `ResponseJson` — the parsed JSON error payload as a `System.Text.Json.JsonElement?`. The API returns JSON error bodies for all responses except HTTP 429 (rate limiting), so this is `null` when the body was missing or not valid JSON. Use `JsonElement.GetRawText()` to read the raw text.

```csharp
using SerenityStar.Exceptions;

try
{
    var result = await client.GetAgents().Activities
        .Create("marketing-campaign")
        .ExecuteAsync();
}
catch (SerenityApiException ex)
{
    Console.WriteLine($"Request failed ({(int)ex.StatusCode} {ex.ReasonPhrase})");
    if (ex.ResponseJson is not null)
        Console.WriteLine(ex.ResponseJson.Value.GetRawText());
}
```

## Migrating to 3.0

Namespaces and models are unchanged, so most code only needs the new entry points:

| Before | After |
| --- | --- |
| `client.Agents.Activities.Create(...)` | `client.GetAgents().Activities.Create(...)` |
| `client.AIServices.Transcription.TranscribeAsync(...)` | `client.GetTranscriptionClient().TranscribeAsync(...)` (install `SubgenAI.SerenityStar.AIServices`) |
| Inject `ISerenityClient` | Inject `SerenityClient`, or `AgentsScope` / an AI service interface after `AddSerenityStarAgents()` / `AddSerenityStarAIServices()` |
| `services.AddHttpClient<ISerenityClient, SerenityClient>()` | `services.AddHttpClient<SerenityClient>()` |

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the [MIT License](LICENSE).