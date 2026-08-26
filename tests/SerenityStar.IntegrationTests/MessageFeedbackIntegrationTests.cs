using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Models.Execute;
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
        SubmitFeedbackReq feedbackOptions = new()
        {
            AgentMessageId = Guid.Parse(agentMessageId),
            Feedback = true // Positive feedback
        };

        await conversation.SubmitFeedbackAsync(feedbackOptions);

        // Assert - If no exception is thrown, the feedback was submitted successfully
        Assert.True(true);
    }

    [Fact]
    public async Task SubmitFeedback_WithComment_ShouldSucceed()
    {
        // Arrange - Create a conversation and send a message
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        AgentResult result = await conversation.SendMessageAsync("Give me a recipe for parmesan chicken");

        Assert.NotNull(result.AgentMessageId);

        // Act - Submit negative feedback with a comment explaining why
        SubmitFeedbackReq feedbackOptions = new()
        {
            AgentMessageId = result.AgentMessageId!.Value,
            Feedback = false,
            Comment = "The recipe skipped the cooking temperature."
        };

        await conversation.SubmitFeedbackAsync(feedbackOptions);

        // Assert - If no exception is thrown, the feedback was submitted successfully
        Assert.True(true);
    }

    [Fact]
    public async Task SubmitFeedback_WithCommentAtMaxLength_ShouldSucceed()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        AgentResult result = await conversation.SendMessageAsync("What is AI?");

        Assert.NotNull(result.AgentMessageId);

        // Act - The API accepts comments of up to 1000 characters
        SubmitFeedbackReq feedbackOptions = new()
        {
            AgentMessageId = result.AgentMessageId!.Value,
            Feedback = true,
            Comment = new string('a', 1000)
        };

        await conversation.SubmitFeedbackAsync(feedbackOptions);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task SubmitFeedback_WithCommentExceedingMaxLength_ShouldFail()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        AgentResult result = await conversation.SendMessageAsync("What is AI?");

        Assert.NotNull(result.AgentMessageId);

        // The SDK does not validate the length locally, the API answers with a 400
        SubmitFeedbackReq feedbackOptions = new()
        {
            AgentMessageId = result.AgentMessageId!.Value,
            Feedback = true,
            Comment = new string('a', 1001)
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            conversation.SubmitFeedbackAsync(feedbackOptions));
    }

    [Fact]
    public async Task SubmitFeedback_WithWhitespaceComment_ShouldSucceed()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        AgentResult result = await conversation.SendMessageAsync("Tell me a fun fact");

        Assert.NotNull(result.AgentMessageId);

        // Act - A blank comment is treated as no comment at all
        SubmitFeedbackReq feedbackOptions = new()
        {
            AgentMessageId = result.AgentMessageId!.Value,
            Feedback = true,
            Comment = "   "
        };

        await conversation.SubmitFeedbackAsync(feedbackOptions);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task SubmitFeedback_ResubmittedWithoutComment_ShouldClearTheComment()
    {
        // Arrange - Submit feedback with a comment
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        AgentResult result = await conversation.SendMessageAsync("Explain quantum computing");

        Assert.NotNull(result.AgentMessageId);
        Guid agentMessageId = result.AgentMessageId!.Value;

        await conversation.SubmitFeedbackAsync(new SubmitFeedbackReq
        {
            AgentMessageId = agentMessageId,
            Feedback = false,
            Comment = "Too technical for me."
        });

        await Task.Delay(500);

        // Act - Re-submit without a comment, which overwrites the stored one
        await conversation.SubmitFeedbackAsync(new SubmitFeedbackReq
        {
            AgentMessageId = agentMessageId,
            Feedback = true
        });

        await Task.Delay(500);

        // Assert - Reading it back shows the comment was cleared
        MessageFeedbackPage page = await _client.Agents.Assistants.GetMessageFeedbackAsync(
            _fixture.AssistantAgent,
            new GetMessageFeedbackReq { PageSize = 100 });

        MessageFeedbackRes? stored = page.Items.FirstOrDefault(f => f.AgentMessageId == agentMessageId);

        Assert.NotNull(stored);
        Assert.True(stored!.Feedback);
        Assert.True(string.IsNullOrEmpty(stored.Comment));
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
        SubmitFeedbackReq feedbackOptions = new()
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
        SubmitFeedbackReq feedbackOptions = new()
        {
            AgentMessageId = Guid.Parse(agentMessageId),
            Feedback = true
        };
        await conversation.SubmitFeedbackAsync(feedbackOptions);

        // Add delay to ensure feedback is recorded
        await Task.Delay(500);

        // Act - Remove the feedback
        RemoveFeedbackReq removeOptions = new()
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
                SubmitFeedbackReq feedbackOptions = new()
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

        SubmitFeedbackReq feedbackOptions = new()
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

        RemoveFeedbackReq removeOptions = new()
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
                SubmitFeedbackReq feedbackOptions = new()
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
        SubmitFeedbackReq submitOptions = new()
        {
            AgentMessageId = Guid.Parse(result.AgentMessageId?.ToString() ?? "invalid-id"),
            Feedback = true
        };
        await conversation.SubmitFeedbackAsync(submitOptions);

        // Add delay
        await Task.Delay(500);

        // Remove feedback
        RemoveFeedbackReq removeOptions = new()
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
                SubmitFeedbackReq feedbackOptions = new()
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

    [Fact]
    public async Task GetAllFeedback_ShouldReturnSubmittedFeedbackWithComment()
    {
        // Arrange - Submit feedback with a known comment
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        AgentResult result = await conversation.SendMessageAsync("How do I boil an egg?");

        Assert.NotNull(result.AgentMessageId);
        Guid agentMessageId = result.AgentMessageId!.Value;
        string comment = $"Integration test comment {agentMessageId}";

        await conversation.SubmitFeedbackAsync(new SubmitFeedbackReq
        {
            AgentMessageId = agentMessageId,
            Feedback = false,
            Comment = comment
        });

        await Task.Delay(500);

        // Act
        MessageFeedbackPage page = await _client.Agents.Assistants.GetMessageFeedbackAsync(
            _fixture.AssistantAgent,
            new GetMessageFeedbackReq { PageSize = 100 });

        // Assert
        Assert.Equal(_fixture.AssistantAgent, page.AgentCode);
        Assert.Equal(1, page.Page);
        Assert.Equal(100, page.PageSize);
        Assert.True(page.Total > 0);

        MessageFeedbackRes? stored = page.Items.FirstOrDefault(f => f.AgentMessageId == agentMessageId);

        Assert.NotNull(stored);
        Assert.Equal(comment, stored!.Comment);
        Assert.False(stored.Feedback);
        Assert.NotEqual(Guid.Empty, stored.Id);
        Assert.False(string.IsNullOrEmpty(stored.AgentMessage));
    }

    [Fact]
    public async Task GetAllFeedback_WithDefaultOptions_ShouldUseApiDefaults()
    {
        // Act - No options means page 1 with the API default page size
        MessageFeedbackPage page = await _client.Agents.Assistants.GetMessageFeedbackAsync(_fixture.AssistantAgent);

        // Assert
        Assert.Equal(1, page.Page);
        Assert.Equal(20, page.PageSize);
        Assert.True(page.Items.Count <= 20);
    }

    [Fact]
    public async Task GetAllFeedback_WithDateRange_ShouldReturnOnlyFeedbackInRange()
    {
        // Arrange
        DateTime startDate = DateTime.UtcNow.AddDays(-7);
        DateTime endDate = DateTime.UtcNow.AddMinutes(5);

        // Act
        MessageFeedbackPage page = await _client.Agents.Assistants.GetMessageFeedbackAsync(
            _fixture.AssistantAgent,
            new GetMessageFeedbackReq
            {
                PageSize = 100,
                StartDate = startDate,
                EndDate = endDate,
                SortDirection = "asc"
            });

        // Assert
        Assert.All(page.Items, feedback =>
        {
            Assert.True(feedback.DateUtc >= startDate);
            Assert.True(feedback.DateUtc <= endDate);
        });
    }

    [Fact]
    public async Task GetAllFeedback_WithInvalidSortDirection_ShouldFail()
    {
        // The SDK does not validate the sort direction locally, the API answers with a 400
        GetMessageFeedbackReq options = new() { SortDirection = "sideways" };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _client.Agents.Assistants.GetMessageFeedbackAsync(_fixture.AssistantAgent, options));
    }

    [Fact]
    public async Task GetAllFeedback_WithPageSizeAboveLimit_ShouldFail()
    {
        // The API caps the page size at 1000
        GetMessageFeedbackReq options = new() { PageSize = 1001 };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _client.Agents.Assistants.GetMessageFeedbackAsync(_fixture.AssistantAgent, options));
    }

    [Fact]
    public async Task GetAllFeedback_WithUnknownAgentCode_ShouldFail()
    {
        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _client.Agents.Assistants.GetMessageFeedbackAsync("agent-that-does-not-exist"));
    }

    [Fact]
    public async Task GetAllFeedback_WithEmptyAgentCode_ShouldThrowArgumentNullException()
    {
        // Act & Assert - This one is validated locally, no request is issued
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _client.Agents.Assistants.GetMessageFeedbackAsync(string.Empty));
    }

    [Fact]
    public async Task GetAllFeedback_OnCopilots_ShouldSucceed()
    {
        // Feedback exists for both conversational agent types, so the operation is available
        // on Copilots too and not only on Assistants
        MessageFeedbackPage page = await _client.Agents.Copilots.GetMessageFeedbackAsync(_fixture.CopilotAgent);

        // Assert
        Assert.Equal(_fixture.CopilotAgent, page.AgentCode);
        Assert.Equal(1, page.Page);
    }
}
