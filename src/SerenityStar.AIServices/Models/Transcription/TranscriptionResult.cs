using SerenityStar.Models.AIServices;
using System;

namespace SerenityStar.Models.Transcription
{
    /// <summary>
    /// Represents the result of an audio transcription operation.
    /// </summary>
    public sealed class TranscriptionResult
    {
        /// <summary>
        /// The unique identifier of the audio transcription instance.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// The transcribed text from the audio file.
        /// </summary>
        public string Transcript { get; set; } = string.Empty;

        /// <summary>
        /// Metadata from the transcription service including language and duration.
        /// </summary>
        public TranscriptionMetadata? Metadata { get; set; }

        /// <summary>
        /// The billed quantities in the units the model is priced by.
        /// </summary>
        public AIServiceUsage? Usage { get; set; }

        /// <summary>
        /// The cost information for this transcription in the tenant's currency.
        /// </summary>
        public AIServiceTokenCost? Cost { get; set; }
    }

    /// <summary>
    /// Represents metadata information returned from the audio transcription service.
    /// </summary>
    public sealed class TranscriptionMetadata
    {
        /// <summary>
        /// The detected language of the audio content (ISO 639-1 format, e.g., "en", "es", "fr").
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// The total duration of the audio file.
        /// </summary>
        public TimeSpan? Duration { get; set; }
    }
}
