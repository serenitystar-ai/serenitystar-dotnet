using System.Collections.Generic;

namespace SerenityStar.Models.ChatCompletion
{
    /// <summary>
    /// Options for chat completion execution.
    /// </summary>
    public class ChatCompletionOptions
    {
        /// <summary>
        /// The current message to send.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Previous messages in the conversation.
        /// </summary>
        public List<ChatMessage>? Messages { get; set; }

        /// <summary>
        /// User identifier for tracking and personalization.
        /// </summary>
        public string? UserIdentifier { get; set; }

        /// <summary>
        /// Additional input parameters.
        /// </summary>
        public Dictionary<string, object>? InputParameters { get; set; }

        /// <summary>
        /// Specific version of the agent to execute.
        /// </summary>
        public int? AgentVersion { get; set; }
    }
}
