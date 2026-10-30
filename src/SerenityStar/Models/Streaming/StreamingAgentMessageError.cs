using System.Collections.Generic;
using SerenityStar.Models.Execute;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents an error during streaming.
    /// </summary>
    public sealed class StreamingAgentMessageError : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "error";

        /// <summary>
        /// The HTTP status code, if applicable.
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Validation errors, if applicable.
        /// </summary>
        public IDictionary<string, string>? Errors { get; set; }

        /// <summary>
        /// The pending actions that blocked the execution, if applicable.
        /// For example, the skill approvals left unanswered when continuing the execution.
        /// </summary>
        public List<PendingAction>? PendingActions { get; set; }
    }
}
