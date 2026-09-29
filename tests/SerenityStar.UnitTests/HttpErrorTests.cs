using System.Net;
using System.Net.Http.Headers;
using SerenityStar.Client;
using SerenityStar.Errors;
using SerenityStar.Errors.Constants;
using SerenityStar.Errors.Models;
using SerenityStar.Models.Transcription;
using Xunit;

namespace SerenityStar.UnitTests;

/// <summary>
/// Errors returned as non-success HTTP responses, thrown as <see cref="SerenityApiException"/>.
/// The bodies come from the API error reference.
/// </summary>
public class HttpErrorTests
{
    private const string DocsUrl = "https://docs.serenitystar.ai/docs/serenity-aihub/dev-tools/api-error-responses";

    [Fact]
    public async Task InputValidationError_KeepsEveryMessagePerField()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.BadRequest, $$"""
            {
              "message": "One or more validation errors occurred.",
              "code": "input_validation_error",
              "documentationUrl": "{{DocsUrl}}#input_validation_error",
              "errors": { "message": ["The field is required.", "The field is too short."] }
            }
            """);

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("One or more validation errors occurred.", ex.Message);
        Assert.Null(ex.ResponseContent);

        InputValidationError error = Assert.IsType<InputValidationError>(ex.Error);
        Assert.Equal(SerenityErrorCodes.InputValidationError, error.Code);
        Assert.Equal($"{DocsUrl}#input_validation_error", error.DocumentationUrl);
        Assert.Equal(new[] { "The field is required.", "The field is too short." }, error.Errors["message"]);
        Assert.Null(error.ExtensionData);
    }

    [Fact]
    public async Task ValidationError_WithKeyedErrors()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.BadRequest, """
            {
              "code": "validation_error",
              "message": "One or more validation errors occurred.",
              "errors": { "input_keys_duplicated": "Duplicate input keys are not allowed: message" }
            }
            """);

        ValidationError error = Assert.IsType<ValidationError>(ex.Error);
        Assert.NotNull(error.Errors);
        Assert.Equal("Duplicate input keys are not allowed: message", error.Errors![ValidationErrorKeys.InputKeysDuplicated]);
        Assert.Null(error.AgentResult);
        Assert.Null(error.GeneratedJson);
    }

    [Fact]
    public async Task ValidationError_WithMessageOnly()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.BadRequest, """
            { "code": "validation_error", "message": "There is no room for the user message." }
            """);

        ValidationError error = Assert.IsType<ValidationError>(ex.Error);
        Assert.Null(error.Errors);
        Assert.Equal("There is no room for the user message.", ex.Message);
    }

    [Fact]
    public async Task ValidationError_WithAgentOutput()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.BadRequest, """
            {
              "code": "validation_error",
              "message": "The generated JSON is invalid (Code 0066)",
              "agentResult": { "content": "{ not json" },
              "generatedJson": "{ not json",
              "pendingActions": []
            }
            """);

        ValidationError error = Assert.IsType<ValidationError>(ex.Error);
        Assert.NotNull(error.AgentResult);
        Assert.Equal("{ not json", error.AgentResult!.Content);
        Assert.Equal("{ not json", error.GeneratedJson);

        // pendingActions isn't modelled, so it's kept as-is.
        Assert.NotNull(error.ExtensionData);
        Assert.True(error.ExtensionData!.ContainsKey("pendingActions"));
    }

    [Fact]
    public async Task AgentExecutionFailed_KeepsEveryAttemptAndRetryAfter()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.TooManyRequests, """
            {
              "code": "agent_execution_failed",
              "message": "All retry attempts with the base and fallback model have failed.",
              "errors": { "vendor_error": "fallback detail" },
              "attempts": [
                { "index": 0, "modelType": "main", "code": "vendor_rate_limit_error", "statusCode": 429, "message": "main failed", "errors": { "vendor_error": "main detail" } },
                { "index": 1, "modelType": "fallback", "code": "vendor_rate_limit_error", "statusCode": 429, "message": "fallback failed", "errors": { "vendor_error": "fallback detail" } }
              ]
            }
            """,
            response => response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30)));

        AgentExecutionFailedError error = Assert.IsType<AgentExecutionFailedError>(ex.Error);
        Assert.Equal(TimeSpan.FromSeconds(30), error.RetryAfter);
        Assert.Equal("fallback detail", error.Errors![VendorErrorCodes.VendorError]);
        Assert.Collection(error.Attempts,
            main =>
            {
                Assert.Equal(0, main.Index);
                Assert.Equal(AgentModelTypes.Main, main.ModelType);
                Assert.Equal(VendorErrorCodes.VendorRateLimitError, main.Code);
                Assert.Equal(429, main.StatusCode);
                Assert.Equal("main failed", main.Message);
                Assert.Equal("main detail", main.VendorError);
            },
            fallback =>
            {
                Assert.Equal(1, fallback.Index);
                Assert.Equal(AgentModelTypes.Fallback, fallback.ModelType);
            });
    }

    [Fact]
    public async Task AgentExecutionFailed_WithoutStatusCodeOnAttempt()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.BadRequest, """
            {
              "code": "agent_execution_failed",
              "message": "The request exceeded the model's context window.",
              "attempts": [
                { "index": 0, "modelType": "main", "code": "vendor_context_length_error", "message": "too long", "errors": { "vendor_error": "detail" } }
              ]
            }
            """);

        AgentExecutionFailedError error = Assert.IsType<AgentExecutionFailedError>(ex.Error);
        AgentExecutionAttempt attempt = Assert.Single(error.Attempts);
        Assert.Equal(VendorErrorCodes.VendorContextLengthError, attempt.Code);
        Assert.Null(attempt.StatusCode);
        Assert.Null(error.RetryAfter);
    }

    [Fact]
    public async Task AIServiceExecutionFailed_WithAttempts()
    {
        SerenityApiException ex = await TranscribeAsync(HttpStatusCode.BadGateway, """
            {
              "code": "aiservice_execution_failed",
              "message": "The audio couldn't be transcribed. Please try again.",
              "errors": { "vendor_error": "detail" },
              "attempts": [
                { "index": 0, "code": "vendor_service_error", "statusCode": 503, "message": "The AI provider returned a server error (HTTP 503) (Code 0086)", "errors": { "vendor_error": "detail" } }
              ]
            }
            """);

        AIServiceExecutionFailedError error = Assert.IsType<AIServiceExecutionFailedError>(ex.Error);
        Assert.Null(error.Files);
        ExecutionAttempt attempt = Assert.Single(error.Attempts!);
        Assert.IsType<ExecutionAttempt>(attempt);
        Assert.Equal(VendorErrorCodes.VendorServiceError, attempt.Code);
        Assert.Equal(503, attempt.StatusCode);
        Assert.Equal("detail", attempt.VendorError);
    }

    [Fact]
    public async Task AIServiceExecutionFailed_WithFilesForOcr()
    {
        SerenityApiException ex = await TranscribeAsync(HttpStatusCode.BadRequest, """
            {
              "code": "aiservice_execution_failed",
              "message": "The text couldn't be extracted from any of the files. Please try again.",
              "errors": { "invoice.pdf": "server error", "scan.png": "rejected" },
              "files": [
                { "index": 0, "source": "invoice.pdf", "attempts": [ { "index": 0, "code": "vendor_service_error", "statusCode": 503, "message": "server error", "errors": { "vendor_error": "a" } } ] },
                { "index": 1, "source": "scan.png", "attempts": [ { "index": 0, "code": "vendor_validation_error", "statusCode": 422, "message": "rejected", "errors": { "vendor_error": "b" } } ] }
              ]
            }
            """);

        AIServiceExecutionFailedError error = Assert.IsType<AIServiceExecutionFailedError>(ex.Error);
        Assert.Null(error.Attempts);
        Assert.Equal("rejected", error.Errors!["scan.png"]);
        Assert.Collection(error.Files!,
            invoice =>
            {
                Assert.Equal("invoice.pdf", invoice.Source);
                Assert.Equal(503, Assert.Single(invoice.Attempts).StatusCode);
            },
            scan =>
            {
                Assert.Equal(1, scan.Index);
                Assert.Equal(VendorErrorCodes.VendorValidationError, Assert.Single(scan.Attempts).Code);
            });
    }

    [Fact]
    public async Task ResourceNotFound_WithKey()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.NotFound, """
            {
              "code": "resource_not_found",
              "message": "The resource you're trying to see was not found (Code 0040)",
              "errors": { "agent_version_not_found": "No published agent version found for this agent code: agent" }
            }
            """);

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        ResourceNotFoundError error = Assert.IsType<ResourceNotFoundError>(ex.Error);
        Assert.True(error.Errors!.ContainsKey(ResourceNotFoundKeys.AgentVersionNotFound));
    }

    [Fact]
    public async Task ResourceNotFound_WithoutKey()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.NotFound, """
            { "code": "resource_not_found", "message": "The requested model was not found." }
            """);

        ResourceNotFoundError error = Assert.IsType<ResourceNotFoundError>(ex.Error);
        Assert.Null(error.Errors);
    }

    [Fact]
    public async Task RateLimitExceeded_ReadsRetryAfterSeconds()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.TooManyRequests, """
            { "code": "rate_limit_exceeded", "message": "You have exceeded the allowed number of requests. Please try again later." }
            """,
            response => response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(12)));

        RateLimitExceededError error = Assert.IsType<RateLimitExceededError>(ex.Error);
        Assert.Equal(TimeSpan.FromSeconds(12), error.RetryAfter);
    }

    [Fact]
    public async Task RateLimitExceeded_ReadsRetryAfterDate()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.TooManyRequests, """
            { "code": "rate_limit_exceeded", "message": "Too many requests." }
            """,
            response => response.Headers.RetryAfter = new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddSeconds(60)));

        TimeSpan? retryAfter = ex.Error.RetryAfter;
        Assert.NotNull(retryAfter);
        Assert.InRange(retryAfter!.Value, TimeSpan.FromSeconds(50), TimeSpan.FromSeconds(61));
    }

    [Fact]
    public async Task CodeWithoutExtraData_UsesBaseType()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.Unauthorized, $$"""
            { "code": "unauthorized", "message": "Authentication is required to access this resource.", "documentationUrl": "{{DocsUrl}}#unauthorized" }
            """);

        Assert.Equal(typeof(SerenityApiError), ex.Error.GetType());
        Assert.Equal(SerenityErrorCodes.Unauthorized, ex.Error.Code);
        Assert.Equal("Authentication is required to access this resource.", ex.Message);
        Assert.Null(ex.Error.RetryAfter);
        Assert.Null(ex.Error.ExtensionData);
    }

    [Fact]
    public async Task UnrecognisedCode_KeepsCodeAndUnmodelledFields()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.Conflict, """
            { "code": "brand_new_error", "message": "Something new.", "errors": { "some_key": "some detail" } }
            """);

        Assert.Equal(typeof(SerenityApiError), ex.Error.GetType());
        Assert.Equal("brand_new_error", ex.Error.Code);
        Assert.Null(ex.ResponseContent);
        Assert.Equal("some detail", ex.Error.ExtensionData!["errors"].GetProperty("some_key").GetString());
    }

    [Fact]
    public async Task KnownCodeWithUnexpectedShape_FallsBackToBaseType()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.BadRequest, """
            { "code": "validation_error", "message": "Arrays where strings are expected.", "errors": { "some_key": ["a", "b"] } }
            """);

        Assert.Equal(typeof(SerenityApiError), ex.Error.GetType());
        Assert.Equal(SerenityErrorCodes.ValidationError, ex.Error.Code);
        Assert.Equal("Arrays where strings are expected.", ex.Message);
        Assert.True(ex.Error.ExtensionData!.ContainsKey("errors"));
    }

    [Fact]
    public async Task HtmlBody_IsUnknownWithRawContent()
    {
        const string html = "<html><body>502 Bad Gateway</body></html>";

        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.BadGateway, html, mediaType: "text/html");

        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.Equal(SerenityErrorCodes.Unknown, ex.Error.Code);
        Assert.Equal("Bad Gateway", ex.Message);
        Assert.Equal(html, ex.ResponseContent);
    }

    [Fact]
    public async Task ProblemJsonBody_IsUnknownWithRawContent()
    {
        const string body = """{ "detail": "missing Audit permission" }""";

        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.Forbidden, body, mediaType: "application/problem+json");

        Assert.Equal(SerenityErrorCodes.Unknown, ex.Error.Code);
        Assert.Equal("Forbidden", ex.Message);
        Assert.Equal(body, ex.ResponseContent);
    }

    [Fact]
    public async Task MalformedJsonBody_IsUnknownWithRawContent()
    {
        const string body = "not valid json {";

        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.InternalServerError, body);

        Assert.Equal(SerenityErrorCodes.Unknown, ex.Error.Code);
        Assert.Equal("Internal Server Error", ex.Message);
        Assert.Equal(body, ex.ResponseContent);
    }

    [Fact]
    public async Task EmptyBody_IsUnknownWithReasonPhrase()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.ServiceUnavailable, string.Empty);

        Assert.Equal(SerenityErrorCodes.Unknown, ex.Error.Code);
        Assert.Equal("Service Unavailable", ex.Message);
        Assert.Equal(string.Empty, ex.ResponseContent);
    }

    [Fact]
    public async Task JsonWithoutCode_IsUnknownButKeepsMessage()
    {
        // The shape the API used before error codes were introduced.
        const string body = """{ "message": "The conversation was not found." }""";

        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.NotFound, body);

        Assert.Equal(SerenityErrorCodes.Unknown, ex.Error.Code);
        Assert.Equal("The conversation was not found.", ex.Message);
        Assert.Equal(body, ex.ResponseContent);
    }

    [Fact]
    public async Task PlainTextRateLimit_IsUnknownButKeepsRetryAfter()
    {
        // The shape the API used before error codes were introduced.
        const string body = "API calls quota exceeded! maximum admitted 100 over 1m.";

        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.TooManyRequests, body, mediaType: "text/plain",
            configure: response => response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)));

        Assert.Equal(SerenityErrorCodes.Unknown, ex.Error.Code);
        Assert.Equal("Too Many Requests", ex.Message);
        Assert.Equal(TimeSpan.FromSeconds(60), ex.Error.RetryAfter);
        Assert.Equal(body, ex.ResponseContent);
    }

    [Fact]
    public async Task Exception_IsAnHttpRequestException()
    {
        SerenityApiException ex = await ExecuteAsync(HttpStatusCode.InternalServerError, """
            { "code": "server_error", "message": "An unexpected error occurred." }
            """);

        Assert.IsAssignableFrom<HttpRequestException>(ex);
    }

    [Fact]
    public async Task FileUploadFailure_ThrowsSerenityApiException()
    {
        SerenityClient client = TestClient.Returning(HttpStatusCode.RequestEntityTooLarge, """
            { "code": "request_too_large", "message": "The request is too large." }
            """);

        using MemoryStream audio = new(new byte[] { 1, 2, 3 });
        SerenityApiException ex = await Assert.ThrowsAsync<SerenityApiException>(() =>
            client.Agents.Assistants.CreateConversation("agent").SendMessageAsync(audio, "audio.mp3"));

        Assert.Equal(SerenityErrorCodes.RequestTooLarge, ex.Error.Code);
    }

    private static Task<SerenityApiException> ExecuteAsync(
        HttpStatusCode statusCode,
        string body,
        Action<HttpResponseMessage>? configure = null,
        string mediaType = "application/json")
    {
        SerenityClient client = TestClient.Returning(statusCode, body, mediaType, configure);
        return Assert.ThrowsAsync<SerenityApiException>(() => client.Agents.Activities.Create("agent").ExecuteAsync());
    }

    private static Task<SerenityApiException> TranscribeAsync(HttpStatusCode statusCode, string body)
    {
        SerenityClient client = TestClient.Returning(statusCode, body);
        TranscribeAudioReq request = new()
        {
            FileStream = new MemoryStream(new byte[] { 1, 2, 3 }),
            FileName = "audio.mp3"
        };
        return Assert.ThrowsAsync<SerenityApiException>(() => client.AIServices.Transcription.TranscribeAsync(request));
    }
}
