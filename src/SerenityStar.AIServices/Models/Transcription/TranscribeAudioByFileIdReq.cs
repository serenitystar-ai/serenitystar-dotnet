using System;
using System.Text.Json.Serialization;

namespace SerenityStar.Models.Transcription
{
    /// <summary>
    /// Request for transcribing an audio or video file previously uploaded to Serenity Star.
    /// </summary>
    public sealed class TranscribeAudioByFileIdReq
    {
        /// <summary>
        /// The AI Model Id to use for transcription.
        /// If not provided, the default audio model will be used.
        /// </summary>
        [JsonPropertyName("modelId")]
        public Guid? AIModelId { get; set; }

        /// <summary>
        /// The id of the uploaded file to transcribe.
        /// Upload the file with <see cref="Client.SerenityClient.GetFileClient"/>.
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

        /// <summary>
        /// Validates the request parameters.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when FileId is empty.</exception>
        public void Validate()
        {
            if (FileId == Guid.Empty)
                throw new ArgumentException("File id is required for transcription.", nameof(FileId));
        }
    }
}
