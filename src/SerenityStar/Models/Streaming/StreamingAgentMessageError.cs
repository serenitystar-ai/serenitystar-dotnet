namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents an error during streaming.
    /// </summary>
    public sealed class StreamingAgentMessageError : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "error";

        /// <summary>
        /// The error message.
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// The HTTP status code, if applicable.
        /// </summary>
        public int? StatusCode { get; set; }
    }
}
