using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Streaming;
using SerenityStar.Agents.System;
using Xunit;

namespace SerenityStar.IntegrationTests;

public class ActivityIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ISerenityClient _client;

    public ActivityIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISerenityClient>();
    }

    [Fact]
    public async Task ExecuteAsync_WithWord_ShouldSucceed()
    {
        // Arrange
        AgentExecutionReq options = new()
        {
            InputParameters = new Dictionary<string, object>
            {
                ["word"] = "running"
            }
        };

        Activity activity = _client.Agents.Activities.Create(_fixture.ActivityAgent, options);

        // Act
        AgentResult result = await activity.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutWord_ShouldFail()
    {
        // Arrange - No input parameters
        Activity activity = _client.Agents.Activities.Create(_fixture.ActivityAgent);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            activity.ExecuteAsync());
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentWords_ShouldReturnDifferentResponses()
    {
        // Arrange
        string[] words = ["swimming", "cycling", "hiking"];
        var responses = new List<string>();

        // Act
        foreach (string word in words)
        {
            AgentExecutionReq options = new()
            {
                InputParameters = new Dictionary<string, object>
                {
                    ["word"] = word
                }
            };

            Activity activity = _client.Agents.Activities.Create(_fixture.ActivityAgent, options);
            AgentResult result = await activity.ExecuteAsync();

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.NotEmpty(result.Content);

            responses.Add(result.Content);

            // Add a small delay between requests to avoid rate limiting
            await Task.Delay(1000);
        }

        // Assert
        Assert.Equal(words.Length, responses.Distinct().Count());
    }

    [Fact]
    public async Task ExecuteAsync_WithVersion_ShouldSucceed()
    {
        // Arrange
        AgentExecutionReq options = new()
        {
            InputParameters = new Dictionary<string, object>
            {
                ["word"] = "dancing"
            },
            AgentVersion = 25
        };

        Activity activity = _client.Agents.Activities.Create(_fixture.ActivityAgent, options);

        // Act
        AgentResult result = await activity.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
    }

    [Fact]
    public async Task ExecuteAsync_WithActionResults_ShouldReturnValidData()
    {
        // Arrange
        AgentExecutionReq options = new()
        {
            InputParameters = new Dictionary<string, object>
            {
                ["word"] = "yoga"
            }
        };

        Activity activity = _client.Agents.Activities.Create(_fixture.ActivityAgent, options);

        // Act
        AgentResult result = await activity.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        if (result.ActionResults != null)
            Assert.IsType<Dictionary<string, object>>(result.ActionResults);

        if (result.ExecutorTaskLogs != null)
            Assert.IsType<List<AgentResult.Task>>(result.ExecutorTaskLogs);
    }

    [Fact]
    public async Task StreamAsync_WithWord_ShouldSucceed()
    {
        // Arrange
        AgentExecutionReq options = new()
        {
            InputParameters = new Dictionary<string, object>
            {
                ["word"] = "running"
            }
        };

        Activity activity = _client.Agents.Activities.Create(_fixture.ActivityAgent, options);
        List<StreamingAgentMessage> messages = [];

        // Act
        await foreach (StreamingAgentMessage message in activity.StreamAsync())
            messages.Add(message);

        // Assert
        Assert.NotEmpty(messages);
        Assert.Contains(messages, m => m is StreamingAgentMessageStart);
        Assert.Contains(messages, m => m is StreamingAgentMessageContent);
        Assert.Contains(messages, m => m is StreamingAgentMessageStop);
    }

    [Fact]
    public async Task StreamAsync_ShouldHaveCompleteStreamSequence()
    {
        // Arrange
        AgentExecutionReq options = new()
        {
            InputParameters = new Dictionary<string, object>
            {
                ["word"] = "tennis"
            }
        };

        Activity activity = _client.Agents.Activities.Create(_fixture.ActivityAgent, options);
        List<StreamingAgentMessage> messages = new();

        // Act
        await foreach (StreamingAgentMessage message in activity.StreamAsync())
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
    public async Task ExecuteAsync_WithBuilderClient_ShouldSucceed()
    {
        // Arrange - Create client using builder pattern
        string? apiKey = _fixture.Configuration["SerenityStar:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("API key is required for this test");

        SerenityClient serenityClient = new SerenityClientBuilder()
            .WithApiKey(apiKey)
            .Build();

        AgentExecutionReq options = new()
        {
            InputParameters = new Dictionary<string, object>
            {
                ["word"] = "running"
            }
        };

        Activity activity = serenityClient.Agents.Activities.Create(_fixture.ActivityAgent, options);

        // Act
        AgentResult result = await activity.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
    }
}
