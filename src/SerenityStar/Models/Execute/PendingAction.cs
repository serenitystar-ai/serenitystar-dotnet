using System.Collections.Generic;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Represents a pending action that requires user input or confirmation.
    /// </summary>
    public sealed class PendingAction
    {
        /// <summary>
        /// The unique identifier for this pending action.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The type of action that is pending.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// A human-readable description of the pending action.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Additional metadata or parameters for the pending action.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
