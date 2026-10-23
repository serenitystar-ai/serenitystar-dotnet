using SerenityStar.Models.Anonymizer;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <summary>
    /// Detects and obfuscates sensitive data (PII) in texts.
    /// Obtain an instance with <see cref="Client.SerenityClientAIServicesExtensions.GetAnonymizerClient"/>.
    /// </summary>
    /// <remarks>
    /// This interface is intended to be consumed (and mocked in tests), not implemented: new members may be added in future versions.
    /// </remarks>
    public interface IAnonymizerClient
    {
        /// <summary>
        /// Replaces the sensitive entities found in the text with placeholders.
        /// </summary>
        /// <param name="request">The anonymizer request containing the text and optional parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The obfuscated text and the detected entities.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request or its inputs are null.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<AnonymizerResult> ObfuscateAsync(AnonymizerReq request, CancellationToken cancellationToken = default);
    }
}
