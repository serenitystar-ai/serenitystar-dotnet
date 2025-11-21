using SerenityStar.Constants;
using System;
using System.Net.Http;

namespace SerenityStar.Client
{
    /// <summary>
    /// Builder for creating a <see cref="SerenityClient"/> instance with a fluent API.
    /// </summary>
    /// <remarks>
    /// This builder provides a convenient way to configure and instantiate a <see cref="SerenityClient"/>
    /// without using dependency injection. It follows a fluent builder pattern similar to KernelBuilder.
    ///
    /// Example usage:
    /// <code>
    /// var client = new SerenityClientBuilder()
    ///     .WithApiKey("your-api-key")
    ///     .WithTimeout(30)
    ///     .Build();
    /// </code>
    /// </remarks>
    public sealed class SerenityClientBuilder
    {
        private string? _apiKey;
        private string? _baseUrl;
        private int? _timeoutSeconds;
        private HttpClient? _httpClient;

        /// <summary>
        /// Sets the API key for authentication.
        /// </summary>
        /// <param name="apiKey">The API key to use.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when apiKey is null or empty.</exception>
        public SerenityClientBuilder WithApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey), "API key cannot be null or empty.");

            _apiKey = apiKey;
            return this;
        }

        /// <summary>
        /// Sets the base URL for the API.
        /// </summary>
        /// <param name="baseUrl">The base URL to use.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when baseUrl is null or empty.</exception>
        public SerenityClientBuilder WithBaseUrl(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl), "Base URL cannot be null or empty.");

            _baseUrl = baseUrl;
            return this;
        }

        /// <summary>
        /// Sets the timeout for HTTP requests in seconds.
        /// </summary>
        /// <param name="timeoutSeconds">The timeout in seconds.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when timeoutSeconds is less than or equal to zero.</exception>
        public SerenityClientBuilder WithTimeout(int timeoutSeconds)
        {
            if (timeoutSeconds <= 0)
                throw new ArgumentException("Timeout must be greater than zero.", nameof(timeoutSeconds));

            _timeoutSeconds = timeoutSeconds;
            return this;
        }

        /// <summary>
        /// Sets a custom HttpClient instance to use for making requests.
        /// </summary>
        /// <param name="httpClient">The HttpClient to use.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when httpClient is null.</exception>
        public SerenityClientBuilder WithHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient), "HttpClient cannot be null.");
            return this;
        }

        /// <summary>
        /// Builds and returns a configured <see cref="SerenityClient"/> instance.
        /// </summary>
        /// <returns>A configured SerenityClient instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when required configuration is missing (API key).</exception>
        public SerenityClient Build()
        {
            if (string.IsNullOrEmpty(_apiKey))
                throw new InvalidOperationException("API key must be set before building the client. Use WithApiKey() to configure it.");

            HttpClient httpClient = _httpClient ?? new HttpClient();
            ConfigureHttpClient(httpClient, _apiKey, _baseUrl, _timeoutSeconds);

            return new SerenityClient(httpClient);
        }

        private static void ConfigureHttpClient(HttpClient client, string apiKey, string? baseUrl, int? timeoutSeconds)
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            baseUrl ??= ClientConstants.BaseUrl;
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds ?? 100);
        }
    }
}
