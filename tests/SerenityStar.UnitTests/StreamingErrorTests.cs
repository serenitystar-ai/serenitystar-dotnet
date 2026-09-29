using System.Net;
using SerenityStar.Client;
using SerenityStar.Errors;
using SerenityStar.Errors.Constants;
using SerenityStar.Errors.Models;
using SerenityStar.Models.Streaming;
using Xunit;

namespace SerenityStar.UnitTests;

/// <summary>
/// Errors on streaming calls: thrown as <see cref="SerenityApiException"/> before the stream opens, and yielded as
/// <see cref="StreamingAgentMessageError"/> after. The payloads come from the API error reference.
/// </summary>
public class StreamingErrorTests
{
    [Fact]
    public async Task AgentExecutionFailed_ReadsSnakeCasePayload()
    {
        StreamingAgentMessageError message = await StreamErrorAsync("""
            {"type":"error","code":"agent_execution_failed","message":"All retry attempts with the base and fallback model have failed.","documentation_url":"https://docs.serenitystar.ai/#agent_execution_failed","errors":{"vendor_error":"fallback detail"},"attempts":[{"index":0,"model_type":"main","code":"vendor_rate_limit_error","status_code":429,"message":"main failed","errors":{"vendor_error":"main detail"}},{"index":1,"model_type":"fallback","code":"vendor_rate_limit_error","status_code":429,"message":"fallback failed","errors":{"vendor_error":"fallback detail"}}],"retry_after_seconds":30}
            """);

        Assert.Equal("All retry attempts with the base and fallback model have failed.", message.Message);

        AgentExecutionFailedError error = Assert.IsType<AgentExecutionFailedError>(message.Error);
        Assert.Equal(SerenityErrorCodes.AgentExecutionFailed, error.Code);
        Assert.Equal("https://docs.serenitystar.ai/#agent_execution_failed", error.DocumentationUrl);
        Assert.Equal(TimeSpan.FromSeconds(30), error.RetryAfter);
        Assert.Equal("fallback detail", error.Errors![VendorErrorCodes.VendorError]);

        // "type" and "retry_after_seconds" are modelled elsewhere, so nothing is left over.
        Assert.Null(error.ExtensionData);

        Assert.Collection(error.Attempts,
            main =>
            {
                Assert.Equal(AgentModelTypes.Main, main.ModelType);
                Assert.Equal(429, main.StatusCode);
                Assert.Equal("main detail", main.VendorError);
            },
            fallback => Assert.Equal(AgentModelTypes.Fallback, fallback.ModelType));
    }

    [Fact]
    public async Task ValidationError_WithAgentOutput()
    {
        StreamingAgentMessageError message = await StreamErrorAsync("""
            {"type":"error","code":"validation_error","message":"The generated JSON is invalid (Code 0066)","documentation_url":null,"agent_result":{"content":"{ not json"},"generated_json":"{ not json"}
            """);

        ValidationError error = Assert.IsType<ValidationError>(message.Error);
        Assert.Null(error.DocumentationUrl);
        Assert.Null(error.Errors);
        Assert.Equal("{ not json", error.AgentResult!.Content);
        Assert.Equal("{ not json", error.GeneratedJson);
    }

    [Fact]
    public async Task ResourceNotFound_OnConversationStream()
    {
        SerenityClient client = TestClient.Returning(HttpStatusCode.OK, ToSse("""
            {"type":"error","code":"resource_not_found","message":"The requested resource was not found.","documentation_url":null,"errors":{"conversation_not_found":"The conversation was not found."}}
            """), "text/event-stream");

        List<StreamingAgentMessage> messages = new();
        await foreach (StreamingAgentMessage message in client.Agents.Assistants.CreateConversation("agent").StreamMessageAsync("Hello"))
            messages.Add(message);

        StreamingAgentMessageError errorMessage = Assert.Single(messages.OfType<StreamingAgentMessageError>());
        ResourceNotFoundError error = Assert.IsType<ResourceNotFoundError>(errorMessage.Error);
        Assert.True(error.Errors!.ContainsKey(ResourceNotFoundKeys.ConversationNotFound));
    }

    [Fact]
    public async Task UnrecognisedCode_UsesBaseType()
    {
        StreamingAgentMessageError message = await StreamErrorAsync("""
            {"type":"error","code":"brand_new_error","message":"Something new.","documentation_url":null,"errors":{"some_key":"some detail"}}
            """);

        Assert.Equal(typeof(SerenityApiError), message.Error.GetType());
        Assert.Equal("brand_new_error", message.Error.Code);
        Assert.Equal(new[] { "errors" }, message.Error.ExtensionData!.Keys);
    }

    [Fact]
    public async Task PayloadWithoutCode_IsUnknown()
    {
        // The shape the API used before error codes were introduced.
        StreamingAgentMessageError message = await StreamErrorAsync("""
            {"type":"error","status":400,"message":"Something failed."}
            """);

        Assert.Equal(SerenityErrorCodes.Unknown, message.Error.Code);
        Assert.Equal("Something failed.", message.Message);
    }

    [Fact]
    public async Task FailureBeforeTheStreamOpens_Throws()
    {
        SerenityClient client = TestClient.Returning(HttpStatusCode.Unauthorized, """
            { "code": "unauthorized", "message": "Authentication is required to access this resource." }
            """);

        SerenityApiException ex = await Assert.ThrowsAsync<SerenityApiException>(async () =>
        {
            await foreach (StreamingAgentMessage _ in client.Agents.Activities.Create("agent").StreamAsync())
            {
            }
        });

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal(SerenityErrorCodes.Unauthorized, ex.Error.Code);
    }

    private static async Task<StreamingAgentMessageError> StreamErrorAsync(string payload)
    {
        SerenityClient client = TestClient.Returning(HttpStatusCode.OK, ToSse(payload), "text/event-stream");

        List<StreamingAgentMessage> messages = new();
        await foreach (StreamingAgentMessage message in client.Agents.Activities.Create("agent").StreamAsync())
            messages.Add(message);

        return Assert.Single(messages.OfType<StreamingAgentMessageError>());
    }

    private static string ToSse(string payload) => $"event: error\ndata: {payload.Trim()}\n\n";
}
