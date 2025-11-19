using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Streaming;
using SerenityStar.Models.AIProxy;
using SerenityStar.Agents.System;
using Xunit;

namespace SerenityStar.IntegrationTests;

public class AIProxyIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ISerenityClient _client;

    public AIProxyIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISerenityClient>();
    }

    [Fact]
    public async Task ExecuteAsync_WithBasicMessage_ShouldSucceed()
    {
        // Arrange
        ProxyExecutionReq options = new()
        {
            Model = "gpt-4o-mini",
            Vendor = "OpenAI",
            MaxTokens = 16000,
            Messages =
            [
                new() { Role = "user", Content = "What is 2+2?" }
            ]
        };

        // Act
        AgentResult result = await _client.Agents.Proxies.ExecuteAsync(_fixture.ProxyAgent, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleMessages_ShouldSucceed()
    {
        // Arrange
        ProxyExecutionReq options = new()
        {
            Model = "gpt-4o-mini",
            Vendor = "OpenAI",
            MaxTokens = 16000,
            Messages =
            [
                new() { Role = "system", Content = "You always answer in german." },
                new() { Role = "user", Content = "Tell me a short joke" }
            ]
        };

        // Act
        AgentResult result = await _client.Agents.Proxies.ExecuteAsync(_fixture.ProxyAgent, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task ExecuteAsync_WithTemperatureParameter_ShouldSucceed()
    {
        // Arrange
        ProxyExecutionReq options = new()
        {
            Model = "gpt-4o-mini",
            Vendor = "OpenAI",
            MaxTokens = 16000,
            Temperature = 0.7,
            Messages =
            [
                new() { Role = "user", Content = "What is AI?" }
            ]
        };

        // Act
        AgentResult result = await _client.Agents.Proxies.ExecuteAsync(_fixture.ProxyAgent, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleParameters_ShouldSucceed()
    {
        // Arrange
        ProxyExecutionReq options = new()
        {
            Model = "gpt-4o-mini",
            Vendor = "OpenAI",
            Temperature = 0.5,
            MaxTokens = 150,
            TopP = 0.9,
            UserIdentifier = "user-123",
            GroupIdentifier = "group-456",
            Messages =
            [
                new() { Role = "user", Content = "How does photosynthesis work?" }
            ]
        };

        // Act
        AgentResult result = await _client.Agents.Proxies.ExecuteAsync(_fixture.ProxyAgent, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutMessages_ShouldFail()
    {
        // Arrange
        ProxyExecutionReq options = new()
        {
            Model = "gpt-4o-mini",
            Vendor = "OpenAI",
            MaxTokens = 16000,
            Messages = [] // Empty messages
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _client.Agents.Proxies.ExecuteAsync(_fixture.ProxyAgent, options));
    }

    [Fact]
    public async Task StreamAsync_WithBasicMessage_ShouldSucceed()
    {
        // Arrange
        ProxyExecutionReq options = new()
        {
            Model = "gpt-4o-mini",
            Vendor = "OpenAI",
            MaxTokens = 16000,
            Messages =
            [
                new() { Role = "user", Content = "Say hello in 5 different languages" }
            ]
        };

        Proxy proxy = _client.Agents.Proxies.Create(_fixture.ProxyAgent, options);
        List<StreamingAgentMessage> messages = [];

        // Act
        await foreach (StreamingAgentMessage message in proxy.StreamAsync())
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
        ProxyExecutionReq options = new()
        {
            Model = "gpt-4o-mini",
            Vendor = "OpenAI",
            MaxTokens = 100,
            Messages =
            [
                new() { Role = "user", Content = "What is quantum computing?" }
            ]
        };

        Proxy proxy = _client.Agents.Proxies.Create(_fixture.ProxyAgent, options);
        List<StreamingAgentMessage> messages = [];

        // Act
        await foreach (StreamingAgentMessage message in proxy.StreamAsync())
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
    public async Task ExecuteAsync_WithDifferentModels_ShouldReturnDifferentResponses()
    {
        // Arrange
        string[] models = ["gpt-4o-mini", "gpt-5.1"];
        List<string> responses = [];

        // Act
        foreach (string model in models)
        {
            ProxyExecutionReq options = new()
            {
                Model = model,
                Vendor = "OpenAI",
                MaxTokens = 16000,
                Messages =
                [
                    new() { Role = "user", Content = "What is AI in one sentence?" }
                ]
            };

            AgentResult result = await _client.Agents.Proxies.ExecuteAsync(_fixture.ProxyAgent, options);

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.NotEmpty(result.Content);

            responses.Add(result.Content);

            // Add delay to avoid rate limiting
            await Task.Delay(1000);
        }

        // Assert - Both models should provide responses
        Assert.Equal(models.Length, responses.Count);
        Assert.All(responses, Assert.NotNull);
    }

    [Fact]
    public async Task ExecuteAsync_WithPenaltyParameters_ShouldSucceed()
    {
        // Arrange
        ProxyExecutionReq options = new()
        {
            Model = "gpt-4o-mini",
            Vendor = "OpenAI",
            MaxTokens = 16000,
            FrequencyPenalty = 0.5,
            PresencePenalty = 0.5,
            Messages =
            [
                new() { Role = "user", Content = "Write a short story about a robot" }
            ]
        };

        // Act
        AgentResult result = await _client.Agents.Proxies.ExecuteAsync(_fixture.ProxyAgent, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
    }
}
