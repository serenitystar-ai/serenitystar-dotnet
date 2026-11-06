using System.Net;
using System.Text;
using SerenityStar.Client;

namespace SerenityStar.UnitTests;

/// <summary>
/// Builds a <see cref="SerenityClient"/> whose HTTP calls are answered by a canned response instead of the API.
/// </summary>
internal static class TestClient
{
    internal static SerenityClient Returning(
        HttpStatusCode statusCode,
        string body,
        string mediaType = "application/json",
        Action<HttpResponseMessage>? configure = null)
    {
        return Create(_ =>
        {
            HttpResponseMessage response = new(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, mediaType)
            };
            configure?.Invoke(response);
            return response;
        });
    }

    internal static SerenityClient Create(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        return new SerenityClientBuilder()
            .WithApiKey("test-api-key")
            .WithBaseUrl("https://api.test")
            .WithHttpClient(new HttpClient(new FakeHttpMessageHandler(respond)))
            .Build();
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            _respond = respond;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_respond(request));
    }
}
