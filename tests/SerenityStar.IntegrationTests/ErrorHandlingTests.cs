using System.Net;
using System.Text;
using SerenityStar.Client;
using SerenityStar.Exceptions;
using SerenityStar.Extensions;
using Xunit;

namespace SerenityStar.IntegrationTests
{
    /// <summary>
    /// Offline tests for how the SDK surfaces unsuccessful API responses as
    /// <see cref="SerenityApiException"/>. These use a fake <see cref="HttpMessageHandler"/> that
    /// returns a fixed error response, so they do not require an API key or network access.
    /// </summary>
    public class ErrorHandlingTests
    {
        /// <summary>
        /// Returns a fixed error response so error-handling paths can be exercised offline.
        /// </summary>
        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly string _reasonPhrase;
            private readonly string _mediaType;
            private readonly string _body;

            public StubHandler(HttpStatusCode statusCode, string reasonPhrase, string mediaType, string body)
            {
                _statusCode = statusCode;
                _reasonPhrase = reasonPhrase;
                _mediaType = mediaType;
                _body = body;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(_statusCode)
                {
                    ReasonPhrase = _reasonPhrase,
                    Content = new StringContent(_body, Encoding.UTF8, _mediaType)
                });
            }
        }

        private static SerenityClient BuildClient(StubHandler handler) =>
            new SerenityClientBuilder()
                .WithApiKey("test-key")
                .WithHttpClient(new HttpClient(handler))
                .Build();

        // Error responses surface as SerenityApiException carrying the status, reason and parsed JSON.
        [Fact]
        public async Task ErrorResponse_ThrowsSerenityApiException_WithStatusAndJson()
        {
            StubHandler handler = new StubHandler(
                HttpStatusCode.BadRequest,
                "Bad Request",
                "application/json",
                "{\"message\":\"invalid agent code\"}");

            SerenityClient client = BuildClient(handler);

            SerenityApiException ex =
                await Assert.ThrowsAsync<SerenityApiException>(
                    () => client.Agents.Activities.Create("marketing-campaign").ExecuteAsync());

            Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
            Assert.Equal("Bad Request", ex.ReasonPhrase);
            // The JSON body is exposed as a parsed JsonElement.
            Assert.NotNull(ex.ResponseJson);
            Assert.Equal("invalid agent code", ex.ResponseJson!.Value.GetProperty("message").GetString());
            // Still catchable as the base HttpRequestException for backward compatibility.
            Assert.IsAssignableFrom<HttpRequestException>(ex);
        }

        // A 429 (rate limit) returns a non-JSON body, so ResponseJson is null but status is still exposed.
        [Fact]
        public async Task RateLimitResponse_ThrowsSerenityApiException_WithNullJson()
        {
            StubHandler handler = new StubHandler(
                (HttpStatusCode)429,
                "Too Many Requests",
                "text/plain",
                "Rate limit exceeded");

            SerenityClient client = BuildClient(handler);

            SerenityApiException ex =
                await Assert.ThrowsAsync<SerenityApiException>(
                    () => client.Agents.Activities.Create("marketing-campaign").ExecuteAsync());

            Assert.Equal((HttpStatusCode)429, ex.StatusCode);
            Assert.Null(ex.ResponseJson);
        }

        // A structured "application/problem+json" error body is still recognised as JSON and parsed.
        [Fact]
        public async Task ErrorResponse_WithProblemJsonContentType_ParsesResponseJson()
        {
            StubHandler handler = new StubHandler(
                HttpStatusCode.Forbidden,
                "Forbidden",
                "application/problem+json",
                "{\"detail\":\"missing Audit permission\"}");

            SerenityClient client = BuildClient(handler);

            SerenityApiException ex =
                await Assert.ThrowsAsync<SerenityApiException>(
                    () => client.Agents.Activities.Create("marketing-campaign").ExecuteAsync());

            Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
            Assert.NotNull(ex.ResponseJson);
            Assert.Equal("missing Audit permission", ex.ResponseJson!.Value.GetProperty("detail").GetString());
        }

        // A body that claims JSON but is malformed must not throw while building the exception; the
        // defensive parse falls back to a null ResponseJson.
        [Fact]
        public async Task ErrorResponse_WithMalformedJsonBody_LeavesResponseJsonNull()
        {
            StubHandler handler = new StubHandler(
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "application/json",
                "not valid json {");

            SerenityClient client = BuildClient(handler);

            SerenityApiException ex =
                await Assert.ThrowsAsync<SerenityApiException>(
                    () => client.Agents.Activities.Create("marketing-campaign").ExecuteAsync());

            Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
            Assert.Null(ex.ResponseJson);
        }

        // An empty body with a JSON content type is treated as no payload rather than a parse error.
        [Fact]
        public async Task ErrorResponse_WithEmptyBody_LeavesResponseJsonNull()
        {
            StubHandler handler = new StubHandler(
                HttpStatusCode.BadGateway,
                "Bad Gateway",
                "application/json",
                string.Empty);

            SerenityClient client = BuildClient(handler);

            SerenityApiException ex =
                await Assert.ThrowsAsync<SerenityApiException>(
                    () => client.Agents.Activities.Create("marketing-campaign").ExecuteAsync());

            Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
            Assert.Null(ex.ResponseJson);
        }

        // The exception message carries the status code so it is meaningful in logs even without inspecting
        // the structured properties.
        [Fact]
        public async Task ErrorResponse_Message_IncludesStatusCode()
        {
            StubHandler handler = new StubHandler(
                HttpStatusCode.NotFound,
                "Not Found",
                "application/json",
                "{\"message\":\"agent not found\"}");

            SerenityClient client = BuildClient(handler);

            SerenityApiException ex =
                await Assert.ThrowsAsync<SerenityApiException>(
                    () => client.Agents.Activities.Create("marketing-campaign").ExecuteAsync());

            Assert.Contains("NotFound", ex.Message);
        }
    }
}
