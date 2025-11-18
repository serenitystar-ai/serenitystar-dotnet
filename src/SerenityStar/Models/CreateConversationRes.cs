using System;

namespace SerenityStar.Models
{
    /// <summary>
    /// Represents the response from creating a conversation.
    /// </summary>
    public class CreateConversationRes
    {
        /// <summary>
        /// Gets or sets the chat ID.
        /// </summary>
        public Guid ChatId { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the conversation starters.
        /// </summary>
        public string[] ConversationStarters { get; set; } = new string[0];

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public int Version { get; set; }
    }
}