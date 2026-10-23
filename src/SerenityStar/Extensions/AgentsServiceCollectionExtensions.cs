using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Agents;
using SerenityStar.Client;
using System;

namespace SerenityStar.Extensions
{
    /// <summary>
    /// Provides extension methods to register Serenity Star agents in an IServiceCollection.
    /// </summary>
    public static class AgentsServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="AgentsScope"/> so it can be injected directly.
        /// Requires <see cref="ServiceCollectionExtensions.AddSerenityStar"/> to be called first.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddSerenityStarAgents(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Transient to match the lifetime of the typed SerenityClient registered by AddSerenityStar.
            services.AddTransient(sp => sp.GetRequiredService<SerenityClient>().GetAgents());

            return services;
        }
    }
}
