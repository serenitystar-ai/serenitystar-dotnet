namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents the start of a task execution.
    /// </summary>
    public sealed class StreamingAgentMessageTaskStart : StreamingAgentMessage
    {
        /// <summary>
        /// The key identifying the task.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// The input to the task.
        /// </summary>
        public string Input { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingAgentMessageTaskStart"/> class.
        /// </summary>
        public StreamingAgentMessageTaskStart()
        {
            Type = "task_start";
        }
    }
}
