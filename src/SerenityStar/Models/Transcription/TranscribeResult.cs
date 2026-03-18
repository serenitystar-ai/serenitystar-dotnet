using System;

namespace SerenityStar.Models.Transcription
{
    /// <summary>
    /// Represents the result of an audio transcription operation.
    /// </summary>
    public sealed class TranscribeResult
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
        /// Representation of the token counts processed for the transcription request.
        /// </summary>
        public TranscribeTokenUsage? TokenUsage { get; set; }

        /// <summary>
        /// The cost information for this transcription in the tenant's currency.
        /// </summary>
        public TranscribeCost? Cost { get; set; }
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

    /// <summary>
    /// Represents the token usage for an audio transcription operation.
    /// </summary>
    public sealed class TranscribeTokenUsage
    {
        /// <summary>
        /// The number of tokens in the provided prompts for the transcription request.
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// The number of tokens generated across all completions emissions.
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// The total number of tokens processed for the transcription request and response.
        /// </summary>
        public int TotalTokens { get; set; }
    }

    /// <summary>
    /// Represents the cost information for an audio transcription in the tenant's currency.
    /// </summary>
    public sealed class TranscribeCost
    {
        /// <summary>
        /// The cost for prompt tokens in the tenant's currency.
        /// </summary>
        public double Prompt { get; set; }

        /// <summary>
        /// The cost for completion tokens in the tenant's currency.
        /// </summary>
        public double Completion { get; set; }

        /// <summary>
        /// The total cost (prompt + completion) in the tenant's currency.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// The ISO currency code for the tenant's currency.
        /// </summary>
        public string Currency { get; set; } = string.Empty;
    }
}
