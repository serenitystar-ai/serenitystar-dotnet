using SerenityStar.Models.Embeddings;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <summary>
    /// Generates vector embeddings for texts.
    /// Obtain an instance with <see cref="Client.SerenityClientAIServicesExtensions.GetEmbeddingClient"/>.
    /// </summary>
    /// <remarks>
    /// This interface is intended to be consumed (and mocked in tests), not implemented: new members may be added in future versions.
    /// </remarks>
    public interface IEmbeddingClient
    {
        /// <summary>
        /// Generates one embedding per input text.
        /// </summary>
        /// <param name="request">The embeddings request containing the texts and optional parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The embeddings, usage, and cost.</returns>
        /// <exception cref="System.ArgumentException">Thrown when no texts are provided.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<EmbeddingsResult> GenerateAsync(GenerateEmbeddingsReq request, CancellationToken cancellationToken = default);
    }
}
