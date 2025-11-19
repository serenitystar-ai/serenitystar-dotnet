using System.Collections.Generic;

namespace SerenityStar.Models.Conversation
{
    /// <summary>
    /// Represents a full conversation with messages.
    /// </summary>
    public sealed class ConversationDetails
    {
        /// <summary>
        /// The conversation ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Array of messages in the conversation.
        /// </summary>
        public List<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();

        /// <summary>
        /// Boolean that indicates if the conversation is open or closed.
        /// </summary>
        public bool Open { get; set; }

        /// <summary>
        /// Detailed task execution logs (only if requested).
        /// </summary>
        public List<object>? ExecutorTaskLogs { get; set; }
    }
}
