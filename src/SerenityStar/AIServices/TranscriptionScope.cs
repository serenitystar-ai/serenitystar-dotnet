using SerenityStar.Client;
using SerenityStar.Extensions;
using SerenityStar.Models.Transcription;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <summary>
    /// Provides methods for audio transcription operations.
    /// </summary>
    public sealed class TranscriptionScope
    {
        private readonly SerenityApiClient _apiClient;

        internal TranscriptionScope(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
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
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
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

            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v2/audio/transcribe")
            {
                Content = content
            };

            HttpResponseMessage response = await _apiClient.SendAsync(httpRequest, cancellationToken);

            return await response.ReadSerenityJsonAsync<TranscribeResult>(cancellationToken);
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
