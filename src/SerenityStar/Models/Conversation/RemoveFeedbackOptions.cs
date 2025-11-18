namespace SerenityStar.Models.Conversation
{
    /// <summary>
    /// Options for removing feedback from a message.
    /// </summary>
    public class RemoveFeedbackOptions
    {
        /// <summary>
        /// The ID of the agent message to remove feedback from.
        /// </summary>
        public string AgentMessageId { get; set; } = string.Empty;
    }
}
