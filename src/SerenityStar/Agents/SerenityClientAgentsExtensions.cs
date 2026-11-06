using SerenityStar.Agents;
using System;

namespace SerenityStar.Client
{
    /// <summary>
    /// Provides access to Serenity Star agents from a <see cref="SerenityClient"/>.
    /// </summary>
    public static class SerenityClientAgentsExtensions
    {
        /// <summary>
        /// Gets the agents scope for interacting with the different agents available in Serenity Star.
        /// You can choose between assistants, copilots, activities, AI proxies and chat completions.
        /// </summary>
        /// <param name="client">The Serenity Star client.</param>
        /// <returns>An <see cref="AgentsScope"/> that uses the client's connection.</returns>
        /// <exception cref="ArgumentNullException">Thrown when client is null.</exception>
        public static AgentsScope GetAgents(this SerenityClient client)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            return new AgentsScope(client.ApiClient);
        }
    }
}
