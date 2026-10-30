using SerenityStar.Client;
using SerenityStar.Helpers;
using SerenityStar.Models.Vision;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <inheritdoc />
    internal sealed class VisionClient : IVisionClient
    {
        private readonly SerenityApiClient _apiClient;

        internal VisionClient(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <inheritdoc />
        public Task<VisionResult> AnalyzeAsync(
            AnalyzeImageReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            MultipartFormDataContent content = new();
            content.AddFile("File", request.FileStream, request.FileName);

            if (request.AIModelId.HasValue)
                content.AddField("AIModelId", request.AIModelId.Value.ToString());

            content.AddField("ModelCode", request.ModelCode);
            content.AddField("Vendor", request.Vendor);
            content.AddField("Prompt", request.Prompt);
            content.AddField("UserIdentifier", request.UserIdentifier);
            content.AddField("Channel", request.Channel);

            return AIServiceRequestHelper.PostMultipartAsync<VisionResult>(
                _apiClient, "/api/v2/image/vision", content, cancellationToken);
        }

        /// <inheritdoc />
        public Task<VisionResult> AnalyzeByFileIdAsync(
            AnalyzeImageByFileIdReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            return AIServiceRequestHelper.PostJsonAsync<VisionResult>(
                _apiClient, "/api/v2/image/vision/file", request, cancellationToken);
        }
    }
}
