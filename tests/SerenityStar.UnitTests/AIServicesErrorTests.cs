using System.Net;
using System.Net.Http.Headers;
using SerenityStar.Client;
using SerenityStar.Errors;
using SerenityStar.Errors.Constants;
using SerenityStar.Errors.Models;
using SerenityStar.Models.Embeddings;
using SerenityStar.Models.Ocr;
using SerenityStar.Models.SpeechGeneration;
using SerenityStar.Models.Transcription;
using Xunit;

namespace SerenityStar.UnitTests;

/// <summary>
/// AI service errors: thrown as <see cref="SerenityApiException"/> on HTTP calls, and yielded as
/// <see cref="StreamingSpeechGenerationMessageError"/> after a speech stream opens. The bodies come from the API error reference.
/// </summary>
public class AIServicesErrorTests
{
    private const string AttemptsBody = """
        {
          "code": "aiservice_execution_failed",
          "message": "The AI service couldn't complete the request. Please try again.",
          "errors": { "vendor_error": "detail" },
          "attempts": [
            { "index": 0, "code": "vendor_service_error", "statusCode": 503, "message": "The AI provider returned a server error (HTTP 503) (Code 0086)", "errors": { "vendor_error": "detail" } }
          ]
        }
        """;

    [Fact]
    public async Task AIServiceExecutionFailed_WithAttempts_OnJsonClient()
    {
        SerenityClient client = TestClient.Returning(HttpStatusCode.BadGateway, AttemptsBody);

        SerenityApiException ex = await Assert.ThrowsAsync<SerenityApiException>(() =>
            client.GetEmbeddingClient().GenerateAsync(new GenerateEmbeddingsReq { Content = { "hello" } }));

        AssertSingleAttempt(ex);
    }

    [Fact]
    public async Task AIServiceExecutionFailed_WithAttempts_OnMultipartClient()
    {
        SerenityClient client = TestClient.Returning(HttpStatusCode.BadGateway, AttemptsBody);
        TranscribeAudioReq request = new()
        {
            FileStream = new MemoryStream(new byte[] { 1, 2, 3 }),
            FileName = "audio.mp3"
        };

        SerenityApiException ex = await Assert.ThrowsAsync<SerenityApiException>(() =>
            client.GetTranscriptionClient().TranscribeAsync(request));

        AssertSingleAttempt(ex);
    }

    [Fact]
    public async Task AIServiceExecutionFailed_WithFilesForOcr()
    {
        SerenityClient client = TestClient.Returning(HttpStatusCode.BadRequest, """
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

        SerenityApiException ex = await Assert.ThrowsAsync<SerenityApiException>(() =>
            client.GetOcrClient().ExtractFromUrlsAsync(new OcrUrlReq { Urls = { "https://files.test/invoice.pdf", "https://files.test/scan.png" } }));

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
    public async Task RateLimitExceeded_ReadsRetryAfter()
    {
        SerenityClient client = TestClient.Returning(HttpStatusCode.TooManyRequests, """
            { "code": "rate_limit_exceeded", "message": "You have exceeded the allowed number of requests. Please try again later." }
            """,
            configure: response => response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(12)));

        SerenityApiException ex = await Assert.ThrowsAsync<SerenityApiException>(() =>
            client.GetEmbeddingClient().GenerateAsync(new GenerateEmbeddingsReq { Content = { "hello" } }));

        Assert.Equal(HttpStatusCode.TooManyRequests, ex.StatusCode);
        RateLimitExceededError error = Assert.IsType<RateLimitExceededError>(ex.Error);
        Assert.Equal(TimeSpan.FromSeconds(12), error.RetryAfter);
    }

    [Fact]
    public async Task SpeechGenerationStream_AIServiceExecutionFailed_ReadsSnakeCasePayload()
    {
        SerenityClient client = TestClient.Returning(HttpStatusCode.OK, """
            event: error
            data: {"type":"error","code":"aiservice_execution_failed","message":"The speech couldn't be generated. Please try again.","documentation_url":"https://docs.serenitystar.ai/#aiservice_execution_failed","errors":{"vendor_error":"detail"},"attempts":[{"index":0,"code":"vendor_rate_limit_error","status_code":429,"message":"rate limited","errors":{"vendor_error":"detail"}}],"retry_after_seconds":20}

            """, "text/event-stream");

        List<StreamingSpeechGenerationMessage> messages = new();
        await foreach (StreamingSpeechGenerationMessage message in client.GetSpeechGenerationClient().StreamAsync(new GenerateSpeechReq { Text = "Hello" }))
            messages.Add(message);

        StreamingSpeechGenerationMessageError errorMessage = Assert.IsType<StreamingSpeechGenerationMessageError>(Assert.Single(messages));
        Assert.Equal("The speech couldn't be generated. Please try again.", errorMessage.Message);

        AIServiceExecutionFailedError error = Assert.IsType<AIServiceExecutionFailedError>(errorMessage.Error);
        Assert.Equal(SerenityErrorCodes.AIServiceExecutionFailed, error.Code);
        Assert.Equal("https://docs.serenitystar.ai/#aiservice_execution_failed", error.DocumentationUrl);
        Assert.Equal(TimeSpan.FromSeconds(20), error.RetryAfter);

        ExecutionAttempt attempt = Assert.Single(error.Attempts!);
        Assert.Equal(VendorErrorCodes.VendorRateLimitError, attempt.Code);
        Assert.Equal(429, attempt.StatusCode);
        Assert.Equal("detail", attempt.VendorError);

        // "type" and "retry_after_seconds" are modelled elsewhere, so nothing is left over.
        Assert.Null(error.ExtensionData);
    }

    private static void AssertSingleAttempt(SerenityApiException ex)
    {
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);

        AIServiceExecutionFailedError error = Assert.IsType<AIServiceExecutionFailedError>(ex.Error);
        Assert.Null(error.Files);
        Assert.Equal("detail", error.Errors![VendorErrorCodes.VendorError]);

        ExecutionAttempt attempt = Assert.Single(error.Attempts!);
        Assert.Equal(VendorErrorCodes.VendorServiceError, attempt.Code);
        Assert.Equal(503, attempt.StatusCode);
        Assert.Equal("detail", attempt.VendorError);
    }
}
