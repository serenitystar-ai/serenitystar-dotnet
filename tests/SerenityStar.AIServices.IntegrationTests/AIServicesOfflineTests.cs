using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Exceptions;
using SerenityStar.Extensions;
using SerenityStar.Models.AIServices;
using SerenityStar.Models.Anonymizer;
using Xunit;

namespace SerenityStar.AIServices.IntegrationTests
{
    /// <summary>
    /// Offline tests for the AI service clients. These use a fake <see cref="HttpMessageHandler"/> to capture the
    /// outgoing request and return a canned response, so they do not require an API key or network access.
    /// </summary>
    public class AIServicesOfflineTests
    {
        private const string BaseUrl = "https://offline.example.com";

        /// <summary>
        /// Captures each outgoing request (including multipart fields) and returns the configured response.
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
            public HttpMethod? Method { get; private set; }
            public Uri? RequestUri { get; private set; }
            public string[] ApiKeyValues { get; private set; } = Array.Empty<string>();
            public string? ContentMediaType { get; private set; }
            public JsonElement? JsonBody { get; private set; }
            public List<(string Name, string? FileName, string? MediaType, string? Value)> FormParts { get; } = new();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestCount++;
                Method = request.Method;
                RequestUri = request.RequestUri;
                ApiKeyValues = request.Headers.TryGetValues("X-API-KEY", out IEnumerable<string>? values) ? values.ToArray() : Array.Empty<string>();
                ContentMediaType = request.Content?.Headers.ContentType?.MediaType;

                if (request.Content is MultipartFormDataContent multipart)
                {
                    foreach (HttpContent part in multipart)
                    {
                        string name = part.Headers.ContentDisposition!.Name!.Trim('"');
                        string? fileName = part.Headers.ContentDisposition.FileName?.Trim('"');
                        string? value = fileName is null ? await part.ReadAsStringAsync(cancellationToken) : null;
                        FormParts.Add((name, fileName, part.Headers.ContentType?.MediaType, value));
                    }
                }
                else if (request.Content != null)
                {
                    using JsonDocument document = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
                    JsonBody = document.RootElement.Clone();
                }

                return new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_responseBody, Encoding.UTF8, _responseMediaType)
                };
            }

            public string? FormValue(string name) => FormParts.SingleOrDefault(p => p.Name == name).Value;
        }

        private static SerenityClient CreateClient(CapturingHandler handler) =>
            new SerenityClientBuilder()
                .WithApiKey("test-key")
                .WithBaseUrl(BaseUrl)
                .WithHttpClient(new HttpClient(handler))
                .Build();

        private static Stream FileStreamOf(string content) => new MemoryStream(Encoding.UTF8.GetBytes(content));

        private static void AssertPost(CapturingHandler handler, string path)
        {
            Assert.Equal(1, handler.RequestCount);
            Assert.Equal(HttpMethod.Post, handler.Method);
            Assert.Equal($"{BaseUrl}{path}", handler.RequestUri!.AbsoluteUri);
            Assert.Equal(new[] { "test-key" }, handler.ApiKeyValues);
        }

        #region Anonymizer
        [Fact]
        public async Task Anonymizer_Obfuscate_SendsJsonAndParsesSnakeCaseSpans()
        {
            CapturingHandler handler = new("""{"text":"Hi Person-1","spans":[{"entity_group":"private_person","key":"Person","word":"John","start":3,"end":7,"score":0.98,"placeholder":"Person-1"}],"provider":"OpenAIPrivacyFilter"}""");

            AnonymizerResult result = await CreateClient(handler).GetAnonymizerClient().ObfuscateAsync(new AnonymizerReq
            {
                Inputs = "Hi John",
                Categories = new List<string> { "private_person" },
                CategoryOptions = new List<AnonymizerCategoryOption> { new() { Category = "private_person", Key = "Person", Threshold = 0.5 } }
            });

            AssertPost(handler, "/api/v2/anonymizer/obfuscate");
            JsonElement body = handler.JsonBody!.Value;
            Assert.Equal("Hi John", body.GetProperty("inputs").GetString());
            Assert.Equal("private_person", body.GetProperty("categories")[0].GetString());
            Assert.Equal(0.5, body.GetProperty("categoryOptions")[0].GetProperty("threshold").GetDouble());
            Assert.False(body.TryGetProperty("vendor", out _));

            Assert.Equal("Hi Person-1", result.Text);
            AnonymizerSpan span = Assert.Single(result.Spans);
            Assert.Equal("private_person", span.EntityGroup);
            Assert.Equal("Person-1", span.Placeholder);
            Assert.Equal(7, span.End);
        }
        #endregion

        #region Cross-cutting
        [Fact]
        public async Task ErrorResponse_ThrowsSerenityApiException_WithPayload()
        {
            CapturingHandler handler = new("""{"message":"Vendor not found","code":"not_found"}""", statusCode: HttpStatusCode.NotFound);

            SerenityApiException exception = await Assert.ThrowsAsync<SerenityApiException>(() =>
                CreateClient(handler).GetAnonymizerClient().ObfuscateAsync(new AnonymizerReq { Inputs = "x" }));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal("not_found", exception.ResponseJson!.Value.GetProperty("code").GetString());
        }

        [Fact]
        public void GetClients_ThrowArgumentNullException_ForNullClient()
        {
            SerenityClient client = null!;

            Assert.Throws<ArgumentNullException>(() => client.GetAnonymizerClient());
        }

        [Fact]
        public async Task DependencyInjection_RegistersEveryAIService()
        {
            CapturingHandler handler = new("""{"text":"ok","spans":[]}""");

            ServiceCollection services = new();
            services.AddSerenityStar("di-key", baseUrl: BaseUrl)
                    .AddSerenityStarAIServices();
            services.AddHttpClient<SerenityClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => handler);

            using ServiceProvider provider = services.BuildServiceProvider();


            await provider.GetRequiredService<IAnonymizerClient>().ObfuscateAsync(new AnonymizerReq { Inputs = "x" });

            Assert.Equal(new[] { "di-key" }, handler.ApiKeyValues);
            Assert.Equal($"{BaseUrl}/api/v2/anonymizer/obfuscate", handler.RequestUri!.AbsoluteUri);
        }
        #endregion
    }
}
