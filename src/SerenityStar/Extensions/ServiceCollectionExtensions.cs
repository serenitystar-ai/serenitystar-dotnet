using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Constants;
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
        /// Adds the Serenity Star client to the service collection.
        /// </summary>
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
            services.Configure<SerenityStarOptions>(options =>
            {
                options.ApiKey = apiKey;
                options.TimeoutSeconds = timeoutSeconds;
                options.BaseUrl = baseUrl;
            });

            services.AddHttpClient<ISerenityClient, SerenityClient>((_, client) =>
            {
                client.BaseAddress = new Uri(baseUrl ?? ClientConstants.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            return services;
        }
    }
}
