using SerenityStar.Errors.Constants;
using System.Collections.Generic;

namespace SerenityStar.Errors.Models
{
    /// <summary>
    /// An agent's call to the AI model failed after exhausting all of its attempts: the main model plus any
    /// fallbacks (<see cref="SerenityErrorCodes.AgentExecutionFailed"/>).
    /// </summary>
    /// <remarks>
    /// On HTTP the status is 400 when any attempt was client-fixable, 500 when any attempt was a server-side fault,
    /// 429 when every attempt was rate limited by the provider, and 502 otherwise.
    /// </remarks>
    public sealed class AgentExecutionFailedError : SerenityApiError
    {
        /// <summary>
        /// The errors of the last attempt.
        /// </summary>
        public Dictionary<string, string>? Errors { get; set; }

        /// <summary>
        /// Every attempt made, in order.
        /// </summary>
        public List<AgentExecutionAttempt> Attempts { get; set; } = new();
    }
}
