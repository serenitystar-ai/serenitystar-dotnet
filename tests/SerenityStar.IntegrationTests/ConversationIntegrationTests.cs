using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Streaming;
using SerenityStar.Models.Conversation;
using SerenityStar.Agents.Conversational;
using Xunit;

namespace SerenityStar.IntegrationTests;

public class ConversationIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ISerenityClient _client;

    public ConversationIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISerenityClient>();
    }

    [Fact]
    public async Task SendMessage_WithInvalidAgent_ShouldFail()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation("invalid-agent");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            conversation.SendMessageAsync("Hello"));
    }

    [Fact]
    public async Task CreateConversation_ShouldReturnValidInstance()
    {
        // Arrange & Act
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        // Assert
        Assert.NotNull(conversation);
        Assert.Null(conversation.ConversationId); // Not set until first message
    }

    [Fact]
    public async Task ResumeConversation_WithExistingId_ShouldUseProvidedId()
    {
        // Arrange
        const string testConversationId = "test-conv-id-12345";
        Conversation conversation = _client.Agents.Assistants.CreateConversation(
            _fixture.AssistantAgent,
            conversationId: testConversationId
        );

        // Act & Assert
        Assert.NotNull(conversation);
        Assert.Equal(testConversationId, conversation.ConversationId);
    }

    [Fact]
    public async Task SendMultipleMessages_ShouldSucceed()
    {
        // Arrange - Create a conversation
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        // Act - First message creates conversation automatically
        AgentResult firstResult = await conversation.SendMessageAsync("Hello, how are you?");
        string? conversationId = conversation.ConversationId;

        // Add delay to avoid rate limiting
        await Task.Delay(1000);

        // Second message uses the same conversation
        AgentResult secondResult = await conversation.SendMessageAsync("What can you help me with?");

        // Assert
        Assert.NotEqual(Guid.Empty, firstResult.InstanceId);
        Assert.NotNull(firstResult.Content);
        Assert.NotEmpty(firstResult.Content);

        Assert.NotEqual(Guid.Empty, secondResult.InstanceId);
        Assert.NotNull(secondResult.Content);
        Assert.NotEmpty(secondResult.Content);

        // Both messages should use the same conversation
        Assert.NotNull(conversationId);
        Assert.Equal(conversationId, conversation.ConversationId);
    }

    [Fact]
    public async Task FullConversationFlow_ShouldSucceed()
    {
        // Arrange - Create a conversation
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        // Act & Assert - Send multiple messages
        string[] messages =
        [
            "Hello, how are you?",
            "What can you help me with?",
            "Thank you for your help!"
        ];

        string? conversationId = null;

        foreach (string message in messages)
        {
            AgentResult response = await conversation.SendMessageAsync(message);

            Assert.NotNull(response);
            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content);

            // Verify conversation ID is set after first message
            if (conversationId == null)
            {
                conversationId = conversation.ConversationId;
                Assert.NotNull(conversationId);
            }
            else
                // Subsequent messages should use the same conversation
                Assert.Equal(conversationId, conversation.ConversationId);

            // Add a small delay between messages to avoid rate limiting
            await Task.Delay(1000);
        }
    }

    [Fact]
    public async Task StreamMessage_ShouldSucceed()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        List<StreamingAgentMessage> messages = [];

        // Act - Stream first message (creates conversation automatically)
        await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("Hello, tell me a short joke"))
            messages.Add(message);

        // Assert
        Assert.NotNull(conversation.ConversationId);
        Assert.NotEmpty(messages);

        // Should have at least start, content, and stop messages
        Assert.Contains(messages, m => m is StreamingAgentMessageStart);
        Assert.Contains(messages, m => m is StreamingAgentMessageContent);
        Assert.Contains(messages, m => m is StreamingAgentMessageStop);
    }

    [Fact]
    public async Task StreamMessage_MultipleMessages_ShouldSucceed()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        // Act - First stream (creates conversation)
        List<StreamingAgentMessage> firstMessages = new();
        await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("What's a good breakfast?"))
            firstMessages.Add(message);

        string? conversationIdAfterFirst = conversation.ConversationId;

        // Add delay to avoid rate limiting
        await Task.Delay(1000);

        // Second stream (uses existing conversation)
        List<StreamingAgentMessage> secondMessages = new();
        await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("Any protein suggestions?"))
            secondMessages.Add(message);

        // Assert
        Assert.NotEmpty(firstMessages);
        Assert.NotEmpty(secondMessages);
        Assert.NotNull(conversationIdAfterFirst);
        Assert.Equal(conversationIdAfterFirst, conversation.ConversationId);

        // Both should have proper streaming messages
        Assert.Contains(firstMessages, m => m is StreamingAgentMessageStart);
        Assert.Contains(secondMessages, m => m is StreamingAgentMessageStart);
    }

    [Fact]
    public async Task StreamMessage_WithExecutionOptions_ShouldSucceed()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(
            _fixture.AssistantAgent,
            options: new AgentExecutionReq
            {
                UserIdentifier = "test-user-123",
                Channel = "test-channel"
            }
        );
        List<StreamingAgentMessage> messages = [];

        // Act
        await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("Hello with options"))
            messages.Add(message);

        // Assert
        Assert.NotEmpty(messages);
        Assert.NotNull(conversation.ConversationId);
        Assert.Contains(messages, m => m is StreamingAgentMessageContent);
    }

    [Fact]
    public async Task GetConversationInfo_ShouldSucceed()
    {
        // Act
        ConversationInfoResult info = await _client.Agents.Assistants.GetInfoByCodeAsync(_fixture.AssistantAgent);

        // Assert
        Assert.NotNull(info);
        Assert.NotNull(info.Conversation);
        Assert.NotNull(info.Agent);
    }

    [Fact]
    public async Task GetConversationInfo_WithOptions_ShouldSucceed()
    {
        // Arrange
        var options = new AgentExecutionReq
        {
            UserIdentifier = "test-user",
            Channel = "web"
        };

        // Act
        ConversationInfoResult info = await _client.Agents.Assistants.GetInfoByCodeAsync(
            _fixture.AssistantAgent,
            options
        );

        // Assert
        Assert.NotNull(info);
        Assert.NotNull(info.Conversation);
    }
}
