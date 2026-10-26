using SerenityStar.Agents.Conversational;
using SerenityStar.Agents.System;
using SerenityStar.Client;
using SerenityStar.Exceptions;
using SerenityStar.Extensions;
using SerenityStar.Models.AIProxy;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Streaming;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SerenityStar.IntegrationTests;

/// <summary>
/// Offline tests for skill approvals. These use a fake <see cref="HttpMessageHandler"/> to capture the outgoing request
/// and return a canned response, so they do not require an API key or network access.
/// </summary>
public class SkillApprovalsOfflineTests
{
    private const string BaseUrl = "https://offline.example.com";
    private const string ConversationId = "11111111-1111-1111-1111-111111111111";

    private const string SnakeCaseApproval = """{"type":"approval","request_id":"req-1","call_id":"call-1","skill_code":"send_email","skill_type":"HttpRequest","tool":null,"arguments":{"to":"john@example.com"}}""";

    /// <summary>
    /// Captures the outgoing request JSON body and returns the configured response.
    /// </summary>
    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;
        private readonly string _responseMediaType;

        public CapturingHandler(string responseBody, string responseMediaType = "application/json", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseBody = responseBody;
            _responseMediaType = responseMediaType;
            _statusCode = statusCode;
        }

        public int RequestCount { get; private set; }
        public Uri? RequestUri { get; private set; }
        public JsonElement? JsonBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            RequestUri = request.RequestUri;

