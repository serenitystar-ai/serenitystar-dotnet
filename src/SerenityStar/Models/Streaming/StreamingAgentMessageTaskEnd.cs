namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents the end of a task execution.
    /// </summary>
    public sealed class StreamingAgentMessageTaskEnd : StreamingAgentMessage
    {
        /// <summary>
        /// The key identifying the task.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// The result of the task execution.
        /// </summary>
        public object? Result { get; set; }

        /// <summary>
        /// The duration of the task in milliseconds.
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingAgentMessageTaskEnd"/> class.
        /// </summary>
        public StreamingAgentMessageTaskEnd()
        {
            Type = "task_end";
        }
    }
}
