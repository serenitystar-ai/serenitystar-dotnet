using System;
using System.Collections.Generic;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Represents the result of an agent execution.
    /// </summary>
    public class AgentResult
    {
        /// <summary>
        /// The main content or response from the agent.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// The instance ID from the execution.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// Token usage statistics for the execution.
        /// </summary>
        public CompletionUsage? CompletionUsage { get; set; }

        /// <summary>
        /// Any pending actions that require user input.
        /// </summary>
        public List<PendingAction>? PendingActions { get; set; }

        /// <summary>
        /// Additional metadata from the execution.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Represents token usage statistics for an agent execution.
    /// </summary>
    public class CompletionUsage
    {
        /// <summary>
        /// Number of tokens in the prompt.
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// Number of tokens in the completion.
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Total number of tokens used.
        /// </summary>
        public int TotalTokens { get; set; }
    }
}