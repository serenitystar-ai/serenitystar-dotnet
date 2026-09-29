using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Extensions;
using Xunit;

namespace SerenityStar.UnitTests;

/// <summary>
/// How the SDK attaches authentication and resolves URIs. A fake <see cref="HttpMessageHandler"/>
/// captures the outgoing request instead of sending it.
/// </summary>
public class HttpClientAuthenticationTests
{
    // The SDK's built-in base URL, used when none is configured.
    private const string BaseUrl = "https://api.serenitystar.ai";

    /// <summary>
    /// Captures each outgoing request and returns a canned successful response so that
    /// ExecuteAsync completes without hitting the network.
    /// </summary>
    private sealed class CapturingHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public Uri? LastRequestUri { get; private set; }
        public string[] LastApiKeyValues { get; private set; } = Array.Empty<string>();
        public HttpRequestHeaders? LastRequestHeaders { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequestUri = request.RequestUri;
            LastRequestHeaders = request.Headers;
            LastApiKeyValues = request.Headers.TryGetValues("X-API-KEY", out IEnumerable<string>? values)
                ? values.ToArray()
                : Array.Empty<string>();

            // Drain any request body so content is materialized before the message is disposed.
            if (request.Content != null)
                await request.Content.ReadAsStringAsync();

            // Mock a successful response without hitting the network.
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
        }
    }

    // Approach 1: builder with a caller-provided HttpClient.
    [Fact]
    public async Task Builder_WithProvidedHttpClient_AttachesApiKeyPerRequest_AndDoesNotMutateClient()
    {
        CapturingHandler handler = new();
        HttpClient customHttpClient = new(handler);

        SerenityClient client = new SerenityClientBuilder()
            .WithApiKey("test-key")
            .WithHttpClient(customHttpClient)
            .Build();

        await client.Agents.Activities.Create("marketing-campaign").ExecuteAsync();

        // The API key is attached to the request...
        Assert.Equal(new[] { "test-key" }, handler.LastApiKeyValues);
        Assert.Equal($"{BaseUrl}/api/v2/agent/marketing-campaign/execute", handler.LastRequestUri!.AbsoluteUri);

        // ...but the caller's HttpClient is never mutated.
        Assert.False(customHttpClient.DefaultRequestHeaders.Contains("X-API-KEY"));
        Assert.Null(customHttpClient.BaseAddress);
    }

    // Approach 1b: default headers configured on the caller's HttpClient are preserved and sent
    // alongside the per-request API key.
    [Fact]
    public async Task Builder_WithProvidedHttpClient_PreservesCustomDefaultHeaders()
    {
        CapturingHandler handler = new();
        HttpClient customHttpClient = new(handler);
        customHttpClient.DefaultRequestHeaders.Add("X-Custom-Header", "custom-value");
        customHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("my-app/1.0");

        SerenityClient client = new SerenityClientBuilder()
            .WithApiKey("test-key")
            .WithHttpClient(customHttpClient)
            .Build();

        await client.Agents.Activities.Create("marketing-campaign").ExecuteAsync();

        // The caller's default headers ride along on the outgoing request...
        Assert.Equal(new[] { "custom-value" }, handler.LastRequestHeaders!.GetValues("X-Custom-Header"));
        Assert.Equal("my-app/1.0", handler.LastRequestHeaders.UserAgent.ToString());
        // ...as does the SDK's per-request API key.
        Assert.Equal(new[] { "test-key" }, handler.LastApiKeyValues);

        // And the client's own default headers are still intact after the request.
        Assert.Equal(new[] { "custom-value" }, customHttpClient.DefaultRequestHeaders.GetValues("X-Custom-Header"));
    }

    // Approach 2: builder with a custom base URL still resolves relative request URIs.
    [Fact]
    public async Task Builder_WithCustomBaseUrl_ResolvesRequestUri()
    {
        CapturingHandler handler = new();
        HttpClient customHttpClient = new(handler);

        SerenityClient client = new SerenityClientBuilder()
            .WithApiKey("test-key")
            .WithBaseUrl("https://custom.example.com")
            .WithHttpClient(customHttpClient)
            .Build();

        await client.Agents.Activities.Create("marketing-campaign").ExecuteAsync();

        Assert.Equal("https://custom.example.com/api/v2/agent/marketing-campaign/execute", handler.LastRequestUri!.AbsoluteUri);
    }

    // Approach 3: the original bug - reusing one HttpClient across clients/requests must not throw
    // a duplicate-header exception, and exactly one API key must be sent each time.
    [Fact]
    public async Task SharedHttpClient_ReusedAcrossClientsAndRequests_DoesNotDuplicateHeader()
    {
        CapturingHandler handler = new();
        HttpClient sharedHttpClient = new(handler);

        SerenityClient first = new SerenityClientBuilder()
            .WithApiKey("key-1")
            .WithHttpClient(sharedHttpClient)
            .Build();

        SerenityClient second = new SerenityClientBuilder()
            .WithApiKey("key-2")
            .WithHttpClient(sharedHttpClient)
            .Build();

        // Each request carries exactly its own client's key - assert right after each send,
        // before the next request overwrites the captured values.
        await first.Agents.Activities.Create("agent-a").ExecuteAsync();
        Assert.Equal(new[] { "key-1" }, handler.LastApiKeyValues);

        await second.Agents.Activities.Create("agent-b").ExecuteAsync();
        Assert.Equal(new[] { "key-2" }, handler.LastApiKeyValues);

        await first.Agents.Activities.Create("agent-a").ExecuteAsync();
        Assert.Equal(new[] { "key-1" }, handler.LastApiKeyValues);

        Assert.Equal(3, handler.RequestCount);
    }

    // Approach 4: dependency injection via IHttpClientFactory (AddSerenityStar + a primary handler).
    [Fact]
    public async Task DependencyInjection_WithHttpClientFactory_AttachesApiKeyPerRequest()
    {
        CapturingHandler handler = new();

        ServiceCollection services = new();
        services.AddSerenityStar("di-key");
        // Route the factory-managed HttpClient through our capturing handler.
        services.AddHttpClient<ISerenityClient, SerenityClient>()
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        using ServiceProvider provider = services.BuildServiceProvider();
        ISerenityClient client = provider.GetRequiredService<ISerenityClient>();

        // Execute twice: IHttpClientFactory pools/reuses the handler, which must not throw.
        await client.Agents.Activities.Create("marketing-campaign").ExecuteAsync();
        await client.Agents.Activities.Create("marketing-campaign").ExecuteAsync();

        Assert.Equal(2, handler.RequestCount);
        Assert.Equal(new[] { "di-key" }, handler.LastApiKeyValues);
        Assert.Equal($"{BaseUrl}/api/v2/agent/marketing-campaign/execute", handler.LastRequestUri!.AbsoluteUri);
    }
}