            if (request.Content != null)
            {
                using JsonDocument document = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
                JsonBody = document.RootElement.Clone();
            }

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, Encoding.UTF8, _responseMediaType)
            };
        }

        /// <summary>
        /// Gets the value of a conversational input parameter from the captured key/value body.
        /// </summary>
        public JsonElement? Parameter(string key)
        {
            foreach (JsonElement parameter in JsonBody!.Value.EnumerateArray())
                if (parameter.GetProperty("key").GetString() == key)
                    return parameter.GetProperty("value");

            return null;
        }
    }

    private static SerenityClient CreateClient(CapturingHandler handler) =>
        new SerenityClientBuilder()
            .WithApiKey("test-key")
            .WithBaseUrl(BaseUrl)
            .WithHttpClient(new HttpClient(handler))
            .Build();

    private static string Sse(params string[] events) =>
        string.Concat(events.Select(e => $"data: {e}\n\n")) + "data: [DONE]\n\n";

    private static async Task<List<StreamingAgentMessage>> ToListAsync(IAsyncEnumerable<StreamingAgentMessage> stream)
    {
        List<StreamingAgentMessage> messages = new();
        await foreach (StreamingAgentMessage message in stream)
            messages.Add(message);
        return messages;
    }

    #region Pending actions
    [Fact]
    public async Task SendMessage_ParsesEveryPendingActionType()
    {
        CapturingHandler handler = new($$$"""
            {"content":"","instanceId":"{{{ConversationId}}}","pendingActions":[
              {"type":"connection","url":"https://connect","connectorId":"22222222-2222-2222-2222-222222222222","connectorName":"Drive","connectorImgUrl":"https://img"},
              {"type":"approval","requestId":"req-1","callId":"call-1","skillCode":"send_email","skillType":"HttpRequest","tool":"send","arguments":{"to":"john@example.com"}},
              {"type":"user_choice","questions":[]}]}
            """);

        Conversation conversation = CreateClient(handler).GetAgents().Assistants.CreateConversation("assistant");
        AgentResult result = await conversation.SendMessageAsync("Send an email to John");

        Assert.Equal(3, result.PendingActions.Count);

        ConnectionPendingAction connection = Assert.IsType<ConnectionPendingAction>(result.PendingActions[0]);
        Assert.Equal("Drive", connection.ConnectorName);

        ApprovalPendingAction approval = Assert.IsType<ApprovalPendingAction>(result.PendingActions[1]);
        Assert.Equal("req-1", approval.RequestId);
        Assert.Equal("call-1", approval.CallId);
        Assert.Equal("send_email", approval.SkillCode);
        Assert.Equal("HttpRequest", approval.SkillType);
        Assert.Equal("send", approval.Tool);
        Assert.Equal("john@example.com", ((JsonElement)approval.Arguments["to"]).GetString());

        UnsupportedPendingAction unsupported = Assert.IsType<UnsupportedPendingAction>(result.PendingActions[2]);
        Assert.Equal("user_choice", unsupported.OriginalType);
        Assert.True(unsupported.RawData!.Value.TryGetProperty("questions", out _));
    }

    [Fact]
    public void ApprovalPendingAction_ApproveAndReject_CreateDecisionsForTheRequest()
    {
        ApprovalPendingAction approval = new() { RequestId = "req-1" };

        ApprovalDecision approved = approval.Approve();
        ApprovalDecision rejected = approval.Reject("Wrong recipient");

        Assert.Equal("req-1", approved.RequestId);
        Assert.True(approved.Approved);
        Assert.Null(approved.Reason);
        Assert.Equal("req-1", rejected.RequestId);
        Assert.False(rejected.Approved);
        Assert.Equal("Wrong recipient", rejected.Reason);
    }
    #endregion

    #region Conversational
    [Fact]
    public async Task SendApprovals_SendsDecisionsChatIdMessageAndSkills()
    {
        CapturingHandler handler = new($$"""{"content":"Email sent","instanceId":"{{ConversationId}}"}""");

        Conversation conversation = CreateClient(handler).GetAgents().Assistants.CreateConversation(
            "assistant",
            ConversationId,
            new AgentExecutionReq { Skills = new SkillsOptions { AutoMode = true } });

        ApprovalPendingAction first = new() { RequestId = "req-1" };
        ApprovalPendingAction second = new() { RequestId = "req-2" };

        AgentResult result = await conversation.SendApprovalsAsync(
            new[] { first.Approve(), second.Reject("Not now") },
            "And let me know when it's done");

        Assert.Equal("Email sent", result.Content);
        Assert.Equal($"{BaseUrl}/api/v2/agent/assistant/execute", handler.RequestUri!.AbsoluteUri);
        Assert.Equal(ConversationId, handler.Parameter("chatId")!.Value.GetString());
        Assert.Equal("And let me know when it's done", handler.Parameter("message")!.Value.GetString());
        Assert.True(handler.Parameter("skills")!.Value.GetProperty("autoMode").GetBoolean());

        JsonElement approvals = handler.Parameter("toolApprovals")!.Value;
        Assert.Equal(2, approvals.GetArrayLength());
        Assert.Equal("req-1", approvals[0].GetProperty("requestId").GetString());
        Assert.True(approvals[0].GetProperty("approved").GetBoolean());
        Assert.Equal("req-2", approvals[1].GetProperty("requestId").GetString());
        Assert.False(approvals[1].GetProperty("approved").GetBoolean());
        Assert.Equal("Not now", approvals[1].GetProperty("reason").GetString());
    }

    [Fact]
    public async Task SendMessage_WithoutSkillsOptions_DoesNotSendSkillsOrApprovals()
    {
        CapturingHandler handler = new($$"""{"content":"Hi","instanceId":"{{ConversationId}}"}""");

        await CreateClient(handler).GetAgents().Assistants.CreateConversation("assistant").SendMessageAsync("Hi");

        Assert.Null(handler.Parameter("skills"));
        Assert.Null(handler.Parameter("toolApprovals"));
    }

    [Fact]
    public async Task SendApprovals_RejectsInvalidInputBeforeSending()
    {
        CapturingHandler handler = new("{}");
        AssistantsScope assistants = CreateClient(handler).GetAgents().Assistants;
        ApprovalDecision decision = new ApprovalPendingAction { RequestId = "req-1" }.Approve();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => assistants.CreateConversation("assistant").SendApprovalsAsync(new[] { decision }));
        await Assert.ThrowsAsync<ArgumentException>(
            () => assistants.CreateConversation("assistant", ConversationId).SendApprovalsAsync(Array.Empty<ApprovalDecision>()));
        await Assert.ThrowsAsync<ArgumentException>(
            () => assistants.CreateConversation("assistant", ConversationId).SendApprovalsAsync(new[] { new ApprovalDecision() }));
        Assert.Throws<InvalidOperationException>(
            () => assistants.CreateConversation("assistant").StreamApprovalsAsync(new[] { decision }));

        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task SendApprovals_UnansweredApproval_ExposesPendingActionsOnException()
    {
        CapturingHandler handler = new(
            """{"code":"validation_error","message":"Pending approvals","errors":{"tool_approval_pending":"Answer every approval"},"pendingActions":[{"type":"approval","requestId":"req-2","callId":"call-2","skillCode":"send_email","skillType":"HttpRequest","arguments":{}}]}""",
            statusCode: HttpStatusCode.BadRequest);

        Conversation conversation = CreateClient(handler).GetAgents().Assistants.CreateConversation("assistant", ConversationId);

        SerenityApiException exception = await Assert.ThrowsAsync<SerenityApiException>(
            () => conversation.SendApprovalsAsync(new[] { new ApprovalPendingAction { RequestId = "req-1" }.Approve() }));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        ApprovalPendingAction pending = Assert.IsType<ApprovalPendingAction>(Assert.Single(exception.GetPendingActions()));
        Assert.Equal("req-2", pending.RequestId);
    }

    [Fact]
    public void GetPendingActions_WithoutPendingActions_ReturnsEmpty()
    {
        SerenityApiException exception = new("Bad request", HttpStatusCode.BadRequest);

        Assert.Empty(exception.GetPendingActions());
    }
    #endregion

    #region Streaming
    [Fact]
    public async Task StreamMessage_SurfacesApprovalOnTaskStopAndStopResult()
    {
        CapturingHandler handler = new(Sse(
            $$"""{"type":"task_stop","task":"Requesting tool approval","task_key":"pending_actions_approval_request","output":{{SnakeCaseApproval}},"success":true}""",
            """{"type":"task_stop","task":"Getting AI response","task_key":"llm_response","output":{"text":"x"},"success":true}""",
            $$$"""{"type":"stop","result":{"content":"","instance_id":"{{{ConversationId}}}","pending_actions":[{{{SnakeCaseApproval}}}]}}"""),
            "text/event-stream");

        Conversation conversation = CreateClient(handler).GetAgents().Assistants.CreateConversation("assistant");
        List<StreamingAgentMessage> messages = await ToListAsync(conversation.StreamMessageAsync("Send an email to John"));

        List<StreamingAgentMessageTaskStop> taskStops = messages.OfType<StreamingAgentMessageTaskStop>().ToList();
        Assert.True(taskStops[0].TryGetApprovalRequest(out ApprovalPendingAction? requested));
        Assert.Equal("req-1", requested.RequestId);
        Assert.Equal("send_email", requested.SkillCode);
        Assert.Null(requested.Tool);
        Assert.Equal("john@example.com", ((JsonElement)requested.Arguments["to"]).GetString());
        Assert.False(taskStops[1].TryGetApprovalRequest(out _));

        StreamingAgentMessageStop stop = Assert.Single(messages.OfType<StreamingAgentMessageStop>());
        ApprovalPendingAction pending = Assert.IsType<ApprovalPendingAction>(Assert.Single(stop.Result!.PendingActions));
        Assert.Equal("call-1", pending.CallId);
        Assert.Equal("HttpRequest", pending.SkillType);
        Assert.Equal(ConversationId, conversation.ConversationId);
    }

    [Fact]
    public async Task StreamApprovals_SendsDecisionsAndParsesPendingActionsOnError()
    {
        CapturingHandler handler = new(Sse(
            $$"""{"type":"error","code":"validation_error","message":"Pending approvals","errors":{"tool_approval_pending":"Answer every approval"},"pending_actions":[{{SnakeCaseApproval}}]}"""),
            "text/event-stream");

        Conversation conversation = CreateClient(handler).GetAgents().Copilots.CreateConversation("copilot", ConversationId);
        List<StreamingAgentMessage> messages = await ToListAsync(
            conversation.StreamApprovalsAsync(new[] { new ApprovalPendingAction { RequestId = "req-0" }.Reject() }));

        Assert.True(handler.Parameter("stream")!.Value.GetBoolean());
        Assert.Equal("req-0", handler.Parameter("toolApprovals")!.Value[0].GetProperty("requestId").GetString());
        Assert.Null(handler.Parameter("message"));

        StreamingAgentMessageError error = Assert.Single(messages.OfType<StreamingAgentMessageError>());
        ApprovalPendingAction pending = Assert.IsType<ApprovalPendingAction>(Assert.Single(error.PendingActions!));
        Assert.Equal("req-1", pending.RequestId);
    }
    #endregion

    #region AI Proxy
    [Fact]
    public async Task Proxy_AddApprovalDecision_EchoesRequestAndDecisionInSnakeCase()
    {
        CapturingHandler handler = new("""{"content":"Email sent"}""");

        ProxyExecutionReq options = new()
        {
            Model = "gpt-4o-mini",
            Messages = new List<ProxyExecutionMessage> { new() { Role = "user", Content = "Send an email to John" } }
        };

        ApprovalPendingAction approval = JsonSerializer.Deserialize<PendingAction>(
            """{"type":"approval","requestId":"req-1","callId":"call-1","skillCode":"send_email","skillType":"HttpRequest","arguments":{"to":"john@example.com"}}""",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) as ApprovalPendingAction
            ?? throw new InvalidOperationException("Expected an approval");

        options.AddApprovalDecision(approval, approval.Reject("Wrong recipient"));

        Proxy proxy = CreateClient(handler).GetAgents().AIProxies.Create("proxy", options);
        await proxy.ExecuteAsync();

        JsonElement messages = handler.JsonBody!.Value.GetProperty("messages");
        Assert.Equal(3, messages.GetArrayLength());
        Assert.Equal("Send an email to John", messages[0].GetProperty("content").GetString());
        Assert.False(messages[0].TryGetProperty("tool_approval_request", out _));

        Assert.Equal("assistant", messages[1].GetProperty("role").GetString());
        JsonElement request = messages[1].GetProperty("tool_approval_request");
        Assert.Equal("req-1", request.GetProperty("request_id").GetString());
        Assert.Equal("call-1", request.GetProperty("call_id").GetString());
        Assert.Equal("send_email", request.GetProperty("skill_code").GetString());
        Assert.Equal("HttpRequest", request.GetProperty("skill_type").GetString());
        Assert.Equal("john@example.com", request.GetProperty("arguments").GetProperty("to").GetString());

        Assert.Equal("user", messages[2].GetProperty("role").GetString());
        JsonElement response = messages[2].GetProperty("tool_approval_response");
        Assert.Equal("req-1", response.GetProperty("request_id").GetString());
        Assert.False(response.GetProperty("approved").GetBoolean());
        Assert.Equal("Wrong recipient", response.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task Proxy_Skills_AreSentInSnakeCaseWithTheirApprovalSetting()
    {
        CapturingHandler handler = new("""{"content":"Done"}""");

        ProxyExecutionReq options = new()
        {
            Model = "gpt-4o-mini",
            Messages = new List<ProxyExecutionMessage> { new() { Role = "user", Content = "Draw an apple" } },
            Skills = new ProxySkillsOptions
            {
                ImageGeneration = new ProxySkillsOptions.ImageGenerationSkillOptions { Model = "gpt-image-1", Vendor = "OpenAI", RequiresApproval = true },
                SpeechGeneration = new ProxySkillsOptions.SpeechGenerationSkillOptions { Voice = "alloy", VoiceId = "v-1", Speed = 1.5f },
                Workbench = new ProxySkillsOptions.WorkbenchSkillOptions { RequiresApproval = true }
            }
        };

        await CreateClient(handler).GetAgents().AIProxies.Create("proxy", options).ExecuteAsync();

        JsonElement skills = handler.JsonBody!.Value.GetProperty("skills");
        Assert.False(skills.TryGetProperty("web_search", out _));

        Assert.True(skills.GetProperty("image_generation").GetBoolean());
        JsonElement image = skills.GetProperty("image_generation_options");
        Assert.Equal("gpt-image-1", image.GetProperty("model").GetString());
        Assert.Equal("OpenAI", image.GetProperty("vendor").GetString());
        Assert.True(image.GetProperty("requires_approval").GetBoolean());

        Assert.True(skills.GetProperty("speech_generation").GetBoolean());
        JsonElement speech = skills.GetProperty("speech_generation_options");
        Assert.Equal("alloy", speech.GetProperty("voice").GetString());
        Assert.Equal("v-1", speech.GetProperty("voice_id").GetString());
        Assert.Equal(1.5f, speech.GetProperty("speed").GetSingle());
        Assert.False(speech.GetProperty("requires_approval").GetBoolean());

        Assert.True(skills.GetProperty("workbench").GetBoolean());
        Assert.True(skills.GetProperty("workbench_options").GetProperty("requires_approval").GetBoolean());
    }

    [Fact]
    public async Task Proxy_WithoutSkills_DoesNotSendSkills()
    {
        CapturingHandler handler = new("""{"content":"Done"}""");

        await CreateClient(handler).GetAgents().AIProxies.Create("proxy", new ProxyExecutionReq { Model = "gpt-4o-mini" }).ExecuteAsync();

        Assert.False(handler.JsonBody!.Value.TryGetProperty("skills", out _));
    }

    [Fact]
    public void Proxy_AddApprovalDecision_RejectsDecisionForAnotherRequest()
    {
        ProxyExecutionReq options = new();
        ApprovalPendingAction approval = new() { RequestId = "req-1" };

        Assert.Throws<ArgumentException>(
            () => options.AddApprovalDecision(approval, new ApprovalPendingAction { RequestId = "req-2" }.Approve()));
        Assert.Empty(options.Messages);
    }
    #endregion
}
