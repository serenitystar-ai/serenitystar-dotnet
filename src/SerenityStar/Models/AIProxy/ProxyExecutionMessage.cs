using System.Collections.Generic;
using SerenityStar.Models.Execute;

namespace SerenityStar.Models.AIProxy
{
    /// <summary>
    /// Represents a message in a proxy execution.
    /// </summary>
    public sealed class ProxyExecutionMessage
    {
        /// <summary>
        /// The role of the message sender (e.g., "system", "user", "assistant").
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// The content of the message.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// The IDs of volatile knowledge items associated with this message.
        /// </summary>
        public List<string> VolatileKnowledgeIds { get; set; } = new();

        /// <summary>
        /// On an assistant message, the skill approval the agent requested in a previous execution, echoed back so the
        /// agent can resume the skill invocation.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ProxyExecutionReq.AddApprovalDecision"/> to add it along with the user's decision.
        /// </remarks>
        public ApprovalPendingAction? ToolApprovalRequest { get; set; }

        /// <summary>
        /// On a user message, the decision the user made on the approval requested by the preceding assistant message.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ProxyExecutionReq.AddApprovalDecision"/> to add it along with the approval request.
        /// </remarks>
        public ApprovalDecision? ToolApprovalResponse { get; set; }
    }
}
