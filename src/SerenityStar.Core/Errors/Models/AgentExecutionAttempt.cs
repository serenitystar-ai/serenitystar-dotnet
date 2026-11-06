using SerenityStar.Errors.Constants;

namespace SerenityStar.Errors.Models
{
    /// <summary>
    /// One attempt to call the AI model during an agent execution.
    /// </summary>
    public sealed class AgentExecutionAttempt : ExecutionAttempt
    {
        /// <summary>
        /// Which model the attempt used. See <see cref="AgentModelTypes"/>.
        /// </summary>
        public string ModelType { get; set; } = string.Empty;
    }
}
