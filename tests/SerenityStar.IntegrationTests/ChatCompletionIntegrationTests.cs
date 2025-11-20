using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Streaming;
using SerenityStar.Models.ChatCompletion;
using SerenityStar.Agents.System;
using Xunit;

namespace SerenityStar.IntegrationTests;

public class ChatCompletionIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ISerenityClient _client;

    public ChatCompletionIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISerenityClient>();
    }

    [Fact]
    public async Task ExecuteAsync_WithSimpleMessage_ShouldSucceed()
    {
        // Arrange
        ChatCompletionReq options = new()
        {
            Message = "Hello, how are you?"
        };

        ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);

        // Act
        AgentResult result = await chatCompletion.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
    }

    [Fact]
    public async Task ExecuteAsync_WithMessageHistory_ShouldSucceed()
    {
        // Arrange
        ChatCompletionReq options = new()
        {
            Message = "What was my first question?",
            Messages =
            [
                new() { Role = "user", Content = "What is the capital of France?" },
                new() { Role = "assistant", Content = "The capital of France is Paris." }
            ]
        };

        ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);

        // Act
        AgentResult result = await chatCompletion.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task ExecuteAsync_WithUserIdentifier_ShouldSucceed()
    {
        // Arrange
        ChatCompletionReq options = new()
        {
            Message = "Tell me something interesting",
            UserIdentifier = "test-user-123"
        };

        ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);

        // Act
        AgentResult result = await chatCompletion.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
    }

    [Fact]
    public async Task ExecuteAsync_WithInputParameters_ShouldSucceed()
    {
        // Arrange
        ChatCompletionReq options = new()
        {
            Message = "What is machine learning?",
            InputParameters = new Dictionary<string, object>
            {
                ["tone"] = "formal",
                ["length"] = "short"
            }
        };

        ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);

        // Act
        AgentResult result = await chatCompletion.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task ExecuteAsync_WithAgentVersion_ShouldSucceed()
    {
        // Arrange
        ChatCompletionReq options = new()
        {
            Message = "Hello",
            AgentVersion = 1
        };

        ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);

        // Act
        AgentResult result = await chatCompletion.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexConversation_ShouldSucceed()
    {
        // Arrange
        ChatCompletionReq options = new()
        {
            Message = "Based on our conversation, what would you recommend?",
            Messages =
            [
                new() { Role = "user", Content = "I want to learn programming" },
                new() { Role = "assistant", Content = "I'd recommend starting with Python or JavaScript." },
                new() { Role = "user", Content = "Which one is easier?" },
                new() { Role = "assistant", Content = "Python is generally considered easier for beginners." }
            ],
            UserIdentifier = "learner-001"
        };

        ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);

        // Act
        AgentResult result = await chatCompletion.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task StreamAsync_WithSimpleMessage_ShouldSucceed()
    {
        // Arrange
        ChatCompletionReq options = new()
        {
            Message = "Tell me a short story"
        };

        ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);
        List<StreamingAgentMessage> messages = [];

        // Act
        await foreach (StreamingAgentMessage message in chatCompletion.StreamAsync())
            messages.Add(message);

        // Assert
        Assert.NotEmpty(messages);
        Assert.Contains(messages, m => m is StreamingAgentMessageStart);
        Assert.Contains(messages, m => m is StreamingAgentMessageContent);
        Assert.Contains(messages, m => m is StreamingAgentMessageStop);
    }

    [Fact]
    public async Task StreamAsync_WithMessageHistory_ShouldSucceed()
    {
        // Arrange
        ChatCompletionReq options = new()
        {
            Message = "Continue the story",
            Messages =
            [
                new() { Role = "user", Content = "Tell me a story about a dragon" },
                new() { Role = "assistant", Content = "Once upon a time, there was a dragon..." }
            ]
        };

        ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);
        List<StreamingAgentMessage> messages = [];

        // Act
        await foreach (StreamingAgentMessage message in chatCompletion.StreamAsync())
            messages.Add(message);

        // Assert
        Assert.NotEmpty(messages);
        Assert.Contains(messages, m => m is StreamingAgentMessageStart);
        Assert.Contains(messages, m => m is StreamingAgentMessageStop);
    }

    [Fact]
    public async Task StreamAsync_ShouldHaveCompleteStreamSequence()
    {
        // Arrange
        ChatCompletionReq options = new()
        {
            Message = "What is cloud computing?"
        };

        ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);
        List<StreamingAgentMessage> messages = [];

        // Act
        await foreach (StreamingAgentMessage message in chatCompletion.StreamAsync())
            messages.Add(message);

        // Assert
        Assert.NotEmpty(messages);

        // Should start with StreamingAgentMessageStart
        Assert.IsType<StreamingAgentMessageStart>(messages[0]);

        // Should end with StreamingAgentMessageStop
        Assert.IsType<StreamingAgentMessageStop>(messages[^1]);

        // Should have content messages
        Assert.Contains(messages, m => m is StreamingAgentMessageContent);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleMessages_ShouldSucceed()
    {
        // Arrange
        string[] questions = ["What is AI?", "How does it work?", "What are the applications?"];
        List<string> responses = [];

        // Act
        foreach (string question in questions)
        {
            ChatCompletionReq options = new()
            {
                Message = question
            };

            ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);
            AgentResult result = await chatCompletion.ExecuteAsync();

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.NotEmpty(result.Content);

            responses.Add(result.Content);

            // Add delay to avoid rate limiting
            await Task.Delay(1000);
        }

        // Assert
        Assert.Equal(questions.Length, responses.Count);
        Assert.All(responses, Assert.NotNull);
    }

    [Fact]
    public async Task ExecuteAsync_WithAllParameters_ShouldSucceed()
    {
        // Arrange
        ChatCompletionReq options = new()
        {
            Message = "Explain quantum mechanics in simple terms",
            Messages =
            [
                new() { Role = "system", Content = "You are a physics teacher." },
                new() { Role = "user", Content = "I want to learn quantum mechanics" }
            ],
            UserIdentifier = "physics-student-001",
            InputParameters = new Dictionary<string, object>
            {
                ["complexity_level"] = "beginner",
                ["use_analogies"] = true
            },
            AgentVersion = 1
        };

        ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);

        // Act
        AgentResult result = await chatCompletion.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
    }

    [Fact]
    public async Task StreamAsync_WithInputParameters_ShouldSucceed()
    {
        // Arrange
        ChatCompletionReq options = new()
        {
            Message = "Write a poem about nature",
            InputParameters = new Dictionary<string, object>
            {
                ["style"] = "haiku"
            }
        };

        ChatCompletion chatCompletion = _client.Agents.ChatCompletions.Create(_fixture.ChatCompletionAgent, options);
        List<StreamingAgentMessage> messages = [];

        // Act
        await foreach (StreamingAgentMessage message in chatCompletion.StreamAsync())
            messages.Add(message);

        // Assert
        Assert.NotEmpty(messages);
        Assert.Contains(messages, m => m is StreamingAgentMessageContent);
    }
}
