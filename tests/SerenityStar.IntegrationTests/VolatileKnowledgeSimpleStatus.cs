namespace SerenityStar.IntegrationTests
{
    /// <summary>
    /// Defines simple status values for volatile knowledge processing.
    /// </summary>

    public static class VolatileKnowledgeSimpleStatus
    {
        /// <summary>
        /// Status indicating the knowledge is being analyzed.
        /// </summary>
        public const string Analyzing = "analyzing";
        /// <summary>
        /// Status indicating the knowledge is invalid.
        /// </summary>
        public const string Invalid = "invalid";
        /// <summary>
        /// Status indicating the knowledge has been successfully processed.
        /// </summary>
        public const string Success = "success";
        /// <summary>
        /// Status indicating an error occurred during processing.
        /// </summary>
        public const string Error = "error";
        /// <summary>
        /// Status indicating the knowledge has expired.
        /// </summary>
        public const string Expired = "expired";
    }
}