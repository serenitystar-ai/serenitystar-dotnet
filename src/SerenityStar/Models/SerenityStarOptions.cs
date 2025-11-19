namespace SerenityStar.Models
{
    /// <summary>
    /// Options for Serenity Star.
    /// </summary>
    public class SerenityStarOptions
    {
        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timeout in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the assistant agent name.
        /// </summary>
        public string? AssistantAgent { get; set; }

        /// <summary>
        /// Gets or sets the activity agent name.
        /// </summary>
        public string? ActivityAgent { get; set; }
    }
}