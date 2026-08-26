using System;

namespace SerenityStar.Models.MessageFeedback
{
    /// <summary>
    /// Options for submitting feedback on a message.
    /// </summary>
    public sealed class SubmitFeedbackReq
    {
        /// <summary>
        /// The ID of the agent message to provide feedback for.
        /// </summary>
        public Guid AgentMessageId { get; set; } = Guid.Empty;

        /// <summary>
        /// The feedback value - true for positive, false for negative.
        /// </summary>
        public bool Feedback { get; set; }

        /// <summary>
        /// Optional free-text comment explaining why the message was rated that way.
        /// Limited to 1000 characters by the API; longer values are rejected with an HTTP 400 response.
        /// Leading and trailing whitespace is trimmed, and blank values are stored as no comment.
        /// </summary>
        /// <remarks>
        /// Submitting feedback again for the same message overwrites the stored comment, so leaving
        /// this null clears a comment that was submitted previously.
        /// This value is written by the end user and is persisted and displayed in the Serenity Star
        /// back office, so avoid placing sensitive or personal data in it.
        /// </remarks>
        public string? Comment { get; set; }
    }
}
