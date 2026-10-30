using System;
using System.Collections.Generic;
using SerenityStar.Models.Execute;

namespace SerenityStar.Models.AIProxy
{
    /// <summary>
    /// Options for executing a proxy agent.
    /// </summary>
    public sealed class ProxyExecutionReq
    {
        /// <summary>
        /// The model to use for the execution.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// The messages to send to the model.
        /// </summary>
        public List<ProxyExecutionMessage> Messages { get; set; } = new List<ProxyExecutionMessage>();

        /// <summary>
        /// Temperature for response randomness (0.0 to 2.0).
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Maximum number of tokens to generate.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Top-p sampling parameter.
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// Top-k sampling parameter.
        /// </summary>
        public int? TopK { get; set; }

        /// <summary>
        /// Frequency penalty parameter.
        /// </summary>
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// Presence penalty parameter.
        /// </summary>
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// User identifier for tracking.
        /// </summary>
        public string? UserIdentifier { get; set; }

        /// <summary>
        /// Vendor identifier.
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// Group identifier.
        /// </summary>
        public string? GroupIdentifier { get; set; }

        /// <summary>
        /// Indicates if vision capabilities should be used.
        /// </summary>
        public bool? UseVision { get; set; }

        /// <summary>
        /// The skills available to the model in this execution. When null, no skills are sent.
        /// </summary>
        public ProxySkillsOptions? Skills { get; set; }

        /// <summary>
        /// Appends a skill approval the agent requested and the user's decision on it to <see cref="Messages"/>,
        /// so the next execution resumes the skill invocation (when approved) or declines it.
        /// </summary>
        /// <remarks>
        /// AI Proxy keeps the chat history on the client, so the approval request is echoed back as an assistant message
        /// followed by a user message carrying the decision. Add one pair per pending approval right before executing
        /// again: only the approvals at the end of the history are resolved.
        /// </remarks>
        /// <param name="approval">The approval the agent requested, from the previous execution's pending actions.</param>
        /// <param name="decision">
        /// The user's decision, created with <see cref="ApprovalPendingAction.Approve"/> or <see cref="ApprovalPendingAction.Reject"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when the approval or the decision is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the decision answers a different approval request.</exception>
        public void AddApprovalDecision(ApprovalPendingAction approval, ApprovalDecision decision)
        {
            if (approval == null)
                throw new ArgumentNullException(nameof(approval));
            if (decision == null)
                throw new ArgumentNullException(nameof(decision));
            if (!string.Equals(approval.RequestId, decision.RequestId, StringComparison.Ordinal))
                throw new ArgumentException("The decision must answer the given approval request.", nameof(decision));

            Messages.Add(new ProxyExecutionMessage { Role = "assistant", ToolApprovalRequest = approval });
            Messages.Add(new ProxyExecutionMessage { Role = "user", ToolApprovalResponse = decision });
        }
    }
}
