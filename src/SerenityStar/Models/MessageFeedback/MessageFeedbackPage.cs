using SerenityStar.Models.Common;
using System.Collections.Generic;

namespace SerenityStar.Models.MessageFeedback
{
    /// <summary>
    /// Represents a page of message feedback entries for an agent.
    /// </summary>
    public sealed class MessageFeedbackPage
    {
        /// <summary>
        /// The code of the agent the feedback belongs to.
        /// </summary>
        public string AgentCode { get; set; } = string.Empty;

        /// <summary>
        /// The feedback entries contained in this page.
        /// </summary>
        public List<MessageFeedbackRes> Items { get; set; } = new List<MessageFeedbackRes>();

        /// <summary>
        /// The total number of feedback entries matching the request across all pages.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The 1-based number of the current page.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The number of entries requested per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Links to the operations available for this page.
        /// </summary>
        public List<HateoasLink> Links { get; set; } = new List<HateoasLink>();
    }
}
