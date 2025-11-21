using System;
using System.Collections.Generic;
using SerenityStar.Models.Execute;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents the completion of a streaming session.
    /// </summary>
    public sealed class StreamingAgentMessageStop : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "stop";

        /// <summary>
        /// The UTC time when the stream stopped.
        /// </summary>
        public DateTime StopTimeUtc { get; set; }

        /// <summary>
        /// The complete agent result with all execution details.
        /// </summary>
        public AgentResult? Result { get; set; }
    }
}
