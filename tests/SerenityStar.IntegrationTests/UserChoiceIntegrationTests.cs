using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Agents.Conversational;
using SerenityStar.Agents.System;
using SerenityStar.Client;
using SerenityStar.Models.AIProxy;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Streaming;
using Xunit;

namespace SerenityStar.IntegrationTests;

/// <summary>
/// Integration tests for Request User Choice. They require an assistant agent with the skill enabled
/// (SerenityStar:UserChoiceAssistantAgent), a proxy agent (UserChoiceProxyAgent, which enables
/// the skill per request) and a message that makes the agent ask the user to choose (SerenityStar:UserChoiceMessage).
/// </summary>
public class UserChoiceIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly SerenityClient _client;

    public UserChoiceIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<SerenityClient>();
    }

    private static UserChoicePendingAction AssertUserChoiceRequested(AgentResult result)
    {
        UserChoicePendingAction userChoice = Assert.Single(result.PendingActions.OfType<UserChoicePendingAction>());

        Assert.Equal("user_choice", userChoice.Type);
        Assert.NotEmpty(userChoice.Questions);
        Assert.All(userChoice.Questions, question =>
        {
            Assert.False(string.IsNullOrEmpty(question.Id));
            Assert.False(string.IsNullOrEmpty(question.Text));
            Assert.NotEmpty(question.Options);
            Assert.All(question.Options, option =>
            {
                Assert.False(string.IsNullOrEmpty(option.Id));
                Assert.False(string.IsNullOrEmpty(option.Title));
            });
        });

        return userChoice;
    }

    /// <summary>
    /// Answers every question with its first option.
    /// </summary>
    private static List<UserChoiceAnswer> SelectFirstOptions(UserChoicePendingAction userChoice)
        => userChoice.Questions.Select(q => q.Select(q.Options[0].Id)).ToList();

    private static void AssertContinued(AgentResult result, Conversation conversation)
    {
        // The agent may ask something new, but the conversation must continue on the same instance.
        Assert.Equal(conversation.ConversationId, result.InstanceId.ToString());
        Assert.True(!string.IsNullOrEmpty(result.Content) || result.PendingActions.Count > 0);
    }

    private async Task<(Conversation Conversation, UserChoicePendingAction UserChoice)> StartConversationAwaitingChoiceAsync(ConversationalAgentBase agents, string agentCode)
    {
        Conversation conversation = agents.CreateConversation(agentCode);
        AgentResult result = await conversation.SendMessageAsync(_fixture.UserChoiceMessage);

        Assert.NotNull(conversation.ConversationId);
        UserChoicePendingAction userChoice = AssertUserChoiceRequested(result);

        await Task.Delay(1000);
        return (conversation, userChoice);
    }

    #region Assistants
    [Fact]
    public async Task Assistant_SendMessage_AskingToChoose_ShouldReturnUserChoicePendingAction()
    {
        // Act
        (Conversation conversation, UserChoicePendingAction userChoice) =
            await StartConversationAwaitingChoiceAsync(_client.GetAgents().Assistants, _fixture.UserChoiceAssistantAgent);

        // Assert
        Assert.NotNull(conversation.ConversationId);
        Assert.NotEmpty(userChoice.Questions);
    }

    [Fact]
    public async Task Assistant_SendUserChoices_AllQuestionsAnswered_ShouldContinueConversation()
    {
        // Arrange
        (Conversation conversation, UserChoicePendingAction userChoice) =
            await StartConversationAwaitingChoiceAsync(_client.GetAgents().Assistants, _fixture.UserChoiceAssistantAgent);

        // Act
        AgentResult result = await conversation.SendUserChoicesAsync(SelectFirstOptions(userChoice));

        // Assert
        AssertContinued(result, conversation);
    }

    [Fact]
    public async Task Assistant_SendUserChoices_SomeQuestionsAnsweredWithMessage_ShouldContinueConversation()
    {
        // Arrange
        (Conversation conversation, UserChoicePendingAction userChoice) =
            await StartConversationAwaitingChoiceAsync(_client.GetAgents().Assistants, _fixture.UserChoiceAssistantAgent);

        UserChoiceQuestion first = userChoice.Questions[0];

        // Act
        AgentResult result = await conversation.SendUserChoicesAsync(
            new[] { first.Select(first.Options[0].Id) },
            "Choose the rest for me.");

        // Assert
        AssertContinued(result, conversation);
    }

    [Fact]
    public async Task Assistant_SendUserChoices_NoAnswersWithMessage_ShouldContinueConversation()
    {
        // Arrange
        (Conversation conversation, _) =
            await StartConversationAwaitingChoiceAsync(_client.GetAgents().Assistants, _fixture.UserChoiceAssistantAgent);

        // Act
        AgentResult result = await conversation.SendUserChoicesAsync(
            Array.Empty<UserChoiceAnswer>(),
            "I'd rather not choose, just give me a general suggestion.");

        // Assert
        AssertContinued(result, conversation);
    }

    [Fact]
    public async Task Assistant_SendUserChoices_FreeTextAndCombinedAnswers_ShouldContinueConversation()
    {
        // Arrange
        (Conversation conversation, UserChoicePendingAction userChoice) =
            await StartConversationAwaitingChoiceAsync(_client.GetAgents().Assistants, _fixture.UserChoiceAssistantAgent);

        List<UserChoiceAnswer> answers = userChoice.Questions
            .Select((q, index) => index == 0
                ? q.AnswerOther("Somewhere with snow")
                : q.Answer(new[] { q.Options[0].Id }, "Flexible on this one"))
            .ToList();

        // Act
        AgentResult result = await conversation.SendUserChoicesAsync(answers);

        // Assert
        AssertContinued(result, conversation);
    }

    [Fact]
    public async Task Assistant_StreamMessage_AskingToChoose_ShouldSurfaceUserChoice()
    {
        // Arrange
        Conversation conversation = _client.GetAgents().Assistants.CreateConversation(_fixture.UserChoiceAssistantAgent);
        List<UserChoicePendingAction> requested = new();
        StreamingAgentMessageStop? stop = null;

        // Act
        await foreach (StreamingAgentMessage message in conversation.StreamMessageAsync(_fixture.UserChoiceMessage))
        {
            if (message is StreamingAgentMessageTaskStop taskStop && taskStop.TryGetUserChoiceRequest(out UserChoicePendingAction? userChoice))
                requested.Add(userChoice);
            else if (message is StreamingAgentMessageStop stopMessage)
                stop = stopMessage;
        }

        // Assert
        UserChoicePendingAction fromTask = Assert.Single(requested);
        Assert.NotNull(stop?.Result);
        UserChoicePendingAction pending = AssertUserChoiceRequested(stop!.Result!);
        Assert.Equal(fromTask.Questions.Select(q => q.Id), pending.Questions.Select(q => q.Id));
        Assert.NotNull(conversation.ConversationId);
    }

    [Fact]
    public async Task Assistant_StreamUserChoices_AllQuestionsAnswered_ShouldContinueConversation()
    {
        // Arrange
        (Conversation conversation, UserChoicePendingAction userChoice) =
            await StartConversationAwaitingChoiceAsync(_client.GetAgents().Assistants, _fixture.UserChoiceAssistantAgent);

        List<StreamingAgentMessage> messages = new();

        // Act
        await foreach (StreamingAgentMessage message in conversation.StreamUserChoicesAsync(SelectFirstOptions(userChoice)))
            messages.Add(message);

        // Assert
        Assert.Empty(messages.OfType<StreamingAgentMessageError>());
        StreamingAgentMessageStop stop = Assert.Single(messages.OfType<StreamingAgentMessageStop>());
        Assert.NotNull(stop.Result);
        Assert.Equal(conversation.ConversationId, stop.Result!.InstanceId.ToString());
    }
    #endregion

    #region AI Proxy
    private ProxyExecutionReq CreateProxyOptions() => new()
    {
        Model = "gpt-4o-mini",
        Vendor = "OpenAI",
        Messages = new List<ProxyExecutionMessage>
        {
            new() { Role = "user", Content = _fixture.UserChoiceMessage }
        },
        // AI Proxy skills are sent on each request, so the same options must be kept when continuing.
        Skills = new ProxySkillsOptions { UserChoice = true }
    };

    private async Task<(ProxyExecutionReq Options, Proxy Proxy, UserChoicePendingAction UserChoice)> StartProxyAwaitingChoiceAsync()
    {
        ProxyExecutionReq options = CreateProxyOptions();
        Proxy proxy = _client.GetAgents().AIProxies.Create(_fixture.UserChoiceProxyAgent, options);

        UserChoicePendingAction userChoice = AssertUserChoiceRequested(await proxy.ExecuteAsync());

        await Task.Delay(1000);
        return (options, proxy, userChoice);
    }

    [Fact]
    public async Task Proxy_AddUserChoiceAnswers_AllQuestionsAnswered_ShouldContinueExecution()
    {
        // Arrange
        (ProxyExecutionReq options, Proxy proxy, UserChoicePendingAction userChoice) = await StartProxyAwaitingChoiceAsync();
        options.AddUserChoiceAnswers(userChoice, SelectFirstOptions(userChoice));

        // Act
        AgentResult result = await proxy.ExecuteAsync();

        // Assert
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        Assert.True(!string.IsNullOrEmpty(result.Content) || result.PendingActions.Count > 0);
    }

    [Fact]
    public async Task Proxy_AddUserChoiceAnswers_SomeQuestionsAnsweredWithMessage_ShouldContinueExecution()
    {
        // Arrange
        (ProxyExecutionReq options, Proxy proxy, UserChoicePendingAction userChoice) = await StartProxyAwaitingChoiceAsync();
        UserChoiceQuestion first = userChoice.Questions[0];
        options.AddUserChoiceAnswers(userChoice, new[] { first.AnswerOther("Somewhere with snow") }, "Choose the rest for me.");

        // Act
        AgentResult result = await proxy.ExecuteAsync();

        // Assert
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        Assert.True(!string.IsNullOrEmpty(result.Content) || result.PendingActions.Count > 0);
    }

    [Fact]
    public async Task Proxy_AddUserChoiceAnswers_NoAnswersWithMessage_ShouldContinueStreaming()
    {
        // Arrange
        (ProxyExecutionReq options, Proxy proxy, UserChoicePendingAction userChoice) = await StartProxyAwaitingChoiceAsync();
        options.AddUserChoiceAnswers(userChoice, Array.Empty<UserChoiceAnswer>(), "I'd rather not choose, just give me a general suggestion.");

        List<StreamingAgentMessage> messages = new();

        // Act
        await foreach (StreamingAgentMessage message in proxy.StreamAsync())
            messages.Add(message);

        // Assert
        Assert.Empty(messages.OfType<StreamingAgentMessageError>());
        StreamingAgentMessageStop stop = Assert.Single(messages.OfType<StreamingAgentMessageStop>());
        Assert.NotNull(stop.Result);
    }

    [Fact]
    public async Task Proxy_StreamAsync_AskingToChoose_ShouldSurfaceUserChoice()
    {
        // Arrange
        Proxy proxy = _client.GetAgents().AIProxies.Create(_fixture.UserChoiceProxyAgent, CreateProxyOptions());
        List<UserChoicePendingAction> requested = new();
        StreamingAgentMessageStop? stop = null;

        // Act
        await foreach (StreamingAgentMessage message in proxy.StreamAsync())
        {
            if (message is StreamingAgentMessageTaskStop taskStop && taskStop.TryGetUserChoiceRequest(out UserChoicePendingAction? userChoice))
                requested.Add(userChoice);
            else if (message is StreamingAgentMessageStop stopMessage)
                stop = stopMessage;
        }

        // Assert
        UserChoicePendingAction fromTask = Assert.Single(requested);
        Assert.NotNull(stop?.Result);
        UserChoicePendingAction pending = AssertUserChoiceRequested(stop!.Result!);
        Assert.Equal(fromTask.Questions.Select(q => q.Id), pending.Questions.Select(q => q.Id));
    }
    #endregion
}
