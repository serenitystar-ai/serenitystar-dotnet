using System;

namespace SerenityStar.Models.MessageFeedback
{
    /// <summary>
    /// Options for removing feedback from a message.
    /// </summary>
    public sealed class RemoveFeedbackReq
    {
        /// <summary>
        /// The ID of the agent message to remove feedback from.
        /// </summary>
        public Guid AgentMessageId { get; set; } = Guid.Empty;
    }
}
