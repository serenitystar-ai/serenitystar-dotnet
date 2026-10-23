using SerenityStar.Client;
using SerenityStar.Helpers;
using SerenityStar.Models.AIServices;
using SerenityStar.Models.ImageGeneration;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <inheritdoc />
    internal sealed class ImageGenerationClient : IImageGenerationClient
    {
        private readonly SerenityApiClient _apiClient;

        internal ImageGenerationClient(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <inheritdoc />
        public Task<ImageGenerationResult> GenerateAsync(
            GenerateImageReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            return AIServiceRequestHelper.PostJsonAsync<ImageGenerationResult>(
                _apiClient, "/api/v2/image/generate", request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ImageGenerationResult> GenerateWithSourceImagesAsync(
            GenerateImageWithSourceImagesReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            // Same route as GenerateAsync: the API selects the source images variant by the multipart content type.
            MultipartFormDataContent content = new();
            foreach (AIServiceFile file in request.Files)
                content.AddFile("Files", file);

            if (request.ModelId.HasValue)
                content.AddField("ModelId", request.ModelId.Value.ToString());

            content.AddField("Prompt", request.Prompt);
            content.AddField("UserIdentifier", request.UserIdentifier);
            content.AddField("Channel", request.Channel);

            return AIServiceRequestHelper.PostMultipartAsync<ImageGenerationResult>(
                _apiClient, "/api/v2/image/generate", content, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ImageGenerationResult> GenerateByFileIdAsync(
            GenerateImageByFileIdReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            return AIServiceRequestHelper.PostJsonAsync<ImageGenerationResult>(
                _apiClient, "/api/v2/image/generate/file", request, cancellationToken);
        }
    }
}
