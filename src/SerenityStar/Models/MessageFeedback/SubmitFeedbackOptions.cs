using System;

namespace SerenityStar.Models.MessageFeedback
{
    /// <summary>
    /// Options for submitting feedback on a message.
    /// </summary>
    public class SubmitFeedbackOptions
    {
        /// <summary>
        /// The ID of the agent message to provide feedback for.
        /// </summary>
        public Guid AgentMessageId { get; set; } = Guid.Empty;

        /// <summary>
        /// The feedback value - true for positive, false for negative.
        /// </summary>
        public bool Feedback { get; set; }
    }
}
