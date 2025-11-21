using System;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents the end of a task execution.
    /// </summary>
    public sealed class StreamingAgentMessageTaskStop : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "task_stop";

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
        /// The UTC time when the task ended.
        /// </summary>
        public DateTime EndTimeUtc { get; set; }

        /// <summary>
        /// The duration of the task execution.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The output of the task execution.
        /// </summary>
        public object? Output { get; set; }
    }
}