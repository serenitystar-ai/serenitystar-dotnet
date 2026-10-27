namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Known values of <see cref="AgentTodoItem.Status"/>.
    /// </summary>
    public static class AgentTodoStatus
    {
        /// <summary>
        /// The agent has not finished the item yet. The agent keeps iterating while any item is pending.
        /// </summary>
        public const string Pending = "Pending";

        /// <summary>
        /// The agent finished the item.
        /// </summary>
        public const string Completed = "Completed";

        /// <summary>
        /// The agent decided the item is no longer needed.
        /// </summary>
        public const string Canceled = "Canceled";
    }
}
