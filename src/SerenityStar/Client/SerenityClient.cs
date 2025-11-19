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

            // Configure the API key header
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            Agents = new AgentsScope(_httpClient);
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
        /// <param name="httpClient">The configured HTTP client.</param>
        internal SerenityClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            Agents = new AgentsScope(_httpClient);
        }
    }
}