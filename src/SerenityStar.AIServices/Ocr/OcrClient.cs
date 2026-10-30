using SerenityStar.Client;
using SerenityStar.Helpers;
using SerenityStar.Models.AIServices;
using SerenityStar.Models.Ocr;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <inheritdoc />
    internal sealed class OcrClient : IOcrClient
    {
        private readonly SerenityApiClient _apiClient;

        internal OcrClient(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <inheritdoc />
        public Task<OcrResult> ExtractAsync(
            OcrReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            MultipartFormDataContent content = new();
            foreach (AIServiceFile file in request.Files)
                content.AddFile("Files", file);

            if (request.AIModelId.HasValue)
                content.AddField("AIModelId", request.AIModelId.Value.ToString());

            content.AddField("ModelCode", request.ModelCode);
            content.AddField("Vendor", request.Vendor);
            content.AddField("Instructions", request.Instructions);
            content.AddField("UserIdentifier", request.UserIdentifier);
            content.AddField("Channel", request.Channel);

            return AIServiceRequestHelper.PostMultipartAsync<OcrResult>(
                _apiClient, "/api/v2/ocr", content, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OcrResult> ExtractFromBase64Async(
            OcrBase64Req request,
            CancellationToken cancellationToken = default)
            => PostJsonAsync(request, "/api/v2/ocr/base64", cancellationToken);

        /// <inheritdoc />
        public Task<OcrResult> ExtractFromUrlsAsync(
            OcrUrlReq request,
            CancellationToken cancellationToken = default)
            => PostJsonAsync(request, "/api/v2/ocr/url", cancellationToken);

        /// <inheritdoc />
        public Task<OcrResult> ExtractByFileIdAsync(
            OcrByFileIdReq request,
            CancellationToken cancellationToken = default)
            => PostJsonAsync(request, "/api/v2/ocr/file", cancellationToken);

        #region Private Methods
        private Task<OcrResult> PostJsonAsync(
            OcrReqBase request,
            string path,
            CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            // Serialized with the runtime type so the derived document list is included.
            return AIServiceRequestHelper.PostJsonAsync<OcrResult>(_apiClient, path, request, cancellationToken);
        }
        #endregion
    }
}
