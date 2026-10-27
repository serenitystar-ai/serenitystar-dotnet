using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Agents.Conversational;
using SerenityStar.Client;
using SerenityStar.Models.Streaming;
using Xunit;

namespace SerenityStar.IntegrationTests;

/// <summary>
/// Integration tests for Agent TODOs. They require the assistant agent (SerenityStar:AssistantAgent) with Agent TODOs
/// enabled and a message that makes it plan a TODO list (SerenityStar:AgentTodoMessage).
/// </summary>
public class AgentTodosIntegrationTests : IClassFixture<TestFixture>
{
    private static readonly string[] s_knownStatuses = { AgentTodoStatus.Pending, AgentTodoStatus.Completed, AgentTodoStatus.Canceled };

    private readonly TestFixture _fixture;
    private readonly SerenityClient _client;

    public AgentTodosIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<SerenityClient>();
    }

    private static List<IReadOnlyList<AgentTodoItem>> GetTodoSnapshots(IEnumerable<StreamingAgentMessage> messages)
    {
        List<IReadOnlyList<AgentTodoItem>> snapshots = new();
        foreach (StreamingAgentMessageTaskStop taskStop in messages.OfType<StreamingAgentMessageTaskStop>())
            if (taskStop.TryGetAgentTodos(out IReadOnlyList<AgentTodoItem>? todos))
                snapshots.Add(todos);

        return snapshots;
    }

    [Fact]
    public async Task Assistant_Stream_WithTodos_ShouldReportTheListProgress()
    {
        // Arrange
        Conversation conversation = _client.GetAgents().Assistants.CreateConversation(_fixture.AssistantAgent);

        // Act
        List<StreamingAgentMessage> messages = new();
        await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync(_fixture.AgentTodoMessage))
            messages.Add(message);

        // Assert
        Assert.Empty(messages.OfType<StreamingAgentMessageError>());
        StreamingAgentMessageStop stop = Assert.Single(messages.OfType<StreamingAgentMessageStop>());
        Assert.False(string.IsNullOrEmpty(stop.Result?.Content));

        List<IReadOnlyList<AgentTodoItem>> snapshots = GetTodoSnapshots(messages);
        Assert.NotEmpty(snapshots);
        Assert.All(snapshots, snapshot => Assert.All(snapshot, item =>
        {
            Assert.True(item.Id > 0);
            Assert.False(string.IsNullOrWhiteSpace(item.Description));
            Assert.Contains(item.Status, s_knownStatuses);
        }));

        // Items are only added, so each id keeps its description in every later snapshot.
        Dictionary<int, string> descriptions = new();
        foreach (AgentTodoItem item in snapshots.SelectMany(snapshot => snapshot))
        {
            if (descriptions.TryGetValue(item.Id, out string? description))
                Assert.Equal(description, item.Description);
            else
                descriptions[item.Id] = item.Description;
        }

        // The agent keeps iterating until no item is left pending, so the last list must be resolved.
        IReadOnlyList<AgentTodoItem> last = snapshots[^1];
        Assert.NotEmpty(last);
        Assert.DoesNotContain(last, item => item.Status == AgentTodoStatus.Pending);
    }

    [Fact]
    public async Task Assistant_Stream_WithTodos_ShouldReportProgressBeforeTheListIsDone()
    {
        // Arrange
        Conversation conversation = _client.GetAgents().Assistants.CreateConversation(_fixture.AssistantAgent);

        // Act
        List<StreamingAgentMessage> messages = new();
        await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync(_fixture.AgentTodoMessage))
            messages.Add(message);

        // Assert: the list is first reported while items are still pending, then as they get resolved.
        List<IReadOnlyList<AgentTodoItem>> snapshots = GetTodoSnapshots(messages);
        Assert.True(snapshots.Count > 1);
        Assert.Contains(snapshots[0], item => item.Status == AgentTodoStatus.Pending);

        int[] resolvedCounts = snapshots
            .Select(snapshot => snapshot.Count(item => item.Status != AgentTodoStatus.Pending))
            .ToArray();
        Assert.True(resolvedCounts[^1] > resolvedCounts[0]);

        Assert.Equal(conversation.ConversationId, Assert.Single(messages.OfType<StreamingAgentMessageStop>()).Result?.InstanceId.ToString());
    }
}
