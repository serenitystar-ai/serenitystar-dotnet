namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents reasoning content being streamed.
    /// </summary>
    public sealed class StreamingAgentMessageReasoning : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "reasoning";

        /// <summary>
        /// The reasoning text.
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }
}
