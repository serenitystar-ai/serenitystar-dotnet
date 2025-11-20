using System.Text.Json;

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
        /// The key identifying the task.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// The result of the task execution as a JsonElement.
        /// </summary>
        public JsonElement? Result { get; set; }

        /// <summary>
        /// The duration of the task in milliseconds.
        /// </summary>
        public long DurationMs { get; set; }
    }
}