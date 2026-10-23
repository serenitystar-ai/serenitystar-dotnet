using SerenityStar.AIServices;
using System;

namespace SerenityStar.Client
{
    /// <summary>
    /// Provides access to the Serenity Star AI services from a <see cref="SerenityClient"/>.
    /// Each method returns a lightweight client that uses the <see cref="SerenityClient"/> connection.
    /// </summary>
    public static class SerenityClientAIServicesExtensions
    {
        /// <summary>
        /// Gets a client to detect and obfuscate sensitive data (PII) in texts.
        /// </summary>
        /// <param name="client">The Serenity Star client.</param>
        /// <returns>An <see cref="IAnonymizerClient"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when client is null.</exception>
        public static IAnonymizerClient GetAnonymizerClient(this SerenityClient client)
            => new AnonymizerClient(GetApiClient(client));

        private static SerenityApiClient GetApiClient(SerenityClient client)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            return client.ApiClient;
        }
    }
}
