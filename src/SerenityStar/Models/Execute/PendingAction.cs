using System.Text.Json.Serialization;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Represents actions that need to be completed by the user before the agent can continue.
    /// </summary>
    /// <remarks>
    /// The concrete type is resolved from the <c>type</c> property: <see cref="ConnectionPendingAction"/> ("connection"),
    /// <see cref="ApprovalPendingAction"/> ("approval"), and <see cref="UnsupportedPendingAction"/> for any type this SDK
    /// version does not recognize.
    /// </remarks>
    [JsonConverter(typeof(PendingActionJsonConverter))]
    public abstract class PendingAction
    {
        /// <summary>
        /// The type of pending action.
        /// </summary>
        public abstract string Type { get; }
    }
}
