namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents a keep-alive ping during streaming.
    /// </summary>
    public sealed class StreamingAgentMessagePing : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "ping";
    }
}
