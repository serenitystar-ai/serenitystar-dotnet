using SerenityStar.Constants;
using SerenityStar.Exceptions;
using SerenityStar.Models.Execute;
using System.Collections.Generic;
using System.Text.Json;

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
        /// </summary>
        /// <param name="exception">The exception thrown by the agent execution.</param>
        /// <returns>The pending actions, or an empty list when the error doesn't report any.</returns>
        public static List<PendingAction> GetPendingActions(this SerenityApiException exception)
        {
            if (!(exception.ResponseJson is JsonElement { ValueKind: JsonValueKind.Object } payload))
                return new List<PendingAction>();

            foreach (JsonProperty property in payload.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Array)
                    continue;

                if (property.NameEquals("pendingActions"))
                    return property.Value.Deserialize<List<PendingAction>>(JsonSerializerOptionsCache.s_camelCase) ?? new List<PendingAction>();

                if (property.NameEquals("pending_actions"))
                    return property.Value.Deserialize<List<PendingAction>>(JsonSerializerOptionsCache.s_snakeCaseLower) ?? new List<PendingAction>();
            }

            return new List<PendingAction>();
        }
    }
}
