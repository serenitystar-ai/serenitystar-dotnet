using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Models;
using System;

namespace SerenityStar.Extensions
{
    /// <summary>
    /// Provides extension methods for the IServiceCollection interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Serenity Star client (<see cref="SerenityClient"/>) to the service collection.
        /// </summary>
        /// <remarks>
        /// Register the features you use on top of it, e.g. <c>AddSerenityStarAgents()</c>
        /// (SubgenAI.SerenityStar.SDK) or <c>AddSerenityStarAIServices()</c> (SubgenAI.SerenityStar.AIServices).
        /// </remarks>
        /// <param name="services">The service collection.</param>
        /// <param name="apiKey">The API key to use for authentication.</param>
        /// <param name="timeoutSeconds">The timeout in seconds.</param>
        /// <param name="baseUrl">The base URL for the API. If not provided, uses the default Serenity Star API URL.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddSerenityStar(
            this IServiceCollection services,
            string apiKey,
            int timeoutSeconds = 30,
            string? baseUrl = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey), "API key cannot be null or empty.");

            services.Configure<SerenityStarOptions>(options =>
            {
                options.ApiKey = apiKey;
                options.TimeoutSeconds = timeoutSeconds;
                options.BaseUrl = baseUrl;
            });

            // Auth and base URL are applied per-request by SerenityApiClient (built inside
            // SerenityClient from the options), so only the timeout is configured on the client here.
            services.AddHttpClient<SerenityClient>((_, client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            return services;
        }
    }
}
