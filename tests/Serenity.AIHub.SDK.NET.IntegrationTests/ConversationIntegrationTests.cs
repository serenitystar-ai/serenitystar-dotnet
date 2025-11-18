using Microsoft.Extensions.DependencyInjection;
using Serenity.AIHub.SDK.NET.Core.Client;
using Serenity.AIHub.SDK.NET.Core.Models;
using Serenity.AIHub.SDK.NET.Core.Models.Execute;
using Xunit;

namespace Serenity.AIHub.SDK.NET.IntegrationTests;

public class ConversationIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ISerenityAIHubClient _client;
    private bool _skipTests;

    public ConversationIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISerenityAIHubClient>();
        _skipTests = !fixture.HasValidApiKey;
    }

    [Fact]
    public async Task CreateConversation_WithInputParameters_ShouldSucceed()
    {
        if (_skipTests)
        {
            return; // Skip test when no valid API key
        }

        // Arrange - Define input parameters
        List<ExecuteParameter> inputParameters =
        [
            new ExecuteParameter("name", "Matthew")
        ];

        // Act - Create a conversation with input parameters
        CreateConversationRes result = await _client.CreateConversation("assistantagent", inputParameters);

        // Assert - Verify the conversation was created successfully
        Assert.NotEqual(Guid.Empty, result.ChatId);
        Assert.NotNull(result.Content);

        // Optionally, verify that the input parameters were processed correctly
        Assert.Contains("Matthew", result.Content);
    }

    [Fact]
    public async Task CreateConversation_WithoutVersion_ShouldSucceed()
    {
        if (_skipTests)
        {
            return; // Skip test when no valid API key
        }

        // Act
        CreateConversationRes result = await _client.CreateConversation("assistantagent", null);

        // Assert
        Assert.NotEqual(Guid.Empty, result.ChatId);
        Assert.NotNull(result.Content);
        Assert.NotNull(result.ConversationStarters);
    }

    [Fact]
    public async Task CreateConversation_WithVersion_ShouldSucceed()
    {
        if (_skipTests)
        {
            return; // Skip test when no valid API key
        }

        // Act
        CreateConversationRes result = await _client.CreateConversation("assistantagent", null, 1);

        // Assert
        Assert.NotEqual(Guid.Empty, result.ChatId);
        Assert.NotNull(result.Content);
        Assert.NotNull(result.ConversationStarters);
        Assert.NotEmpty(result.ConversationStarters);
        Assert.Equal(1, result.Version);
    }

    [Fact]
    public async Task CreateConversation_WithInvalidAgent_ShouldFail()
    {
        if (_skipTests)
        {
            return; // Skip test when no valid API key
        }

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _client.CreateConversation("invalid-agent", null));
    }

    [Fact]
    public async Task CreateConversationAndSendMessage_ShouldSucceed()
    {
        if (_skipTests)
        {
            return; // Skip test when no valid API key
        }

        // Arrange - Create a conversation first
        CreateConversationRes conversation = await _client.CreateConversation("assistantagent", null);
        Assert.NotEqual(Guid.Empty, conversation.ChatId);

        // Act - Send a message to the conversation
        List<ExecuteParameter> input = [];
        input.Add(new(
                "chatId",
                conversation.ChatId
            ));
        input.Add(new(
            "message",
            "Hello, how are you?"
        ));
        input.Add(new(
            "language",
            "german"
        ));
        AgentResult agentResult = await _client.Execute(
            "assistantagent",
            input);

        // Assert
        Assert.NotNull(agentResult);
        Assert.NotNull(agentResult.Content);
        Assert.NotEmpty(agentResult.Content);
        Assert.NotEqual(Guid.Empty, agentResult.InstanceId);

        // Optional properties might be null depending on the response
        if (agentResult.ActionResults != null)
            Assert.IsType<Dictionary<string, object>>(agentResult.ActionResults);

        if (agentResult.ExecutorTaskLogs != null)
            Assert.IsType<List<ExecutorTaskResult>>(agentResult.ExecutorTaskLogs);
    }

    [Fact]
    public async Task FullConversationFlow_ShouldSucceed()
    {
        if (_skipTests)
        {
            return; // Skip test when no valid API key
        }

        // Arrange - Create a conversation
        CreateConversationRes conversation = await _client.CreateConversation("assistantagent", null);
        Assert.NotEqual(Guid.Empty, conversation.ChatId);

        // Act & Assert - Send multiple messages
        string[] messages =
        [
            "Hello, how are you?",
            "What can you help me with?",
            "Thank you for your help!"
        ];

        foreach (string message in messages)
        {
            List<ExecuteParameter> input = [];
            input.Add(new(
                "chatId",
                conversation.ChatId
            ));
            input.Add(new(
                "message",
                message
            ));

            input.Add(new(
                "language",
                "german"
            ));

            AgentResult response = await _client.Execute(
                "assistantagent", input);

            Assert.NotNull(response);
            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content);

            // Add a small delay between messages to avoid rate limiting
            await Task.Delay(1000);
        }
    }
}