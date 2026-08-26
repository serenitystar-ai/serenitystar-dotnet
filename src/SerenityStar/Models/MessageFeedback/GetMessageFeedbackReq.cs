using System;

namespace SerenityStar.Models.MessageFeedback
{
    /// <summary>
    /// Options for retrieving a page of message feedback for an agent.
    /// </summary>
    public sealed class GetMessageFeedbackReq
    {
        /// <summary>
        /// The 1-based page number to retrieve. Defaults to 1.
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// The number of entries per page. Defaults to 20.
        /// The API rejects values above 1000 with an HTTP 400 response.
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Optional inclusive lower bound for the feedback date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional inclusive upper bound for the feedback date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The sort direction by feedback date, either "asc" or "desc". Defaults to "desc".
        /// The API rejects any other value with an HTTP 400 response.
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }
}
