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
        AgentExecutionOptions options = new()
        {
            InputParameters = new Dictionary<string, object>
            {
                ["word"] = "running"
            }
        };

        // Act
        AgentResult result = await _client.Agents.Activities.ExecuteAsync(_fixture.ActivityAgent, options);

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

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _client.Agents.Activities.ExecuteAsync(_fixture.ActivityAgent));
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
            AgentExecutionOptions options = new()
            {
                InputParameters = new Dictionary<string, object>
                {
                    ["word"] = word
                }
            };

            AgentResult result = await _client.Agents.Activities.ExecuteAsync(_fixture.ActivityAgent, options);

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
        AgentExecutionOptions options = new()
        {
            InputParameters = new Dictionary<string, object>
            {
                ["word"] = "dancing"
            },
            AgentVersion = 25
        };

        // Act
        AgentResult result = await _client.Agents.Activities.ExecuteAsync(_fixture.ActivityAgent, options);

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
        AgentExecutionOptions options = new()
        {
            InputParameters = new Dictionary<string, object>
            {
                ["word"] = "yoga"
            }
        };

        // Act
        AgentResult result = await _client.Agents.Activities.ExecuteAsync(_fixture.ActivityAgent, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        if (result.ActionResults != null)
            Assert.IsType<Dictionary<string, object>>(result.ActionResults);

        if (result.ExecutorTaskLogs != null)
            Assert.IsType<List<ExecutorTaskResult>>(result.ExecutorTaskLogs);
    }

    [Fact]
    public async Task StreamAsync_WithWord_ShouldSucceed()
    {
        // Arrange
        AgentExecutionOptions options = new()
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
        AgentExecutionOptions options = new()
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
}
