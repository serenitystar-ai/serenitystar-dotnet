using System.Collections.Generic;

namespace SerenityStar.Models.Conversation
{
    /// <summary>
    /// Represents a conversation retrieved by ID.
    /// </summary>
    public class ConversationResult
    {
        /// <summary>
        /// The conversation ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// List of messages in the conversation.
        /// </summary>
        public List<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();

        /// <summary>
        /// Indicates if the conversation is still open.
        /// </summary>
        public bool Open { get; set; }

        /// <summary>
        /// Detailed task execution logs (when showExecutorTaskLogs is true).
        /// </summary>
        public string? ExecutorTaskLogs { get; set; }
    }
}
