using System;

namespace SerenityStar.Models.Transcription
{
    /// <summary>
    /// Request for transcribing an audio or video file that is already stored in the system.
    /// </summary>
    public sealed class TranscribeAudioByFileIdReq
    {
        /// <summary>
        /// The AI Model Id to use for transcription.
        /// If not provided, the default audio model will be used.
        /// </summary>
        public Guid? ModelId { get; set; }

        /// <summary>
        /// The file ID of the audio or video file to transcribe.
        /// The file must already exist in the system.
        /// </summary>
        public Guid FileId { get; set; }

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
    }
}
