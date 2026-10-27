using SerenityStar.Agents.Conversational;
using SerenityStar.Client;
using SerenityStar.Models.Streaming;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SerenityStar.IntegrationTests;

/// <summary>
/// Offline tests for Agent TODOs. These use a fake <see cref="HttpMessageHandler"/> to return a canned SSE response,
/// so they do not require an API key or network access.
/// </summary>
public class AgentTodosOfflineTests
{
    private const string BaseUrl = "https://offline.example.com";
    private const string ConversationId = "11111111-1111-1111-1111-111111111111";

    private sealed class StaticResponseHandler : HttpMessageHandler
    {
        private readonly string _responseBody;

        public StaticResponseHandler(string responseBody) => _responseBody = responseBody;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseBody, Encoding.UTF8, "text/event-stream")
            });
    }

    private static string Sse(params string[] events) =>
        string.Concat(events.Select(e => $"data: {e}\n\n")) + "data: [DONE]\n\n";

    private static string SkillTaskStop(string tool, string content) =>
        $$$"""{"type":"task_stop","task":"Executing Skill: AgentTodo","task_key":"skills_AgentTodo_{{{tool}}}_execute","output":{"content":{{{content}}},"type":"AgentTodo","iteration":0},"success":true}""";

    private static async Task<List<StreamingAgentMessageTaskStop>> StreamTaskStopsAsync(params string[] events)
    {
        SerenityClient client = new SerenityClientBuilder()
            .WithApiKey("test-key")
            .WithBaseUrl(BaseUrl)
            .WithHttpClient(new HttpClient(new StaticResponseHandler(Sse(events))))
            .Build();

        Conversation conversation = client.GetAgents().Assistants.CreateConversation("assistant");

        List<StreamingAgentMessageTaskStop> taskStops = new();
        await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync("Plan and do it"))
            if (message is StreamingAgentMessageTaskStop taskStop)
                taskStops.Add(taskStop);

        return taskStops;
    }

    private static StreamingAgentMessageTaskStop TaskStop(string taskKey, string output) =>
        new() { TaskKey = taskKey, Output = JsonDocument.Parse(output).RootElement.Clone() };

    [Fact]
    public async Task Stream_SurfacesTheListFromEachTodoTask()
    {
        List<StreamingAgentMessageTaskStop> taskStops = await StreamTaskStopsAsync(
            $$$"""{"type":"start","instance_id":"{{{ConversationId}}}"}""",
            SkillTaskStop("AddTodos", """[{"id":1,"description":"Search flights","status":"Pending"},{"id":2,"description":"Book hotel","status":"Pending"}]"""),
            SkillTaskStop("CompleteTodos", """[{"id":1,"description":"Search flights","status":"Completed"},{"id":2,"description":"Book hotel","status":"Pending"}]"""),
            $$$"""{"type":"task_stop","task":"AgentTodoIterate","task_key":"{{{StreamingTaskKeys.AgentTodoIterate}}}","output":[{"id":1,"description":"Search flights","status":"Completed"},{"id":2,"description":"Book hotel","status":"Pending"}],"success":true}""",
            SkillTaskStop("CancelTodos", """[{"id":1,"description":"Search flights","status":"Completed"},{"id":2,"description":"Book hotel","status":"Canceled"}]"""),
            """{"type":"task_stop","task":"Getting AI response","task_key":"llm_response","output":{"text":"ignored"},"success":true}""",
            $$$"""{"type":"stop","result":{"content":"Done","instance_id":"{{{ConversationId}}}"}}""");

        List<IReadOnlyList<AgentTodoItem>> snapshots = new();
        foreach (StreamingAgentMessageTaskStop taskStop in taskStops)
            if (taskStop.TryGetAgentTodos(out IReadOnlyList<AgentTodoItem>? todos))
                snapshots.Add(todos);

        Assert.Equal(4, snapshots.Count);

        Assert.All(snapshots[0], item => Assert.Equal(AgentTodoStatus.Pending, item.Status));
        Assert.Equal(new[] { 1, 2 }, snapshots[0].Select(item => item.Id));
        Assert.Equal(new[] { "Search flights", "Book hotel" }, snapshots[0].Select(item => item.Description));

        Assert.Equal(new[] { AgentTodoStatus.Completed, AgentTodoStatus.Pending }, snapshots[1].Select(item => item.Status));
        Assert.Equal(new[] { AgentTodoStatus.Completed, AgentTodoStatus.Pending }, snapshots[2].Select(item => item.Status));
        Assert.Equal(new[] { AgentTodoStatus.Completed, AgentTodoStatus.Canceled }, snapshots[3].Select(item => item.Status));

        Assert.False(taskStops[^1].TryGetAgentTodos(out IReadOnlyList<AgentTodoItem>? notTodos));
        Assert.Null(notTodos);
    }

    [Fact]
    public void TryGetAgentTodos_IterationTask_ReadsTheBareList()
    {
        StreamingAgentMessageTaskStop taskStop = TaskStop(StreamingTaskKeys.AgentTodoIterate, """[{"id":3,"description":"Write summary","status":"Pending"}]""");

        Assert.True(taskStop.TryGetAgentTodos(out IReadOnlyList<AgentTodoItem>? todos));
        AgentTodoItem item = Assert.Single(todos);
        Assert.Equal(3, item.Id);
        Assert.Equal("Write summary", item.Description);
        Assert.Equal(AgentTodoStatus.Pending, item.Status);
    }

    [Fact]
    public void TryGetAgentTodos_EmptyList_ReturnsTrueWithNoItems()
    {
        StreamingAgentMessageTaskStop taskStop = TaskStop("skills_AgentTodo_GetTodos_execute", """{"content":[],"type":"AgentTodo","iteration":0}""");

        Assert.True(taskStop.TryGetAgentTodos(out IReadOnlyList<AgentTodoItem>? todos));
        Assert.Empty(todos);
    }

    [Fact]
    public void TryGetAgentTodos_UnknownStatus_IsKeptAsReceived()
    {
        StreamingAgentMessageTaskStop taskStop = TaskStop(StreamingTaskKeys.AgentTodoIterate, """[{"id":1,"description":"Retry","status":"Blocked"}]""");

        Assert.True(taskStop.TryGetAgentTodos(out IReadOnlyList<AgentTodoItem>? todos));
        Assert.Equal("Blocked", Assert.Single(todos).Status);
    }

    [Theory]
    // Another skill type
    [InlineData("skills_WebSearch_execute", """{"content":[{"id":1,"description":"x","status":"Pending"}],"type":"WebSearch","iteration":0}""")]
    // A TODO skill that failed carries an error instead of the list
    [InlineData("skills_AgentTodo_AddTodos_execute", """{"message":"Something went wrong"}""")]
    // A TODO skill result whose content is not a list
    [InlineData("skills_AgentTodo_AddTodos_execute", """{"content":"not a list","type":"AgentTodo","iteration":0}""")]
    // The iteration task with an unexpected output
    [InlineData(StreamingTaskKeys.AgentTodoIterate, """{"content":[]}""")]
    // Other pending action tasks
    [InlineData(StreamingTaskKeys.UserChoiceRequest, """{"type":"user_choice","questions":[]}""")]
    public void TryGetAgentTodos_TaskWithoutTodoList_ReturnsFalse(string taskKey, string output)
    {
        StreamingAgentMessageTaskStop taskStop = TaskStop(taskKey, output);

        Assert.False(taskStop.TryGetAgentTodos(out IReadOnlyList<AgentTodoItem>? todos));
        Assert.Null(todos);
    }

    [Fact]
    public void TryGetAgentTodos_NoOutput_ReturnsFalse()
    {
        StreamingAgentMessageTaskStop taskStop = new() { TaskKey = StreamingTaskKeys.AgentTodoIterate };

        Assert.False(taskStop.TryGetAgentTodos(out _));
    }
}
