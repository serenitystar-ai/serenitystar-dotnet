using System;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents the start of a streaming session.
    /// </summary>
    public sealed class StreamingAgentMessageStart : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "start";

        /// <summary>
        /// The UTC time when the stream started.
        /// </summary>
        public DateTime StartTimeUtc { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingAgentMessageStart"/> class.
        /// </summary>
        public StreamingAgentMessageStart()
        {
            StartTimeUtc = DateTime.UtcNow;
        }
    }
}
