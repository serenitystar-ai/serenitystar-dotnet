using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Conversation;
using SerenityStar.Agents.Conversational;
using Xunit;
using SerenityStar.Models.MessageFeedback;

namespace SerenityStar.IntegrationTests;

public class MessageFeedbackIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ISerenityClient _client;

    public MessageFeedbackIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISerenityClient>();
    }

    [Fact]
    public async Task SubmitFeedback_WithPositiveFeedback_ShouldSucceed()
    {
        // Arrange - Create a conversation and send a message to get an agent message ID
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        AgentResult result = await conversation.SendMessageAsync("Tell me a joke");

        Assert.NotNull(result.AgentMessageId);
        string agentMessageId = result.AgentMessageId?.ToString() ?? "invalid-id";

        // Act - Submit positive feedback
        SubmitFeedbackOptions feedbackOptions = new()
        {
            AgentMessageId = Guid.Parse(agentMessageId),
            Feedback = true // Positive feedback
        };

        await conversation.SubmitFeedbackAsync(feedbackOptions);

        // Assert - If no exception is thrown, the feedback was submitted successfully
        Assert.True(true);
    }

    [Fact]
    public async Task SubmitFeedback_WithNegativeFeedback_ShouldSucceed()
    {
        // Arrange - Create a conversation and send a message
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        AgentResult result = await conversation.SendMessageAsync("Explain quantum computing");

        Assert.NotNull(result.AgentMessageId);
        string agentMessageId = result.AgentMessageId?.ToString() ?? "invalid-id";

        // Act - Submit negative feedback
        SubmitFeedbackOptions feedbackOptions = new()
        {
            AgentMessageId = Guid.Parse(agentMessageId),
            Feedback = false // Negative feedback
        };

        await conversation.SubmitFeedbackAsync(feedbackOptions);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task RemoveFeedback_ShouldSucceed()
    {
        // Arrange - Create a conversation, send a message, and submit feedback
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        AgentResult result = await conversation.SendMessageAsync("What is AI?");

        Assert.NotNull(result.AgentMessageId);
        string agentMessageId = result.AgentMessageId?.ToString() ?? "invalid-id";

        // Submit feedback first
        SubmitFeedbackOptions feedbackOptions = new()
        {
            AgentMessageId = Guid.Parse(agentMessageId),
            Feedback = true
        };
        await conversation.SubmitFeedbackAsync(feedbackOptions);

        // Add delay to ensure feedback is recorded
        await Task.Delay(500);

        // Act - Remove the feedback
        RemoveFeedbackOptions removeOptions = new()
        {
            AgentMessageId = Guid.Parse(agentMessageId)
        };

        await conversation.RemoveFeedbackAsync(removeOptions);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task SubmitFeedback_MultipleTimes_ShouldSucceed()
    {
        // Arrange - Create a conversation
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        // Act - Send multiple messages and submit feedback for each
        string[] messages = ["Hello", "How are you?", "What can you do?"];
        var feedbackResults = new List<bool>();

        foreach (string message in messages)
        {
            AgentResult result = await conversation.SendMessageAsync(message);

            if (result.AgentMessageId != null)
            {
                SubmitFeedbackOptions feedbackOptions = new()
                {
                    AgentMessageId = Guid.Parse(result.AgentMessageId?.ToString() ?? "00000000-0000-0000-0000-000000000000"),
                    Feedback = true
                };

                await conversation.SubmitFeedbackAsync(feedbackOptions);
                feedbackResults.Add(true);
            }

            // Add delay to avoid rate limiting
            await Task.Delay(500);
        }

        // Assert
        Assert.Equal(messages.Length, feedbackResults.Count);
        Assert.All(feedbackResults, result => Assert.True(result));
    }

    [Fact]
    public async Task SubmitFeedback_WithInvalidAgentMessageId_ShouldFail()
    {
        // Arrange - Create a conversation and initialize it with a message
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        await conversation.SendMessageAsync("Initialize conversation");

        SubmitFeedbackOptions feedbackOptions = new()
        {
            AgentMessageId = Guid.Parse("00000000-0000-0000-0000-000000000000"),
            Feedback = true
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            conversation.SubmitFeedbackAsync(feedbackOptions));
    }

    [Fact]
    public async Task RemoveFeedback_WithInvalidAgentMessageId_ShouldFail()
    {
        // Arrange - Create a conversation and initialize it with a message
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        await conversation.SendMessageAsync("Initialize conversation");

        RemoveFeedbackOptions removeOptions = new()
        {
            AgentMessageId = Guid.Empty
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            conversation.RemoveFeedbackAsync(removeOptions));
    }

    [Fact]
    public async Task SubmitFeedback_AlternateFeedback_ShouldSucceed()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        // Act - Send messages and alternate between positive and negative feedback
        bool[] feedbackValues = [true, false, true, false];
        var submittedFeedback = new List<bool>();

        for (int i = 0; i < feedbackValues.Length; i++)
        {
            AgentResult result = await conversation.SendMessageAsync($"Question {i + 1}");

            if (result.AgentMessageId != null)
            {
                SubmitFeedbackOptions feedbackOptions = new()
                {
                    AgentMessageId = Guid.Parse(result.AgentMessageId?.ToString() ?? "invalid-id"),
                    Feedback = feedbackValues[i]
                };

                await conversation.SubmitFeedbackAsync(feedbackOptions);
                submittedFeedback.Add(feedbackValues[i]);
            }

            // Add delay to avoid rate limiting
            await Task.Delay(500);
        }

        // Assert
        Assert.Equal(feedbackValues.Length, submittedFeedback.Count);
    }

    [Fact]
    public async Task SubmitAndRemoveFeedback_ShouldSucceed()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        AgentResult result = await conversation.SendMessageAsync("Test message");

        Assert.NotNull(result.AgentMessageId);

        // Act - Submit feedback
        SubmitFeedbackOptions submitOptions = new()
        {
            AgentMessageId = Guid.Parse(result.AgentMessageId?.ToString() ?? "invalid-id"),
            Feedback = true
        };
        await conversation.SubmitFeedbackAsync(submitOptions);

        // Add delay
        await Task.Delay(500);

        // Remove feedback
        RemoveFeedbackOptions removeOptions = new()
        {
            AgentMessageId = Guid.Parse(result.AgentMessageId?.ToString() ?? "invalid-id")
        };
        await conversation.RemoveFeedbackAsync(removeOptions);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task SubmitFeedback_WithDifferentMessages_ShouldSucceed()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        // Act - Send different types of messages and submit feedback
        string[] messageTypes = ["technical question", "creative request", "general inquiry"];
        var feedbackCount = 0;

        foreach (string messageType in messageTypes)
        {
            AgentResult result = await conversation.SendMessageAsync($"This is a {messageType}");

            if (result.AgentMessageId != null)
            {
                SubmitFeedbackOptions feedbackOptions = new()
                {
                    AgentMessageId = Guid.Parse(result.AgentMessageId?.ToString() ?? "00000000-0000-0000-0000-000000000000"),
                    Feedback = true
                };

                await conversation.SubmitFeedbackAsync(feedbackOptions);
                feedbackCount++;
            }

            // Add delay to avoid rate limiting
            await Task.Delay(500);
        }

        // Assert
        Assert.Equal(messageTypes.Length, feedbackCount);
    }
}
