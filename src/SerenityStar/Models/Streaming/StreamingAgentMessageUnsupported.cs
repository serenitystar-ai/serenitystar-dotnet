using System.Text.Json;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents a streaming message whose <c>type</c> is not recognized by this SDK version.
    /// This is emitted (instead of an error) when the API introduces a new message type, so that
    /// consumers can safely ignore or inspect it without the stream failing.
    /// </summary>
    public sealed class StreamingAgentMessageUnsupported : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "unsupported";

        /// <summary>
        /// The original, unrecognized <c>type</c> value reported by the API.
        /// </summary>
        public string OriginalType { get; set; } = string.Empty;

        /// <summary>
        /// The raw JSON payload of the message, so consumers can inspect it if needed.
        /// This is a detached copy, safe to read after deserialization completes.
        /// </summary>
        public JsonElement? RawData { get; set; }
    }
}
