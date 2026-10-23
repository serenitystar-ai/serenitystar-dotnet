using Microsoft.Extensions.DependencyInjection;
using SerenityStar.AIServices;
using SerenityStar.Client;
using System;

namespace SerenityStar.Extensions
{
    /// <summary>
    /// Provides extension methods to register the Serenity Star AI services in an IServiceCollection.
    /// </summary>
    public static class AIServicesServiceCollectionExtensions
    {
        /// <summary>
        /// Registers every AI service interface (<see cref="IAnonymizerClient"/>, <see cref="IDecisionClient"/>,
        /// <see cref="IEmbeddingClient"/> and <see cref="IImageGenerationClient"/>)
        /// so they can be injected directly.
        /// Requires <see cref="ServiceCollectionExtensions.AddSerenityStar"/> to be called first.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddSerenityStarAIServices(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Transient to match the lifetime of the typed SerenityClient registered by AddSerenityStar.
            services.AddTransient(sp => sp.GetRequiredService<SerenityClient>().GetAnonymizerClient());
            services.AddTransient(sp => sp.GetRequiredService<SerenityClient>().GetDecisionClient());
            services.AddTransient(sp => sp.GetRequiredService<SerenityClient>().GetEmbeddingClient());
            services.AddTransient(sp => sp.GetRequiredService<SerenityClient>().GetImageGenerationClient());

            return services;
        }
    }
}
