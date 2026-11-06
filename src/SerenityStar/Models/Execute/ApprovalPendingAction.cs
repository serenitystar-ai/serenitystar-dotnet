using System.Collections.Generic;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Represents a pending action that requires the user to approve or reject a skill invocation
    /// before the agent can execute it.
    /// </summary>
    /// <remarks>
    /// The agent pauses until every approval it requested is answered. Use <see cref="Approve"/> or <see cref="Reject"/>
    /// to build the decision and send it back to continue the execution.
    /// </remarks>
    public sealed class ApprovalPendingAction : PendingAction
    {
        /// <summary>
        /// The type of action that is pending (always "approval").
        /// </summary>
        public override string Type => "approval";

        /// <summary>
        /// The unique identifier of the approval request.
        /// It must be sent back along with the user's decision so the agent can resume the skill invocation.
        /// </summary>
        public string RequestId { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the skill invocation awaiting approval.
        /// </summary>
        public string CallId { get; set; } = string.Empty;

        /// <summary>
        /// The code of the skill the agent wants to invoke.
        /// </summary>
        public string? SkillCode { get; set; }

        /// <summary>
        /// The type of the skill the agent wants to invoke.
        /// </summary>
        public string? SkillType { get; set; }

        /// <summary>
        /// The name of the skill tool the agent requested.
        /// </summary>
        /// <remarks>
        /// Only differs from <see cref="SkillCode"/> for skills that expose more than one tool,
        /// and it is null for skills that don't expose their tool names.
        /// </remarks>
        public string? Tool { get; set; }

        /// <summary>
        /// The arguments the agent wants to invoke the skill with, so the user can review them before deciding.
        /// </summary>
        /// <remarks>
        /// The values are provided by the agent and can contain sensitive data: avoid logging them.
        /// </remarks>
        public Dictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Creates the decision that approves this skill invocation.
        /// </summary>
        /// <param name="reason">Optional note handed to the agent along with the approval.</param>
        /// <returns>The decision to send back to the agent.</returns>
        public ApprovalDecision Approve(string? reason = null)
            => new ApprovalDecision { RequestId = RequestId, Approved = true, Reason = reason };

        /// <summary>
        /// Creates the decision that rejects this skill invocation.
        /// </summary>
        /// <param name="reason">Optional reason handed to the agent, so it can adjust its response.</param>
        /// <returns>The decision to send back to the agent.</returns>
        public ApprovalDecision Reject(string? reason = null)
            => new ApprovalDecision { RequestId = RequestId, Approved = false, Reason = reason };
    }
}
