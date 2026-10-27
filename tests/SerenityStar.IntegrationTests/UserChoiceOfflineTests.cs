using SerenityStar.Agents.Conversational;
using SerenityStar.Agents.System;
using SerenityStar.Client;
using SerenityStar.Models.AIProxy;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Streaming;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SerenityStar.IntegrationTests;

/// <summary>
/// Offline tests for Request User Choice. These use a fake <see cref="HttpMessageHandler"/> to capture the outgoing
/// request and return a canned response, so they do not require an API key or network access.
/// </summary>
public class UserChoiceOfflineTests
{
    private const string BaseUrl = "https://offline.example.com";
    private const string ConversationId = "11111111-1111-1111-1111-111111111111";

    private const string CamelCaseUserChoice = """
        {"type":"user_choice","questions":[
          {"id":"q1","header":"Destination","text":"Where do you want to go?","isMultiselect":false,"options":[
            {"id":"q1-o1","title":"Beach","description":"Sun and sand"},{"id":"q1-o2","title":"Mountain"}]},
          {"id":"q2","header":"Activities","text":"What do you want to do?","isMultiselect":true,"options":[
            {"id":"q2-o1","title":"Hiking"},{"id":"q2-o2","title":"Museums"},{"id":"q2-o3","title":"Food tours"}]}]}
        """;

    private const string SnakeCaseUserChoice = """{"type":"user_choice","questions":[{"id":"q1","header":"Destination","text":"Where do you want to go?","is_multiselect":true,"options":[{"id":"q1-o1","title":"Beach","description":"Sun and sand"}]}]}""";

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
        public JsonElement? JsonBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;

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

    private static UserChoicePendingAction CreateUserChoice() => new()
    {
        Questions = new List<UserChoiceQuestion>
        {
            new()
            {
                Id = "q1",
                Header = "Destination",
                Text = "Where do you want to go?",
                Options = new List<UserChoiceOption> { new() { Id = "q1-o1", Title = "Beach" }, new() { Id = "q1-o2", Title = "Mountain" } }
            },
            new()
            {
                Id = "q2",
                Text = "What do you want to do?",
                IsMultiselect = true,
                Options = new List<UserChoiceOption> { new() { Id = "q2-o1", Title = "Hiking" }, new() { Id = "q2-o2", Title = "Museums" } }
            }
        }
    };

    private static Conversation CreateStartedConversation(CapturingHandler handler) =>
        CreateClient(handler).GetAgents().Assistants.CreateConversation("assistant", ConversationId);

    #region Pending action
    [Fact]
    public async Task SendMessage_ParsesUserChoicePendingAction()
    {
        CapturingHandler handler = new($$$"""{"content":"Let me ask you first.","instanceId":"{{{ConversationId}}}","pendingActions":[{{{CamelCaseUserChoice}}}]}""");

        Conversation conversation = CreateClient(handler).GetAgents().Assistants.CreateConversation("assistant");
        AgentResult result = await conversation.SendMessageAsync("Plan a trip");

        UserChoicePendingAction userChoice = Assert.IsType<UserChoicePendingAction>(Assert.Single(result.PendingActions));
        Assert.Equal("user_choice", userChoice.Type);
        Assert.Equal(2, userChoice.Questions.Count);

        UserChoiceQuestion destination = userChoice.Questions[0];
        Assert.Equal("q1", destination.Id);
        Assert.Equal("Destination", destination.Header);
        Assert.Equal("Where do you want to go?", destination.Text);
        Assert.False(destination.IsMultiselect);
        Assert.Equal(new[] { "q1-o1", "q1-o2" }, destination.Options.Select(o => o.Id));
        Assert.Equal("Beach", destination.Options[0].Title);
        Assert.Equal("Sun and sand", destination.Options[0].Description);
        Assert.Null(destination.Options[1].Description);

        Assert.True(userChoice.Questions[1].IsMultiselect);
        Assert.Equal(ConversationId, conversation.ConversationId);
    }

