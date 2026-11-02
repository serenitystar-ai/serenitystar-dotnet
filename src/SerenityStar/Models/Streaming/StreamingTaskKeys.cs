namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Well-known task keys reported in <see cref="StreamingAgentMessageTaskStart.TaskKey"/> and
    /// <see cref="StreamingAgentMessageTaskStop.TaskKey"/>.
    /// </summary>
    public static class StreamingTaskKeys
    {
        /// <summary>
        /// The agent paused to request approval for a skill invocation.
        /// The task output is the requested <see cref="Execute.ApprovalPendingAction"/>.
        /// </summary>
        public const string ApprovalRequest = "pending_actions_approval_request";

        /// <summary>
        /// The agent resolved the approval decisions sent by the user and continued the execution.
        /// </summary>
        public const string ApprovalResolve = "pending_actions_approval_resolve";
    }
}
