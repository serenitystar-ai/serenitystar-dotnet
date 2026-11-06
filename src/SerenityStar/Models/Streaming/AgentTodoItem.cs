namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// A task the agent planned for itself when Agent TODOs is enabled, together with its current state.
    /// </summary>
    public sealed class AgentTodoItem
    {
        /// <summary>
        /// The identifier of the item, stable for the whole execution.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A short description of the task.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The state of the item. See <see cref="AgentTodoStatus"/> for the known values.
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}
