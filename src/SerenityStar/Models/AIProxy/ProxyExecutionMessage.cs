using System.Collections.Generic;

namespace SerenityStar.Models.AIProxy
{
    /// <summary>
    /// Represents a message in a proxy execution.
    /// </summary>
    public sealed class ProxyExecutionMessage
    {
        /// <summary>
        /// The role of the message sender (e.g., "system", "user", "assistant").
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// The content of the message.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// The IDs of volatile knowledge items associated with this message.
        /// </summary>
        public List<string> VolatileKnowledgeIds { get; set; } = new();
    }
}
