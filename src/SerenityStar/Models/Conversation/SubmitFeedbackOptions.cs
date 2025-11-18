namespace SerenityStar.Models.Conversation
{
    /// <summary>
    /// Options for submitting feedback on a message.
    /// </summary>
    public class SubmitFeedbackOptions
    {
        /// <summary>
        /// The ID of the agent message to provide feedback for.
        /// </summary>
        public string AgentMessageId { get; set; } = string.Empty;

        /// <summary>
        /// The feedback value - true for positive, false for negative.
        /// </summary>
        public bool Feedback { get; set; }
    }
}
