using SerenityStar.Errors.Constants;
using System.Collections.Generic;

namespace SerenityStar.Errors.Models
{
    /// <summary>
    /// An AI service request (embeddings, image generation, vision, speech, transcription, OCR) failed at the
    /// AI provider (<see cref="SerenityErrorCodes.AIServiceExecutionFailed"/>).
    /// </summary>
    /// <remarks>
    /// On HTTP the status is 400 when any attempt was client-fixable, 500 when any attempt was a server-side fault,
    /// 429 when every attempt was rate limited by the provider, and 502 otherwise.
    /// </remarks>
    public sealed class AIServiceExecutionFailedError : SerenityApiError
    {
        /// <summary>
        /// The errors of the last attempt. On OCR responses, keyed by file.
        /// </summary>
        public Dictionary<string, string>? Errors { get; set; }

        /// <summary>
        /// Every attempt made, in order. Null on OCR responses, which use <see cref="Files"/> instead.
        /// </summary>
        public List<ExecutionAttempt>? Attempts { get; set; }

        /// <summary>
        /// OCR only: the attempts made for each submitted file. Null on every other endpoint.
        /// </summary>
        public List<FileExecutionFailure>? Files { get; set; }
    }
}
