using System.Collections.Generic;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Options for executing an agent.
    /// </summary>
    public sealed class AgentExecutionReq
    {
        /// <summary>
        /// Input parameters for the agent execution.
        /// </summary>
        public Dictionary<string, object>? InputParameters { get; set; }

        /// <summary>
        /// Specific version of the agent to execute.
        /// </summary>
        public int? AgentVersion { get; set; }

        /// <summary>
        /// User identifier for tracking and personalization.
        /// </summary>
        public string? UserIdentifier { get; set; }

        /// <summary>
        /// Channel identifier for the execution context.
        /// </summary>
        public string? Channel { get; set; }
    }
}
