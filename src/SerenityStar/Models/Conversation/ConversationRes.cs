using System;
using System.Collections.Generic;

namespace SerenityStar.Models.Conversation
{
    /// <summary>
    /// Represents a conversation retrieved by ID.
    /// </summary>
    public sealed class ConversationRes
    {
        /// <summary>
        /// The conversation ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The conversation name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The conversation start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The conversation end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// JSON representation of messages in the conversation.
        /// </summary>
        public string MessagesJson { get; set; } = string.Empty;

        /// <summary>
        /// List of messages in the conversation.
        /// </summary>
        public List<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();

        /// <summary>
        /// User identifier for the conversation.
        /// </summary>
        public string UserIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// List of conversation starters.
        /// </summary>
        public List<string> ConversationStarters { get; set; } = new List<string>();

        /// <summary>
        /// Indicates if vision is enabled for this conversation.
        /// </summary>
        public bool UseVision { get; set; }

        /// <summary>
        /// Indicates if the conversation is still open.
        /// </summary>
        public bool Open { get; set; }

        /// <summary>
        /// Detailed task execution logs (when showExecutorTaskLogs is true).
        /// </summary>
        public List<object>? ExecutorTaskLogs { get; set; }
    }
}
