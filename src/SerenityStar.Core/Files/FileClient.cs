using SerenityStar.Client;
using SerenityStar.Constants;
using SerenityStar.Extensions;
using SerenityStar.Helpers;
using SerenityStar.Models.Files;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Files
{
    /// <inheritdoc />
    internal sealed class FileClient : IFileClient
    {
        private readonly SerenityApiClient _apiClient;

        internal FileClient(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <inheritdoc />
        public async Task<UploadFileResult> UploadAsync(
            UploadFileReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            StreamContent fileContent = new(request.FileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentTypeHelper.GetContentType(request.FileName));

            MultipartFormDataContent content = new();
            content.Add(fileContent, "formFile", request.FileName);

            using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/file/upload")
            {
                Content = content
            };

            using HttpResponseMessage response = await _apiClient.SendAsync(httpRequest, cancellationToken);

            return await response.ReadSerenityJsonAsync<UploadFileResult>(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<UploadFileResult> UploadFromUrlAsync(
            UploadFileFromUrlReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/file/upload/url")
            {
                Content = JsonContent.Create(request, options: JsonSerializerOptionsCache.s_camelCaseIgnoreNull)
            };

            using HttpResponseMessage response = await _apiClient.SendAsync(httpRequest, cancellationToken);

            return await response.ReadSerenityJsonAsync<UploadFileResult>(cancellationToken);
        }
    }
}
