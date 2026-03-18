using SerenityStar.Constants;
using SerenityStar.Models.Transcription;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <summary>
    /// Provides methods for audio transcription operations.
    /// </summary>
    public sealed class TranscriptionScope
    {
        private readonly HttpClient _httpClient;

        internal TranscriptionScope(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Transcribes an audio or video file by uploading it directly.
        /// Supported formats: mp3, mp4, mpeg, mpga, m4a, wav, webm.
        /// Maximum file size: 25 MB.
        /// </summary>
        /// <param name="request">The transcription request containing the file stream, file name, and optional parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transcription result including the transcribed text, metadata, token usage, and cost.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the request, file stream, or file name is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        public async Task<TranscribeResult> TranscribeAsync(
            TranscribeAudioReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            using MultipartFormDataContent content = new();

            // Add the audio file
            StreamContent fileContent = new(request.FileStream);
            string contentType = GetAudioContentType(request.FileName);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            content.Add(fileContent, "File", request.FileName);

            // Add optional parameters
            if (request.ModelId.HasValue)
                content.Add(new StringContent(request.ModelId.Value.ToString()), "ModelId");

            if (!string.IsNullOrEmpty(request.Prompt))
                content.Add(new StringContent(request.Prompt), "Prompt");

            if (!string.IsNullOrEmpty(request.UserIdentifier))
                content.Add(new StringContent(request.UserIdentifier), "UserIdentifier");

            if (!string.IsNullOrEmpty(request.Channel))
                content.Add(new StringContent(request.Channel), "Channel");

            HttpResponseMessage response = await _httpClient.PostAsync(
                "/api/v2/audio/transcribe",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<TranscribeResult>(JsonSerializerOptionsCache.s_camelCaseIgnoreNull, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize transcription result");
        }

        #region Private Methods
        private static string GetAudioContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".mp3" => "audio/mpeg",
                ".mp4" => "video/mp4",
                ".mpeg" => "audio/mpeg",
                ".mpga" => "audio/mpeg",
                ".m4a" => "audio/mp4",
                ".wav" => "audio/wav",
                ".webm" => "audio/webm",
                _ => "application/octet-stream",
            };
        }
        #endregion
    }
}
