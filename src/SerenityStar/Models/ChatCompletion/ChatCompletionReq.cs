using System;
using System.Collections.Generic;

namespace SerenityStar.Models.ChatCompletion
{
    /// <summary>
    /// Options for chat completion execution.
    /// Provide either Message or AudioFileId, but not both.
    /// </summary>
    public sealed class ChatCompletionReq
    {
        /// <summary>
        /// The current message to send. Must be null when AudioFileId is provided.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Previous messages in the conversation.
        /// </summary>
        public List<ChatCompletionMessage>? Messages { get; set; }

        /// <summary>
        /// User identifier for tracking and personalization.
        /// </summary>
        public string? UserIdentifier { get; set; }

        /// <summary>
        /// Additional input parameters.
        /// </summary>
        public Dictionary<string, object>? InputParameters { get; set; }

        /// <summary>
        /// Optional file ID of a previously uploaded audio file.
        /// When set, the agent will transcribe the audio and use the transcription as input.
        /// Must be null when Message is provided.
        /// </summary>
        public Guid? AudioFileId { get; set; }

        /// <summary>
        /// Validates that either Message or AudioFileId is provided, but not both.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when both are provided or neither is provided.</exception>
        public void Validate()
        {
            if (Message != null && AudioFileId.HasValue)
                throw new ArgumentException("Cannot provide both a text message and an audio file ID. Use either Message or AudioFileId, but not both.");
            if (Message == null && !AudioFileId.HasValue)
                throw new ArgumentException("Either a text message or an audio file ID must be provided.");
        }
    }
}
