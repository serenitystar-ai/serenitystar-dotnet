namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents content being streamed.
    /// </summary>
    public sealed class StreamingAgentMessageContent : StreamingAgentMessage
    {
        /// <summary>
        /// The text content.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingAgentMessageContent"/> class.
        /// </summary>
        public StreamingAgentMessageContent()
        {
            Type = "content";
        }
    }
}
