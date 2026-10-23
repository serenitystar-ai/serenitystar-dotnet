using Microsoft.Extensions.Options;
using SerenityStar.Constants;
using SerenityStar.Models;
using System;
using System.Net.Http;

namespace SerenityStar.Client
{
    /// <summary>
    /// Entry point for interacting with Serenity Star. Holds the authenticated connection to the API and
    /// is shared by every feature package.
    /// </summary>
    /// <remarks>
    /// Features are obtained from this client through extension methods provided by each package:
    /// <list type="bullet">
    /// <item><description>Agents (SubgenAI.SerenityStar.SDK): <c>client.GetAgents()</c></description></item>
    /// <item><description>AI services (SubgenAI.SerenityStar.AIServices): <c>client.GetTranscriptionClient()</c>, <c>client.GetSpeechGenerationClient()</c>, etc.</description></item>
    /// </list>
    /// </remarks>
    public sealed class SerenityClient
    {
        /// <summary>
        /// The authenticated API client. Internal so the API key is never exposed publicly; feature
        /// packages access it through InternalsVisibleTo.
        /// </summary>
        internal SerenityApiClient ApiClient { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerenityClient"/> class for dependency injection.
        /// </summary>
        /// <param name="options">The options containing the API key.</param>
        /// <param name="httpClient">The HTTP client to use for making requests.</param>
        public SerenityClient(IOptions<SerenityStarOptions> options, HttpClient httpClient)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string apiKey = options.Value.ApiKey ?? throw new ArgumentNullException(nameof(options), "The ApiKey property of the options. Value object is null.");
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            // Auth is attached per-request by SerenityApiClient rather than mutating the HttpClient.
            ApiClient = new SerenityApiClient(httpClient, apiKey, options.Value.BaseUrl);
        }

        /// <summary>
        /// Creates a new <see cref="SerenityClient"/> instance for direct instantiation.
        /// This is a convenience method that uses the builder pattern internally.
        /// </summary>
        /// <param name="apiKey">The API key to use for authentication.</param>
        /// <param name="baseUrl">The base URL to use for the API. Defaults to the standard Serenity Star endpoint.</param>
        /// <param name="timeout">The timeout in seconds. Defaults to 100 seconds.</param>
        /// <returns>A configured SerenityClient instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when apiKey is null or empty.</exception>
        public static SerenityClient Create(string apiKey, string? baseUrl = null, int? timeout = null)
        {
            return new SerenityClientBuilder()
                .WithApiKey(apiKey)
                .WithBaseUrl(baseUrl ?? ClientConstants.BaseUrl)
                .WithTimeout(timeout ?? 100)
                .Build();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerenityClient"/> class.
        /// This constructor is for internal use by the builder pattern.
        /// </summary>
        /// <param name="apiClient">The configured API client.</param>
        internal SerenityClient(SerenityApiClient apiClient)
        {
            ApiClient = apiClient;
        }
    }
}
