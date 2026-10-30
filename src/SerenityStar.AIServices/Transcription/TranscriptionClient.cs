using SerenityStar.Client;
using SerenityStar.Helpers;
using SerenityStar.Models.Transcription;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <inheritdoc />
    internal sealed class TranscriptionClient : ITranscriptionClient
    {
        private readonly SerenityApiClient _apiClient;

        internal TranscriptionClient(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <inheritdoc />
        public Task<TranscriptionResult> TranscribeAsync(
            TranscribeAudioReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            MultipartFormDataContent content = new();
            content.AddFile("File", request.FileStream, request.FileName);

            if (request.AIModelId.HasValue)
                content.AddField("ModelId", request.AIModelId.Value.ToString());

            content.AddField("Prompt", request.Prompt);
            content.AddField("UserIdentifier", request.UserIdentifier);
            content.AddField("Channel", request.Channel);

            return AIServiceRequestHelper.PostMultipartAsync<TranscriptionResult>(
                _apiClient, "/api/v2/audio/transcribe", content, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TranscriptionResult> TranscribeByFileIdAsync(
            TranscribeAudioByFileIdReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            return AIServiceRequestHelper.PostJsonAsync<TranscriptionResult>(
                _apiClient, "/api/v2/audio/transcribe/file", request, cancellationToken);
        }
    }
}
