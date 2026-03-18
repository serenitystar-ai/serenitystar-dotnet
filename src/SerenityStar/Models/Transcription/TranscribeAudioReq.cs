using System;
using System.IO;

namespace SerenityStar.Models.Transcription
{
    /// <summary>
    /// Request for transcribing an audio or video file by uploading it directly.
    /// </summary>
    public sealed class TranscribeAudioReq
    {
        /// <summary>
        /// The AI Model Id to use for transcription.
        /// If not provided, the default audio model will be used.
        /// </summary>
        public Guid? ModelId { get; set; }

        /// <summary>
        /// The audio or video file stream to transcribe.
        /// Supported formats: mp3, mp4, mpeg, mpga, m4a, wav, webm.
        /// Maximum file size: 25 MB.
        /// </summary>
        public Stream FileStream { get; set; } = null!;

        /// <summary>
        /// The file name including extension (e.g., "audio.mp3").
        /// </summary>
        public string FileName { get; set; } = null!;

        /// <summary>
        /// Optional prompt to guide the transcription.
        /// Useful for providing context or specifying terminology.
        /// </summary>
        public string? Prompt { get; set; }

        /// <summary>
        /// Optional user identifier for tracking and cost management purposes.
        /// </summary>
        public string? UserIdentifier { get; set; }

        /// <summary>
        /// The channel used in the execution.
        /// </summary>
        public string? Channel { get; set; }

        /// <summary>
        /// Validates the request parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when FileStream or FileName is null.</exception>
        public void Validate()
        {
            if (FileStream is null)
                throw new ArgumentNullException(nameof(FileStream), "File stream is required for transcription.");
            if (string.IsNullOrEmpty(FileName))
                throw new ArgumentNullException(nameof(FileName), "File name is required for transcription.");
        }
    }
}
