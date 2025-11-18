using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Streaming;
using SerenityStar.Agents.Assistants;
using Xunit;

namespace SerenityStar.IntegrationTests;

public class ConversationIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ISerenityClient _client;
    private bool _skipTests;

    public ConversationIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISerenityClient>();
        _skipTests = !fixture.HasValidApiKey;
    }

    [Fact]
    public async Task SendMessage_WithInvalidAgent_ShouldFail()
    {
        if (_skipTests)
        {
            return; // Skip test when no valid API key
        }

        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation("invalid-agent");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            conversation.SendMessageAsync("Hello"));
    }

    [Fact]
    public async Task SendMultipleMessages_ShouldSucceed()
    {
        if (_skipTests)
        {
            return; // Skip test when no valid API key
        }

        // Arrange - Create a conversation
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        // Act - First message creates conversation automatically
        AgentResult firstResult = await conversation.SendMessageAsync("Hello, how are you?");
        Assert.NotNull(conversation.ConversationId);

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
        Assert.Equal(conversation.ConversationId, firstResult.InstanceId.ToString());
    }

    [Fact]
    public async Task FullConversationFlow_ShouldSucceed()
    {
        if (_skipTests)
        {
            return; // Skip test when no valid API key
        }

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
            {
                // Subsequent messages should use the same conversation
                Assert.Equal(conversationId, conversation.ConversationId);
            }

            // Add a small delay between messages to avoid rate limiting
            await Task.Delay(1000);
        }
    }

    [Fact]
    public async Task StreamMessage_ShouldSucceed()
    {
        if (_skipTests)
        {
            return; // Skip test when no valid API key
        }

        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        List<StreamingAgentMessage> messages = new List<StreamingAgentMessage>();

        // Act - Stream first message (creates conversation automatically)
        await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("Hello, tell me a short joke"))
        {
            messages.Add(message);
        }

        // Assert
        Assert.NotNull(conversation.ConversationId);
        Assert.NotEmpty(messages);

        // Should have at least start, content, and stop messages
        Assert.Contains(messages, m => m is StreamingAgentMessageStart);
        Assert.Contains(messages, m => m is StreamingAgentMessageContent);
        Assert.Contains(messages, m => m is StreamingAgentMessageStop);
    }
}