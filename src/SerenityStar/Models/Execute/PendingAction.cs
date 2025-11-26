using System.Text.Json.Serialization;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Represents actions that need to be completed by the user before the agent can continue.
    /// </summary>
    [JsonDerivedType(typeof(ConnectionPendingAction))]
    public abstract class PendingAction
    {
        /// <summary>
        /// The type of pending action.
        /// </summary>
        public abstract string Type { get; }
    }
}
