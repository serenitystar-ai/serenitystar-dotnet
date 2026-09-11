using System.Collections.Generic;
using SerenityStar.Models.Citations;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents content being streamed.
    /// </summary>
    public sealed class StreamingAgentMessageContent : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "content";

        /// <summary>
        /// The text content.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Citations grounding this content in source knowledge, if any.
        /// </summary>
        public IReadOnlyList<CitationResult>? Citations { get; set; }
    }
}
