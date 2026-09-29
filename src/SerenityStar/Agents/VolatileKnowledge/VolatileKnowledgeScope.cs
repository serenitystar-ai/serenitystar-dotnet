using SerenityStar.Client;
using SerenityStar.Extensions;
using SerenityStar.Models.VolatileKnowledge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Agents.VolatileKnowledge
{
    /// <summary>
    /// Provides methods for uploading and managing volatile knowledge.
    /// </summary>
    public sealed class VolatileKnowledgeScope
    {
        private readonly SerenityApiClient _apiClient;

        internal VolatileKnowledgeScope(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Uploads a file or content as volatile knowledge.
        /// </summary>
        /// <param name="request">The upload request containing file stream or content</param>
        /// <param name="processEmbeddings">Optional parameter to indicate whether to process embeddings. Default is true.</param>
        /// <param name="noExpiration">Optional parameter to indicate whether the knowledge should not expire. Default is false.</param>
        /// <param name="expirationDays">Optional parameter to specify the number of days until expiration.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created volatile knowledge entity</returns>
        public async Task<VolatileKnowledgeRes> UploadAsync(
            UploadVolatileKnowledgeReq request,
            bool processEmbeddings = true,
            bool noExpiration = false,
            int? expirationDays = null,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            using (MultipartFormDataContent content = new())
            {
                // Add content if provided
                if (!string.IsNullOrEmpty(request.Content))
                    content.Add(new StringContent(request.Content), "Content");

                // Add file if provided
                if (request.FileStream is not null)
                {
                    StreamContent fileContent = new(request.FileStream);
                    string contentType = GetContentType(request.FileName);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    content.Add(fileContent, "File", request.FileName);
                }

                // Add callback URL if provided
                if (!string.IsNullOrEmpty(request.CallbackUrl))
                    content.Add(new StringContent(request.CallbackUrl), "CallbackUrl");

                // Build query parameters
                List<string> queryParams = new List<string>
                {
                    $"processEmbeddings={processEmbeddings}",
                    $"noExpiration={noExpiration}"
                };

                if (expirationDays.HasValue)
                    queryParams.Add($"expirationDays={expirationDays.Value}");

                string queryString = string.Join("&", queryParams);
                string endpoint = $"/api/v2/volatileknowledge?{queryString}";

                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
                HttpResponseMessage response = await _apiClient.SendAsync(httpRequest, cancellationToken);

                return await response.ReadSerenityJsonAsync<VolatileKnowledgeRes>(cancellationToken);
            }
        }

        /// <summary>
        /// Gets the status of a volatile knowledge entity by its ID.
        /// </summary>
        /// <param name="knowledgeId">The knowledge ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The volatile knowledge entity with current status.</returns>
        public async Task<VolatileKnowledgeRes> GetStatusAsync(
            Guid knowledgeId,
            CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/v2/volatileknowledge/{knowledgeId}");

            HttpResponseMessage response = await _apiClient.SendAsync(request, cancellationToken);

            return await response.ReadSerenityJsonAsync<VolatileKnowledgeRes>(cancellationToken);
        }

        #region Private Methods
        private static string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".md" => "text/markdown",
                ".jpg" => "image/jpg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => throw new NotSupportedException($"File extension '{extension}' is not supported for upload."),
            };
        }
        #endregion
    }
}
