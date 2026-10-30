namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// The decision the user made on a skill approval requested by the agent (<see cref="ApprovalPendingAction"/>).
    /// </summary>
    /// <remarks>
    /// Create it with <see cref="ApprovalPendingAction.Approve"/> or <see cref="ApprovalPendingAction.Reject"/>.
    /// </remarks>
    public sealed class ApprovalDecision
    {
        /// <summary>
        /// The identifier of the approval request being answered (<see cref="ApprovalPendingAction.RequestId"/>).
        /// </summary>
        public string RequestId { get; set; } = string.Empty;

        /// <summary>
        /// Whether the skill invocation is approved.
        /// </summary>
        public bool Approved { get; set; }

        /// <summary>
        /// Optional reason for the decision, handed to the agent. Usually set when rejecting.
        /// </summary>
        public string? Reason { get; set; }
    }
}
