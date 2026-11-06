using SerenityStar.Errors.Constants;
using System.Collections.Generic;

namespace SerenityStar.Errors.Models
{
    /// <summary>
    /// A business rule rejected the request, for example a required input is missing, the agent is inactive,
    /// or a quota is exhausted (<see cref="SerenityErrorCodes.ValidationError"/>, HTTP 400).
    /// </summary>
    /// <remarks>
    /// Agent-only data is kept in <see cref="SerenityApiError.ExtensionData"/> and read through the agents
    /// package's extension methods: <c>GetAgentResult()</c> when a system agent's output failed its JSON format,
    /// and <c>GetPendingActions()</c> when unanswered pending actions blocked the execution.
    /// </remarks>
    public sealed class ValidationError : SerenityApiError
    {
        /// <summary>
        /// The rule that failed, mapped to its message. See <see cref="ValidationErrorKeys"/>.
        /// Null when the API returned only a <see cref="SerenityApiError.Message"/>.
        /// </summary>
        public Dictionary<string, string>? Errors { get; set; }

        /// <summary>
        /// The raw model output that failed to parse. Set when a system agent's output failed its JSON or
        /// JSON Schema output format.
        /// </summary>
        public string? GeneratedJson { get; set; }
    }
}
