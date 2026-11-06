using SerenityStar.Models.Transcription;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <summary>
    /// Transcribes audio and video files into text.
    /// Obtain an instance with <see cref="Client.SerenityClientAIServicesExtensions.GetTranscriptionClient"/>.
    /// </summary>
    /// <remarks>
    /// This interface is intended to be consumed (and mocked in tests), not implemented: new members may be added in future versions.
    /// </remarks>
    public interface ITranscriptionClient
    {
        /// <summary>
        /// Transcribes an audio or video file by uploading it directly.
        /// The file stream is read from its current position and disposed when the request completes.
        /// </summary>
        /// <param name="request">The transcription request containing the file stream, file name, and optional parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transcription result including the transcribed text, metadata, usage, and cost.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request, file stream, or file name is null.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<TranscriptionResult> TranscribeAsync(TranscribeAudioReq request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transcribes an audio or video file previously uploaded to Serenity Star.
        /// </summary>
        /// <param name="request">The transcription request containing the file id and optional parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transcription result including the transcribed text, metadata, usage, and cost.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the file id is empty.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<TranscriptionResult> TranscribeByFileIdAsync(TranscribeAudioByFileIdReq request, CancellationToken cancellationToken = default);
    }
}
