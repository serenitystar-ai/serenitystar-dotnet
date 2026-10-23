using SerenityStar.Models.SpeechGeneration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <summary>
    /// Generates speech (text-to-speech) from text.
    /// Obtain an instance with <see cref="Client.SerenityClientAIServicesExtensions.GetSpeechGenerationClient"/>.
    /// </summary>
    /// <remarks>
    /// This interface is intended to be consumed (and mocked in tests), not implemented: new members may be added in future versions.
    /// </remarks>
    public interface ISpeechGenerationClient
    {
        /// <summary>
        /// Generates speech from text and returns a public URL to download the audio.
        /// </summary>
        /// <param name="request">The speech generation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result including the audio URL, usage, and cost.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request or its text is null.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<SpeechGenerationResult> GenerateAsync(GenerateSpeechReq request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates speech from text, streaming the audio in chunks as it is generated (Server-Sent Events).
        /// The stream emits a start message, the audio chunks, and a final stop message with the same payload
        /// as <see cref="GenerateAsync"/>. Errors after the stream opened are emitted as <see cref="StreamingSpeechGenerationMessageError"/>.
        /// </summary>
        /// <param name="request">The speech generation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The stream of speech generation messages.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request or its text is null.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API rejects the request before the stream opens.</exception>
        IAsyncEnumerable<StreamingSpeechGenerationMessage> StreamAsync(GenerateSpeechReq request, CancellationToken cancellationToken = default);
    }
}