    [Fact]
    public async Task Stream_SurfacesUserChoiceFromTaskStopAndStopEvents()
    {
        CapturingHandler handler = new(Sse(
            $$$"""{"type":"start","instance_id":"{{{ConversationId}}}"}""",
            $$$"""{"type":"task_stop","task":"Requesting user choice","task_key":"{{{StreamingTaskKeys.UserChoiceRequest}}}","output":{{{SnakeCaseUserChoice}}},"success":true}""",
            """{"type":"task_stop","task":"Other task","task_key":"llm_response","output":{"text":"ignored"},"success":true}""",
            $$$"""{"type":"stop","result":{"content":"","instance_id":"{{{ConversationId}}}","pending_actions":[{{{SnakeCaseUserChoice}}}]}}"""),
            "text/event-stream");

        Conversation conversation = CreateClient(handler).GetAgents().Assistants.CreateConversation("assistant");
        List<StreamingAgentMessage> messages = await ToListAsync(conversation.StreamMessageAsync("Plan a trip"));

        List<StreamingAgentMessageTaskStop> taskStops = messages.OfType<StreamingAgentMessageTaskStop>().ToList();
        Assert.True(taskStops[0].TryGetUserChoiceRequest(out UserChoicePendingAction? requested));
        Assert.False(taskStops[1].TryGetUserChoiceRequest(out UserChoicePendingAction? notRequested));
        Assert.Null(notRequested);
        Assert.False(taskStops[0].TryGetApprovalRequest(out _));

        UserChoiceQuestion question = Assert.Single(requested!.Questions);
        Assert.Equal("q1", question.Id);
        Assert.True(question.IsMultiselect);
        Assert.Equal("Sun and sand", Assert.Single(question.Options).Description);

        StreamingAgentMessageStop stop = Assert.Single(messages.OfType<StreamingAgentMessageStop>());
        UserChoicePendingAction pending = Assert.IsType<UserChoicePendingAction>(Assert.Single(stop.Result!.PendingActions));
        Assert.True(Assert.Single(pending.Questions).IsMultiselect);
        Assert.Equal(ConversationId, conversation.ConversationId);
    }
    #endregion

    #region Question helpers
    [Fact]
    public void Select_SingleOption_CreatesAnswerForTheQuestion()
    {
        UserChoiceAnswer answer = CreateUserChoice().Questions[0].Select("q1-o2");

        Assert.Equal("q1", answer.QuestionId);
        Assert.Equal(new[] { "q1-o2" }, answer.SelectedOptionIds);
        Assert.Null(answer.Other);
    }

    [Fact]
    public void Select_SeveralOptionsOnMultiselect_KeepsEachOptionOnce()
    {
        UserChoiceAnswer answer = CreateUserChoice().Questions[1].Select("q2-o1", "q2-o2", "q2-o1");

        Assert.Equal(new[] { "q2-o1", "q2-o2" }, answer.SelectedOptionIds);
    }

