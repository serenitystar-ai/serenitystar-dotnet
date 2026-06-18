using SerenityStar.Constants;
using SerenityStar.Models.VolatileKnowledge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Agents.VolatileKnowledge
{
    /// <summary>
    /// Provides methods for managing volatile knowledge within a conversation or activity context.
    /// Uploaded knowledge is automatically associated with the parent conversation/activity.
    /// </summary>
    public sealed class ConversationVolatileKnowledgeScope
    {
        private readonly HttpClient _httpClient;
        private readonly string _agentCode;
        private readonly List<Guid> _knowledgeIds;

        internal ConversationVolatileKnowledgeScope(HttpClient httpClient, string agentCode)
        {
            _httpClient = httpClient;
            _agentCode = agentCode;
            _knowledgeIds = new List<Guid>();
        }

        /// <summary>
        /// Gets the list of volatile knowledge IDs associated with this conversation/activity.
        /// </summary>
        internal IReadOnlyList<Guid> KnowledgeIds => _knowledgeIds.AsReadOnly();

        /// <summary>
        /// Uploads a file or content as volatile knowledge and associates it with the current conversation/activity.
        /// The knowledge will be automatically included in the next message sent.
        /// </summary>
        /// <remarks>
        /// This uses the subtenant-scoped endpoint and does not validate the file type against an agent's
        /// configuration. To validate the file type against the agent's allowed MIME types at upload time,
        /// use <see cref="UploadForAgentAsync"/>.
        /// </remarks>
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

            using (MultipartFormDataContent content = BuildMultipartContent(request))
            {
                string endpoint = $"/api/v2/volatileknowledge?{BuildQueryString(processEmbeddings, noExpiration, expirationDays)}";

                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

                return await HandleUploadResponseAsync(response, cancellationToken);
            }
        }

        /// <summary>
        /// Uploads a file or content as volatile knowledge scoped to this agent and associates it with the
        /// current conversation/activity. The file type is validated against the agent's configured allowed
        /// MIME types at upload time.
        /// </summary>
        /// <param name="request">The upload request containing file stream or content</param>
        /// <param name="processEmbeddings">Optional parameter to indicate whether to process embeddings. Default is true.</param>
        /// <param name="noExpiration">Optional parameter to indicate whether the knowledge should not expire. Default is false.</param>
        /// <param name="expirationDays">Optional parameter to specify the number of days until expiration.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created volatile knowledge entity, associated with the agent.</returns>
        public async Task<VolatileKnowledgeRes> UploadForAgentAsync(
            UploadVolatileKnowledgeReq request,
            bool processEmbeddings = true,
            bool noExpiration = false,
            int? expirationDays = null,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            using (MultipartFormDataContent content = BuildMultipartContent(request))
            {
                string endpoint = $"/api/agent/{_agentCode}/volatileKnowledge?{BuildQueryString(processEmbeddings, noExpiration, expirationDays)}";

                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

                return await HandleUploadResponseAsync(response, cancellationToken);
            }
        }

        /// <summary>
        /// Uploads volatile knowledge from an existing file ID, scoped to this agent.
        /// The file type is validated against the agent's configured allowed MIME types at upload time.
        /// </summary>
        /// <param name="request">The upload request containing the file ID and options.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created volatile knowledge entity, associated with the agent.</returns>
        public async Task<VolatileKnowledgeRes> UploadFromFileForAgentAsync(
            UploadVolatileKnowledgeFromFileReq request,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                $"/api/agent/{_agentCode}/volatileKnowledge/upload/file",
                request,
                JsonSerializerOptionsCache.s_camelCase,
                cancellationToken);

            return await HandleUploadResponseAsync(response, cancellationToken);
        }

        /// <summary>
        /// Uploads volatile knowledge from a URL, scoped to this agent.
        /// The file type is validated against the agent's configured allowed MIME types at upload time.
        /// </summary>
        /// <param name="request">The upload request containing the file URL and options.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created volatile knowledge entity, associated with the agent.</returns>
        public async Task<VolatileKnowledgeRes> UploadFromUrlForAgentAsync(
            UploadVolatileKnowledgeFromUrlReq request,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                $"/api/agent/{_agentCode}/volatileKnowledge/upload/url",
                request,
                JsonSerializerOptionsCache.s_camelCase,
                cancellationToken);

            return await HandleUploadResponseAsync(response, cancellationToken);
        }

        /// <summary>
        /// Uploads volatile knowledge from a base64-encoded file, scoped to this agent.
        /// The file type is validated against the agent's configured allowed MIME types at upload time.
        /// </summary>
        /// <param name="request">The upload request containing the base64 content and options.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created volatile knowledge entity, associated with the agent.</returns>
        public async Task<VolatileKnowledgeRes> UploadFromBase64ForAgentAsync(
            UploadVolatileKnowledgeFromBase64Req request,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                $"/api/agent/{_agentCode}/volatileKnowledge/upload/base64",
                request,
                JsonSerializerOptionsCache.s_camelCase,
                cancellationToken);

            return await HandleUploadResponseAsync(response, cancellationToken);
        }

        /// <summary>
        /// Gets the MIME types allowed for volatile knowledge uploads to this agent.
        /// If the agent has no configured types, all platform-supported types are returned.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The list of allowed MIME types.</returns>
        public async Task<IReadOnlyList<string>> GetAllowedMimeTypesAsync(
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                $"/api/agent/{_agentCode}/volatileKnowledge/mimeTypes",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            List<string>? mimeTypes = await response.Content.ReadFromJsonAsync<List<string>>(JsonSerializerOptionsCache.s_camelCase, cancellationToken: cancellationToken);

            return mimeTypes ?? throw new InvalidOperationException("Failed to deserialize allowed MIME types response");
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
            HttpResponseMessage response = await _httpClient.GetAsync(
                $"/api/v2/volatileknowledge/{knowledgeId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            VolatileKnowledgeRes? volatileKnowledge = await response.Content.ReadFromJsonAsync<VolatileKnowledgeRes>(JsonSerializerOptionsCache.s_camelCase, cancellationToken: cancellationToken);

            return volatileKnowledge ?? throw new InvalidOperationException("Failed to deserialize volatile knowledge response");
        }

        /// <summary>
        /// Clears all volatile knowledge IDs from the current conversation/activity.
        /// This is called automatically after sending a message to prevent the knowledge from being included in subsequent messages.
        /// </summary>
        internal void ClearKnowledgeIds()
        {
            _knowledgeIds.Clear();
        }

        #region Private Methods

        private MultipartFormDataContent BuildMultipartContent(UploadVolatileKnowledgeReq request)
        {
            MultipartFormDataContent content = new();

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

            return content;
        }

        private static string BuildQueryString(bool processEmbeddings, bool noExpiration, int? expirationDays)
        {
            List<string> queryParams = new List<string>
            {
                $"processEmbeddings={processEmbeddings}",
                $"noExpiration={noExpiration}"
            };

            if (expirationDays.HasValue)
                queryParams.Add($"expirationDays={expirationDays.Value}");

            return string.Join("&", queryParams);
        }

        private async Task<VolatileKnowledgeRes> HandleUploadResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            VolatileKnowledgeRes? volatileKnowledge = await response.Content.ReadFromJsonAsync<VolatileKnowledgeRes>(JsonSerializerOptionsCache.s_camelCase, cancellationToken: cancellationToken);

            if (volatileKnowledge is null)
                throw new InvalidOperationException("Failed to deserialize volatile knowledge response");

            // Add to the list of knowledge IDs for automatic association
            _knowledgeIds.Add(volatileKnowledge.Id);

            return volatileKnowledge;
        }

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
                // Fall back to a generic content type for unknown extensions; the backend validates
                // the actual allowed types (globally and per-agent) at upload time.
                _ => "application/octet-stream",
            };
        }
        #endregion
    }
}
