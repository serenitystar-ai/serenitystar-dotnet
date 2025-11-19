using Microsoft.Extensions.Options;
using SerenityStar.Agents;
using SerenityStar.Constants;
using SerenityStar.Models;
using System;
using System.Net.Http;

namespace SerenityStar.Client
{
    /// <inheritdoc />
    public class SerenityClient : ISerenityClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Interact with the different agents available in Serenity Star.
        /// You can choose between assistants, activities, proxies, and chat completions.
        /// </summary>
        public AgentsScope Agents { get; }

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
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            ConfigureHttpClient(_httpClient, apiKey);
            Agents = new AgentsScope(_httpClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerenityClient"/> class for direct instantiation.
        /// </summary>
        /// <param name="apiKey">The API key to use for authentication.</param>
        /// <param name="baseUrl">The base URL to use for the API.</param>
        /// <param name="timeout">The timeout in seconds.</param>
        public static SerenityClient Create(string apiKey, string? baseUrl = null, int? timeout = null)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            HttpClient httpClient = new HttpClient();
            ConfigureHttpClient(httpClient, apiKey, baseUrl, timeout);

            return new SerenityClient(httpClient);
        }

        // Private constructor for direct instantiation
        private SerenityClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            Agents = new AgentsScope(_httpClient);
        }

        private static void ConfigureHttpClient(HttpClient client, string apiKey, string? baseUrl = null, int? timeout = null)
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            baseUrl ??= ClientConstants.BaseUrl;
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeout ?? 100);
        }
    }
}