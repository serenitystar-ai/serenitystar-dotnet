using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Agents.Conversational;
using SerenityStar.Agents.System;
using SerenityStar.Client;
using SerenityStar.Exceptions;
using SerenityStar.Extensions;
using SerenityStar.Models.AIProxy;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Streaming;
using System.Net;
using Xunit;

namespace SerenityStar.IntegrationTests;

/// <summary>
/// Integration tests for skill approvals. They require agents with at least one skill that requires approval
/// (SerenityStar:AssistantAgent and ApprovalProxyAgent) and a message that makes
/// the agent invoke it (SerenityStar:ApprovalMessage).
/// </summary>
public class SkillApprovalsIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly SerenityClient _client;

    public SkillApprovalsIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<SerenityClient>();
    }

    private static List<ApprovalPendingAction> AssertApprovalsRequested(AgentResult result)
    {
        List<ApprovalPendingAction> approvals = result.PendingActions.OfType<ApprovalPendingAction>().ToList();

        Assert.NotEmpty(approvals);
        Assert.All(approvals, approval =>
        {
            Assert.Equal("approval", approval.Type);
            Assert.False(string.IsNullOrEmpty(approval.RequestId));
            Assert.False(string.IsNullOrEmpty(approval.CallId));
            Assert.False(string.IsNullOrEmpty(approval.SkillCode));
            Assert.False(string.IsNullOrEmpty(approval.SkillType));
            Assert.NotNull(approval.Arguments);
        });

        return approvals;
    }

    private static void AssertApprovalsResolved(AgentResult result, IEnumerable<ApprovalPendingAction> answered)
    {
        // The agent may ask for a new approval after resuming, but never for the ones already answered.
        HashSet<string> pendingRequestIds = result.PendingActions.OfType<ApprovalPendingAction>().Select(a => a.RequestId).ToHashSet();
        Assert.DoesNotContain(answered, approval => pendingRequestIds.Contains(approval.RequestId));
    }

    private async Task<(Conversation Conversation, List<ApprovalPendingAction> Approvals)> StartConversationAwaitingApprovalAsync(ConversationalAgentBase agents, string agentCode)
    {
        Conversation conversation = agents.CreateConversation(agentCode);
        AgentResult result = await conversation.SendMessageAsync(_fixture.ApprovalMessage);

        Assert.NotNull(conversation.ConversationId);
        return (conversation, AssertApprovalsRequested(result));
    }

    #region Assistants
    [Fact]
    public async Task Assistant_SendMessage_InvokingSkillRequiringApproval_ShouldReturnApprovalPendingAction()
    {
        // Act
        (Conversation conversation, List<ApprovalPendingAction> approvals) =
            await StartConversationAwaitingApprovalAsync(_client.GetAgents().Assistants, _fixture.AssistantAgent);

        // Assert
        Assert.NotNull(conversation.ConversationId);
        Assert.NotEmpty(approvals);
    }

    [Fact]
    public async Task Assistant_SendApprovals_Approve_ShouldContinueExecution()
    {
        // Arrange
        (Conversation conversation, List<ApprovalPendingAction> approvals) =
            await StartConversationAwaitingApprovalAsync(_client.GetAgents().Assistants, _fixture.AssistantAgent);
        await Task.Delay(1000);

        // Act
        AgentResult result = await conversation.SendApprovalsAsync(approvals.Select(a => a.Approve()));

        // Assert
        AssertApprovalsResolved(result, approvals);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
    }

    [Fact]
    public async Task Assistant_SendApprovals_RejectWithReason_ShouldContinueExecution()
    {
        // Arrange
        (Conversation conversation, List<ApprovalPendingAction> approvals) =
            await StartConversationAwaitingApprovalAsync(_client.GetAgents().Assistants, _fixture.AssistantAgent);
        await Task.Delay(1000);

        // Act
        AgentResult result = await conversation.SendApprovalsAsync(
            approvals.Select(a => a.Reject("I don't want to run this skill right now")),
            "Just tell me you won't run it.");

        // Assert
        AssertApprovalsResolved(result, approvals);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task Assistant_SendApprovals_LeavingApprovalsUnanswered_ShouldThrowWithPendingActions()
    {
        // Arrange
        (Conversation conversation, List<ApprovalPendingAction> approvals) =
            await StartConversationAwaitingApprovalAsync(_client.GetAgents().Assistants, _fixture.AssistantAgent);
        await Task.Delay(1000);

        ApprovalDecision unrelatedDecision = new ApprovalPendingAction { RequestId = Guid.NewGuid().ToString() }.Approve();

        // Act
        SerenityApiException exception = await Assert.ThrowsAsync<SerenityApiException>(
            () => conversation.SendApprovalsAsync(new[] { unrelatedDecision }));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        List<ApprovalPendingAction> stillPending = exception.GetPendingActions().OfType<ApprovalPendingAction>().ToList();
        Assert.Equal(
            approvals.Select(a => a.RequestId).OrderBy(id => id),
            stillPending.Select(a => a.RequestId).OrderBy(id => id));
    }

    [Fact]
    public async Task Assistant_StreamMessage_InvokingSkillRequiringApproval_ShouldSurfaceApproval()
    {
        // Arrange
        Conversation conversation = _client.GetAgents().Assistants.CreateConversation(_fixture.AssistantAgent);
        List<ApprovalPendingAction> requested = new();
        StreamingAgentMessageStop? stop = null;

        // Act
        await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync(_fixture.ApprovalMessage))
        {
            if (message is StreamingAgentMessageTaskStop taskStop && taskStop.TryGetApprovalRequest(out ApprovalPendingAction? approval))
                requested.Add(approval);
            else if (message is StreamingAgentMessageStop stopMessage)
                stop = stopMessage;
        }

        // Assert
        Assert.NotEmpty(requested);
        Assert.NotNull(stop?.Result);
        List<ApprovalPendingAction> pending = AssertApprovalsRequested(stop!.Result!);
        Assert.Equal(
            requested.Select(a => a.RequestId).OrderBy(id => id),
            pending.Select(a => a.RequestId).OrderBy(id => id));
        Assert.NotNull(conversation.ConversationId);
    }

    [Fact]
    public async Task Assistant_StreamApprovals_Approve_ShouldContinueExecution()
    {
        // Arrange
        (Conversation conversation, List<ApprovalPendingAction> approvals) =
            await StartConversationAwaitingApprovalAsync(_client.GetAgents().Assistants, _fixture.AssistantAgent);
        await Task.Delay(1000);

        List<StreamingAgentMessage> messages = new();

        // Act
        await foreach (StreamingAgentMessage message in conversation.StreamApprovalsAsync(approvals.Select(a => a.Approve())))
            messages.Add(message);

        // Assert
        Assert.Empty(messages.OfType<StreamingAgentMessageError>());
        Assert.Contains(messages.OfType<StreamingAgentMessageTaskStop>(), t => t.TaskKey == StreamingTaskKeys.ApprovalResolve);

        StreamingAgentMessageStop stop = Assert.Single(messages.OfType<StreamingAgentMessageStop>());
        Assert.NotNull(stop.Result);
        AssertApprovalsResolved(stop.Result!, approvals);
    }

    [Fact]
    public async Task Assistant_SendMessage_WithAutoMode_ShouldNotPauseForApproval()
    {
        // Arrange - Without auto mode the same message pauses, so the assertion below can't pass vacuously.
        (_, _) = await StartConversationAwaitingApprovalAsync(_client.GetAgents().Assistants, _fixture.AssistantAgent);
        await Task.Delay(1000);

        Conversation conversation = _client.GetAgents().Assistants.CreateConversation(
            _fixture.AssistantAgent,
            options: new AgentExecutionReq { Skills = new SkillsOptions { AutoMode = true } });

        // Act
        AgentResult result = await conversation.SendMessageAsync(_fixture.ApprovalMessage);

        // Assert
        Assert.Empty(result.PendingActions.OfType<ApprovalPendingAction>());
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task Assistant_StreamMessage_WithAutoMode_ShouldRunSkillWithoutRequestingApproval()
    {
        // Arrange
        Conversation conversation = _client.GetAgents().Assistants.CreateConversation(
            _fixture.AssistantAgent,
            options: new AgentExecutionReq { Skills = new SkillsOptions { AutoMode = true } });

        List<StreamingAgentMessage> messages = new();

        // Act
        await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync(_fixture.ApprovalMessage))
            messages.Add(message);

        // Assert
        List<StreamingAgentMessageTaskStop> taskStops = messages.OfType<StreamingAgentMessageTaskStop>().ToList();
        Assert.DoesNotContain(taskStops, t => t.TaskKey == StreamingTaskKeys.ApprovalRequest);
        Assert.Contains(taskStops, t => t.TaskKey.StartsWith("skills_") && t.TaskKey.EndsWith("_execute"));

        StreamingAgentMessageStop stop = Assert.Single(messages.OfType<StreamingAgentMessageStop>());
        Assert.NotNull(stop.Result);
        Assert.Empty(stop.Result!.PendingActions.OfType<ApprovalPendingAction>());
    }

    [Fact]
    public async Task Assistant_SendMessage_WithAutoModeDisabled_ShouldPauseForApproval()
    {
        // Arrange - Auto mode is opt-in: sending it as false behaves like not sending it.
        Conversation conversation = _client.GetAgents().Assistants.CreateConversation(
            _fixture.AssistantAgent,
            options: new AgentExecutionReq { Skills = new SkillsOptions { AutoMode = false } });

        // Act
        AgentResult result = await conversation.SendMessageAsync(_fixture.ApprovalMessage);

        // Assert
        AssertApprovalsRequested(result);
    }
    #endregion

    #region AI Proxy
    private ProxyExecutionReq CreateProxyOptions() => new()
    {
        Model = "gpt-4o-mini",
        Vendor = "OpenAI",
        Messages = new List<ProxyExecutionMessage>
        {
            new() { Role = "user", Content = _fixture.ApprovalMessage }
        },
        // AI Proxy skills are sent on each request, so the same options must be kept when resuming.
        Skills = new ProxySkillsOptions
        {
            ImageGeneration = new ProxySkillsOptions.ImageGenerationSkillOptions { RequiresApproval = true }
        }
    };

    [Fact]
    public async Task Proxy_AddApprovalDecision_Approve_ShouldContinueExecution()
    {
        // Arrange
        ProxyExecutionReq options = CreateProxyOptions();
        Proxy proxy = _client.GetAgents().AIProxies.Create(_fixture.ApprovalProxyAgent, options);

        List<ApprovalPendingAction> approvals = AssertApprovalsRequested(await proxy.ExecuteAsync());
        await Task.Delay(1000);

        foreach (ApprovalPendingAction approval in approvals)
            options.AddApprovalDecision(approval, approval.Approve());

        // Act
        AgentResult result = await proxy.ExecuteAsync();

        // Assert
        AssertApprovalsResolved(result, approvals);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
    }

    [Fact]
    public async Task Proxy_AddApprovalDecision_RejectWithReason_ShouldContinueStreaming()
    {
        // Arrange
        ProxyExecutionReq options = CreateProxyOptions();
        Proxy proxy = _client.GetAgents().AIProxies.Create(_fixture.ApprovalProxyAgent, options);

        List<ApprovalPendingAction> approvals = AssertApprovalsRequested(await proxy.ExecuteAsync());
        await Task.Delay(1000);

        foreach (ApprovalPendingAction approval in approvals)
            options.AddApprovalDecision(approval, approval.Reject("I don't want to run this skill right now"));

        List<StreamingAgentMessage> messages = new();

        // Act
        await foreach (StreamingAgentMessage message in proxy.StreamAsync())
            messages.Add(message);

        // Assert
        Assert.Empty(messages.OfType<StreamingAgentMessageError>());
        StreamingAgentMessageStop stop = Assert.Single(messages.OfType<StreamingAgentMessageStop>());
        Assert.NotNull(stop.Result);
        AssertApprovalsResolved(stop.Result!, approvals);
    }
    #endregion
}