    [Fact]
    public void Select_SeveralOptionsOnSingleSelect_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreateUserChoice().Questions[0].Select("q1-o1", "q1-o2"));
    }

    [Fact]
    public void Select_UnknownOption_Throws()
    {
        // Options belong to their question: q2's option can't answer q1.
        Assert.Throws<ArgumentException>(() => CreateUserChoice().Questions[0].Select("q2-o1"));
    }

    [Fact]
    public void Select_NoOptions_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreateUserChoice().Questions[0].Select());
    }

    [Fact]
    public void AnswerOther_CreatesFreeTextAnswerWithoutOptions()
    {
        UserChoiceAnswer answer = CreateUserChoice().Questions[0].AnswerOther("Somewhere with snow");

        Assert.Equal("q1", answer.QuestionId);
        Assert.Empty(answer.SelectedOptionIds);
        Assert.Equal("Somewhere with snow", answer.Other);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AnswerOther_EmptyText_Throws(string other)
    {
        Assert.Throws<ArgumentException>(() => CreateUserChoice().Questions[0].AnswerOther(other));
    }

    [Fact]
    public void Answer_OptionsAndFreeText_CreatesCombinedAnswer()
    {
        UserChoiceAnswer answer = CreateUserChoice().Questions[1].Answer(new[] { "q2-o2" }, "And a cooking class");

        Assert.Equal(new[] { "q2-o2" }, answer.SelectedOptionIds);
        Assert.Equal("And a cooking class", answer.Other);
    }

    [Fact]
    public void Answer_NeitherOptionsNorText_Throws()
    {
        UserChoiceQuestion question = CreateUserChoice().Questions[0];

        Assert.Throws<ArgumentException>(() => question.Answer(null, null));
        Assert.Throws<ArgumentException>(() => question.Answer(Array.Empty<string>(), " "));
    }
    #endregion

    #region Conversational
    [Fact]
    public async Task SendUserChoices_AllQuestionsAnswered_SendsEveryAnswerWithChatId()
    {
        CapturingHandler handler = new($$$"""{"content":"Great choice!","instanceId":"{{{ConversationId}}}"}""");
        Conversation conversation = CreateStartedConversation(handler);
        UserChoicePendingAction userChoice = CreateUserChoice();

        AgentResult result = await conversation.SendUserChoicesAsync(new[]
        {
            userChoice.Questions[0].Select("q1-o1"),
            userChoice.Questions[1].Answer(new[] { "q2-o1", "q2-o2" }, "Also wine tasting")
        });

        Assert.Equal("Great choice!", result.Content);
        Assert.Equal(ConversationId, handler.Parameter("chatId")!.Value.GetString());
        Assert.Null(handler.Parameter("message"));

        JsonElement answers = handler.Parameter("userChoiceResponses")!.Value;
        Assert.Equal(2, answers.GetArrayLength());
        Assert.Equal("q1", answers[0].GetProperty("questionId").GetString());
        Assert.Equal("q1-o1", answers[0].GetProperty("selectedOptionIds")[0].GetString());
        Assert.Equal(JsonValueKind.Null, answers[0].GetProperty("other").ValueKind);
        Assert.Equal("q2", answers[1].GetProperty("questionId").GetString());
        Assert.Equal(2, answers[1].GetProperty("selectedOptionIds").GetArrayLength());
        Assert.Equal("Also wine tasting", answers[1].GetProperty("other").GetString());
    }

    [Fact]
    public async Task SendUserChoices_SomeQuestionsAnsweredWithMessage_SendsOnlyThoseAnswersAndTheMessage()
    {
        CapturingHandler handler = new($$$"""{"content":"ok","instanceId":"{{{ConversationId}}}"}""");
        Conversation conversation = CreateStartedConversation(handler);

        await conversation.SendUserChoicesAsync(new[] { CreateUserChoice().Questions[1].AnswerOther("Surfing") }, "Any destination is fine");

        Assert.Equal("Any destination is fine", handler.Parameter("message")!.Value.GetString());
        JsonElement answer = Assert.Single(handler.Parameter("userChoiceResponses")!.Value.EnumerateArray());
        Assert.Equal("q2", answer.GetProperty("questionId").GetString());
        Assert.Equal(0, answer.GetProperty("selectedOptionIds").GetArrayLength());
        Assert.Equal("Surfing", answer.GetProperty("other").GetString());
    }

    [Fact]
    public async Task SendUserChoices_NoAnswersWithMessage_SendsARegularMessage()
    {
        CapturingHandler handler = new($$$"""{"content":"ok","instanceId":"{{{ConversationId}}}"}""");
        Conversation conversation = CreateStartedConversation(handler);

        await conversation.SendUserChoicesAsync(Array.Empty<UserChoiceAnswer>(), "Just pick for me");

        Assert.Equal("Just pick for me", handler.Parameter("message")!.Value.GetString());
        Assert.Null(handler.Parameter("userChoiceResponses"));
    }

    [Fact]
    public async Task SendUserChoices_NoAnswersAndNoMessage_ThrowsWithoutCallingTheApi()
    {
        CapturingHandler handler = new("{}");
        Conversation conversation = CreateStartedConversation(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => conversation.SendUserChoicesAsync(Array.Empty<UserChoiceAnswer>(), " "));
        await Assert.ThrowsAsync<ArgumentNullException>(() => conversation.SendUserChoicesAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(() => conversation.SendUserChoicesAsync(new[] { new UserChoiceAnswer() }));
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task SendUserChoices_BeforeTheConversationStarted_ThrowsWithoutCallingTheApi()
    {
        CapturingHandler handler = new("{}");
        Conversation conversation = CreateClient(handler).GetAgents().Assistants.CreateConversation("assistant");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => conversation.SendUserChoicesAsync(new[] { CreateUserChoice().Questions[0].Select("q1-o1") }));
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task SendUserChoices_OnCopilot_SendsTheAnswers()
    {
        CapturingHandler handler = new($$$"""{"content":"ok","instanceId":"{{{ConversationId}}}"}""");
        Conversation conversation = CreateClient(handler).GetAgents().Copilots.CreateConversation("copilot", ConversationId);

        await conversation.SendUserChoicesAsync(new[] { CreateUserChoice().Questions[0].Select("q1-o2") });

        Assert.Equal("q1-o2", handler.Parameter("userChoiceResponses")!.Value[0].GetProperty("selectedOptionIds")[0].GetString());
    }

    [Fact]
    public async Task StreamUserChoices_SendsAnswersAndStreamsTheContinuedConversation()
    {
        CapturingHandler handler = new(Sse(
            """{"type":"content","text":"Beach it is!"}""",
            $$$"""{"type":"stop","result":{"content":"Beach it is!","instance_id":"{{{ConversationId}}}"}}"""),
            "text/event-stream");
        Conversation conversation = CreateStartedConversation(handler);

        List<StreamingAgentMessage> messages = await ToListAsync(
            conversation.StreamUserChoicesAsync(new[] { CreateUserChoice().Questions[0].Select("q1-o1") }));

        Assert.True(handler.Parameter("stream")!.Value.GetBoolean());
        Assert.Equal("q1", handler.Parameter("userChoiceResponses")!.Value[0].GetProperty("questionId").GetString());
        Assert.Equal("Beach it is!", Assert.Single(messages.OfType<StreamingAgentMessageContent>()).Text);
        Assert.Empty(Assert.Single(messages.OfType<StreamingAgentMessageStop>()).Result!.PendingActions);
    }

    [Fact]
    public void StreamUserChoices_InvalidInput_ThrowsOnTheCall()
    {
        CapturingHandler handler = new("{}");
        Conversation conversation = CreateStartedConversation(handler);

        // Validation happens before enumerating, so the misuse surfaces where the method is called.
        Assert.Throws<ArgumentException>(() => conversation.StreamUserChoicesAsync(Array.Empty<UserChoiceAnswer>()));
        Assert.Equal(0, handler.RequestCount);
    }
    #endregion

    #region AI Proxy
    private static ProxyExecutionReq CreateProxyOptions() => new()
    {
        Model = "gpt-4o-mini",
        Messages = new List<ProxyExecutionMessage> { new() { Role = "user", Content = "Plan a trip" } },
        Skills = new ProxySkillsOptions { UserChoice = true }
    };

    [Fact]
    public async Task Proxy_UserChoiceSkill_IsEnabledInSnakeCase()
    {
        CapturingHandler handler = new($$$"""{"content":"","instanceId":"{{{ConversationId}}}","pendingActions":[{{{CamelCaseUserChoice}}}]}""");
        Proxy proxy = CreateClient(handler).GetAgents().AIProxies.Create("proxy", CreateProxyOptions());

        AgentResult result = await proxy.ExecuteAsync();

        Assert.True(handler.JsonBody!.Value.GetProperty("skills").GetProperty("user_choice").GetBoolean());
        Assert.Equal(2, Assert.IsType<UserChoicePendingAction>(Assert.Single(result.PendingActions)).Questions.Count);
    }

    [Fact]
    public async Task Proxy_UserChoiceSkillDisabled_IsNotSent()
    {
        CapturingHandler handler = new($$$"""{"content":"ok","instanceId":"{{{ConversationId}}}"}""");
        ProxyExecutionReq options = CreateProxyOptions();
        options.Skills = new ProxySkillsOptions();

        await CreateClient(handler).GetAgents().AIProxies.Create("proxy", options).ExecuteAsync();

        Assert.False(handler.JsonBody!.Value.GetProperty("skills").TryGetProperty("user_choice", out _));
    }

    [Fact]
    public async Task Proxy_AddUserChoiceAnswers_EchoesQuestionsAndAnswersInSnakeCase()
    {
        CapturingHandler handler = new($$$"""{"content":"ok","instanceId":"{{{ConversationId}}}"}""");
        ProxyExecutionReq options = CreateProxyOptions();
        UserChoicePendingAction userChoice = CreateUserChoice();

        options.AddUserChoiceAnswers(userChoice, new[]
        {
            userChoice.Questions[0].Select("q1-o2"),
            userChoice.Questions[1].Answer(new[] { "q2-o1" }, "Also kayaking")
        });

        await CreateClient(handler).GetAgents().AIProxies.Create("proxy", options).ExecuteAsync();

        JsonElement messages = handler.JsonBody!.Value.GetProperty("messages");
        Assert.Equal(3, messages.GetArrayLength());

        JsonElement assistant = messages[1];
        Assert.Equal("assistant", assistant.GetProperty("role").GetString());
        JsonElement question = assistant.GetProperty("user_choice_request").GetProperty("questions")[1];
        Assert.Equal("q2", question.GetProperty("id").GetString());
        Assert.True(question.GetProperty("is_multiselect").GetBoolean());
        Assert.Equal("Hiking", question.GetProperty("options")[0].GetProperty("title").GetString());
        Assert.False(assistant.TryGetProperty("user_choice_response", out _));

        JsonElement user = messages[2];
        Assert.Equal("user", user.GetProperty("role").GetString());
        Assert.Equal(string.Empty, user.GetProperty("content").GetString());
        JsonElement answers = user.GetProperty("user_choice_response");
        Assert.Equal("q1", answers[0].GetProperty("question_id").GetString());
        Assert.Equal("q1-o2", answers[0].GetProperty("selected_option_ids")[0].GetString());
        Assert.Equal("Also kayaking", answers[1].GetProperty("other").GetString());
        Assert.False(user.TryGetProperty("user_choice_request", out _));
    }

    [Fact]
    public void Proxy_AddUserChoiceAnswers_SomeAnsweredWithMessage_AddsMessageContent()
    {
        ProxyExecutionReq options = CreateProxyOptions();
        UserChoicePendingAction userChoice = CreateUserChoice();

        options.AddUserChoiceAnswers(userChoice, new[] { userChoice.Questions[0].Select("q1-o1") }, "Surprise me with the activities");

        ProxyExecutionMessage user = options.Messages[^1];
        Assert.Equal("Surprise me with the activities", user.Content);
        Assert.Equal("q1", Assert.Single(user.UserChoiceResponse!).QuestionId);
    }

    [Fact]
    public async Task Proxy_AddUserChoiceAnswers_NoAnswersWithMessage_SendsOnlyTheMessageAfterTheQuestions()
    {
        CapturingHandler handler = new($$$"""{"content":"ok","instanceId":"{{{ConversationId}}}"}""");
        ProxyExecutionReq options = CreateProxyOptions();

        options.AddUserChoiceAnswers(CreateUserChoice(), Array.Empty<UserChoiceAnswer>(), "Just pick for me");
        await CreateClient(handler).GetAgents().AIProxies.Create("proxy", options).ExecuteAsync();

        JsonElement user = handler.JsonBody!.Value.GetProperty("messages")[2];
        Assert.Equal("Just pick for me", user.GetProperty("content").GetString());
        Assert.False(user.TryGetProperty("user_choice_response", out _));
    }

    [Fact]
    public void Proxy_AddUserChoiceAnswers_InvalidInput_ThrowsWithoutChangingTheMessages()
    {
        ProxyExecutionReq options = CreateProxyOptions();
        UserChoicePendingAction userChoice = CreateUserChoice();
        UserChoiceAnswer foreignAnswer = new() { QuestionId = "unknown", SelectedOptionIds = new List<string> { "x" } };

        Assert.Throws<ArgumentNullException>(() => options.AddUserChoiceAnswers(null!, Array.Empty<UserChoiceAnswer>(), "hi"));
        Assert.Throws<ArgumentNullException>(() => options.AddUserChoiceAnswers(userChoice, null!));
        Assert.Throws<ArgumentException>(() => options.AddUserChoiceAnswers(userChoice, Array.Empty<UserChoiceAnswer>()));
        Assert.Throws<ArgumentException>(() => options.AddUserChoiceAnswers(userChoice, new[] { foreignAnswer }));
        Assert.Single(options.Messages);
    }
    #endregion
}
