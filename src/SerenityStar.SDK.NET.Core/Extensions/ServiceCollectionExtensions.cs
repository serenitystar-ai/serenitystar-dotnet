using Microsoft.Extensions.DependencyInjection;
using SerenityStar.SDK.NET.Core.Client;
using SerenityStar.SDK.NET.Core.Constants;
using SerenityStar.SDK.NET.Core.Models;
using System;

namespace SerenityStar.SDK.NET.Core.Extensions
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
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddSerenityStar(
            this IServiceCollection services,
            string apiKey,
            int timeoutSeconds = 30)
        {
            services.Configure<SerenityStarOptions>(options =>
            {
                options.ApiKey = apiKey;
                options.TimeoutSeconds = timeoutSeconds;
            });

            services.AddHttpClient<ISerenityClient, SerenityClient>((_, client) =>
            {
                client.BaseAddress = new Uri(ClientConstants.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            return services;
        }
    }
}
