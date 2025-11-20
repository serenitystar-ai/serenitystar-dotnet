namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents content being streamed.
    /// </summary>
    public sealed class StreamingAgentMessageContent : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "content";

        /// <summary>
        /// The text content.
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }
}
