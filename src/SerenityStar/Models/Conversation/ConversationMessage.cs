using System;

namespace SerenityStar.Models.Conversation
{
    /// <summary>
    /// Represents a conversation message.
    /// </summary>
    public class ConversationMessage
    {
        /// <summary>
        /// The sender of the message ("user" or "bot").
        /// </summary>
        public string Sender { get; set; } = string.Empty;

        /// <summary>
        /// The date when the message was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The type of the message (e.g., "text", "image", "error").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The content of the message.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// The message ID.
        /// </summary>
        public string? Id { get; set; }
    }
}
