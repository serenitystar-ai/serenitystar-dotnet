using SerenityStar.Errors;
using SerenityStar.Models.Execute;
using System.Collections.Generic;

namespace SerenityStar.Extensions
{
    /// <summary>
    /// Agent-specific helpers for <see cref="SerenityApiException"/>.
    /// </summary>
    public static class SerenityApiExceptionExtensions
    {
        /// <summary>
        /// Gets the pending actions that blocked an agent execution, reported in the error payload.
        /// For example, the skill approvals left unanswered when continuing a conversation.
        /// Shortcut for <c>exception.Error.GetPendingActions()</c>.
        /// </summary>
        /// <param name="exception">The exception thrown by the agent execution.</param>
        /// <returns>The pending actions, or an empty list when the error doesn't report any.</returns>
        public static List<PendingAction> GetPendingActions(this SerenityApiException exception)
            => exception.Error.GetPendingActions();
    }
}
