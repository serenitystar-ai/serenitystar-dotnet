using System;
using System.Collections.Generic;
using System.IO;

namespace SerenityStar.Models.ChatCompletion
{
    /// <summary>
    /// Options for chat completion execution.
    /// Provide either Message or AudioFileStream, but not both.
    /// </summary>
    public sealed class ChatCompletionReq
    {
        /// <summary>
        /// The current message to send. Must be null when AudioFileStream is provided.
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
        /// Optional audio file stream to upload and send as input.
        /// The SDK uploads the file automatically and sends the resulting file ID to the agent.
        /// Must be null when Message is provided.
        /// </summary>
        public Stream? AudioFileStream { get; set; }

        /// <summary>
        /// The file name including extension for the audio file (e.g., "recording.mp3").
        /// Required when AudioFileStream is provided.
        /// </summary>
        public string? AudioFileName { get; set; }

        /// <summary>
        /// Validates that either Message or AudioFileStream is provided, but not both.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when both are provided or neither is provided.</exception>
        /// <exception cref="ArgumentNullException">Thrown when AudioFileStream is provided without AudioFileName.</exception>
        public void Validate()
        {
            if (Message != null && AudioFileStream != null)
                throw new ArgumentException("Cannot provide both a text message and an audio file stream. Use either Message or AudioFileStream, but not both.");
            if (Message == null && AudioFileStream == null)
                throw new ArgumentException("Either a text message or an audio file stream must be provided.");
            if (AudioFileStream != null && string.IsNullOrEmpty(AudioFileName))
                throw new ArgumentNullException(nameof(AudioFileName), "Audio file name is required when providing an audio file stream.");
        }
    }
}
