namespace SerenityStar.Models.ChatCompletion
{
    /// <summary>
    /// Represents a message in a chat completion conversation.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// The role of the message sender (e.g., "system", "user", "assistant").
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// The content of the message.
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
