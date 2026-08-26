using SerenityStar.Models.Common;
using System;
using System.Collections.Generic;

namespace SerenityStar.Models.MessageFeedback
{
    /// <summary>
    /// Represents a single piece of feedback submitted for an agent message.
    /// </summary>
    /// <remarks>
    /// Instances carry end-user content (<see cref="UserMessage"/>, <see cref="AgentMessage"/>,
    /// <see cref="Comment"/>) and the <see cref="UserIdentifier"/>. Treat them as personal data:
    /// do not log them verbatim and do not forward them to third parties without a lawful basis.
    /// </remarks>
    public sealed class MessageFeedbackRes
    {
        /// <summary>
        /// The code of the agent the feedback belongs to.
        /// </summary>
        public string AgentCode { get; set; } = string.Empty;

        /// <summary>
        /// The feedback ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The ID of the conversation the rated message belongs to.
        /// </summary>
        public Guid? ConversationId { get; set; }

        /// <summary>
        /// The ID of the agent message that was rated.
        /// </summary>
        public Guid AgentMessageId { get; set; }

        /// <summary>
        /// The user message that preceded the rated agent message.
        /// </summary>
        public string UserMessage { get; set; } = string.Empty;

        /// <summary>
        /// The content of the agent message that was rated.
        /// </summary>
        public string AgentMessage { get; set; } = string.Empty;

        /// <summary>
        /// The channel the feedback was submitted from.
        /// </summary>
        public string Channel { get; set; } = string.Empty;

        /// <summary>
        /// The UTC date when the feedback was submitted.
        /// </summary>
        public DateTime DateUtc { get; set; }

        /// <summary>
        /// The feedback value - true for positive, false for negative.
        /// </summary>
        public bool Feedback { get; set; }

        /// <summary>
        /// Optional free-text comment explaining why the message was rated that way.
        /// Null when the user did not provide one.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// The identifier of the user who submitted the feedback.
        /// </summary>
        public string? UserIdentifier { get; set; }

        /// <summary>
        /// Links to the operations available for this feedback entry.
        /// </summary>
        public List<HateoasLink> Links { get; set; } = new List<HateoasLink>();
    }
}
