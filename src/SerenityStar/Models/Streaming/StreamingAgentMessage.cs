namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Base class for all streaming agent messages.
    /// </summary>
    public abstract class StreamingAgentMessage
    {
        /// <summary>
        /// The type of the streaming message.
        /// </summary>
        public string Type { get; set; } = string.Empty;
    }
}
