using SerenityStar.Constants;
using SerenityStar.Errors.Models;
using SerenityStar.Models.Execute;
using System.Collections.Generic;
using System.Text.Json;

namespace SerenityStar.Extensions
{
    /// <summary>
    /// Agent-specific helpers for <see cref="SerenityApiError"/>. They read the agent data the API adds to an
    /// error, which is kept in <see cref="SerenityApiError.ExtensionData"/>.
    /// </summary>
    public static class SerenityApiErrorExtensions
    {
        /// <summary>
        /// Gets the pending actions that blocked an agent execution, reported in the error payload.
        /// For example, the skill approvals left unanswered when continuing a conversation.
        /// </summary>
        /// <param name="error">The error returned by the agent execution.</param>
        /// <returns>The pending actions, or an empty list when the error doesn't report any.</returns>
        public static List<PendingAction> GetPendingActions(this SerenityApiError error)
            => ReadExtension<List<PendingAction>>(error, "pendingActions", "pending_actions", JsonValueKind.Array)
               ?? new List<PendingAction>();

        /// <summary>
        /// Gets the agent's result, reported in the error payload when a system agent's output failed its JSON or
        /// JSON Schema output format.
        /// </summary>
        /// <param name="error">The error returned by the agent execution.</param>
        /// <returns>The agent's result, or <c>null</c> when the error doesn't report one.</returns>
        /// <remarks>
        /// The result can echo user input and model output. Treat it as sensitive when logging.
        /// </remarks>
        public static AgentResult? GetAgentResult(this SerenityApiError error)
            => ReadExtension<AgentResult>(error, "agentResult", "agent_result", JsonValueKind.Object);

        // HTTP responses use camelCase and streaming events use snake_case, so each name is read with its own casing.
        private static T? ReadExtension<T>(SerenityApiError error, string camelCaseName, string snakeCaseName, JsonValueKind kind)
            where T : class
        {
            if (error.ExtensionData is null)
                return null;

            if (error.ExtensionData.TryGetValue(camelCaseName, out JsonElement camelCase) && camelCase.ValueKind == kind)
                return camelCase.Deserialize<T>(JsonSerializerOptionsCache.s_camelCase);

            if (error.ExtensionData.TryGetValue(snakeCaseName, out JsonElement snakeCase) && snakeCase.ValueKind == kind)
                return snakeCase.Deserialize<T>(JsonSerializerOptionsCache.s_snakeCaseLower);

            return null;
        }
    }
}
