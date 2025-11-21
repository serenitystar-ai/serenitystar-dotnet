using System;
using System.Collections.Generic;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents the start of a task execution.
    /// </summary>
    public sealed class StreamingAgentMessageTaskStart : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "task_start";

        /// <summary>
        /// The name of the task.
        /// </summary>
        public string Task { get; set; } = string.Empty;

        /// <summary>
        /// The key identifying the task.
        /// </summary>
        public string TaskKey { get; set; } = string.Empty;

        /// <summary>
        /// The UTC time when the task started.
        /// </summary>
        public DateTime StartTimeUtc { get; set; }

        /// <summary>
        /// The input to the task.
        /// </summary>
        public IDictionary<string, object>? Input { get; set; }
    }
}
