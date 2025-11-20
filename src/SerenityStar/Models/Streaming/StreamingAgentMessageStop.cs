using System;
using System.Collections.Generic;
using SerenityStar.Models.Execute;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents the completion of a streaming session.
    /// </summary>
    public sealed class StreamingAgentMessageStop : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "stop";

        /// <summary>
        /// The complete content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// The instance ID of the agent.
        /// </summary>
        public Guid? InstanceId { get; set; }

        /// <summary>
        /// Token usage information.
        /// </summary>
        public AgentResult.TokenUsage? CompletionUsage { get; set; }

        /// <summary>
        /// Time to first token in milliseconds.
        /// </summary>
        public long? TimeToFirstToken { get; set; }

        /// <summary>
        /// Executor task logs.
        /// </summary>
        public List<AgentResult.Task>? ExecutorTaskLogs { get; set; }

        /// <summary>
        /// Action results from the agent.
        /// </summary>
        public Dictionary<string, object>? ActionResults { get; set; }

        /// <summary>
        /// The agent message ID.
        /// </summary>
        public string? AgentMessageId { get; set; }

        /// <summary>
        /// The user message ID.
        /// </summary>
        public string? UserMessageId { get; set; }

        /// <summary>
        /// The complete agent result with all execution details.
        /// </summary>
        public AgentResult? Result { get; set; }
    }
}
