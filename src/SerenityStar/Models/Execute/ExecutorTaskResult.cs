namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Represents the result of an executor task.
    /// </summary>
    public class ExecutorTaskResult
    {
        /// <summary>
        /// Gets or sets the task name.
        /// </summary>
        public string TaskName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the duration in milliseconds.
        /// </summary>
        public long DurationInMs { get; set; }
    }
}