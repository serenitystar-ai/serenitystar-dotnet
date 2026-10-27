using SerenityStar.Client;
using SerenityStar.Helpers;
using SerenityStar.Models.Embeddings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <inheritdoc />
    internal sealed class EmbeddingClient : IEmbeddingClient
    {
        private readonly SerenityApiClient _apiClient;

        internal EmbeddingClient(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <inheritdoc />
        public Task<EmbeddingsResult> GenerateAsync(
            GenerateEmbeddingsReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            return AIServiceRequestHelper.PostJsonAsync<EmbeddingsResult>(
                _apiClient, "/api/v2/embeddings/generate", request, cancellationToken);
        }
    }
}
