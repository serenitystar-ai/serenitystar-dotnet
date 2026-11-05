namespace SerenityStar.Models.AIProxy.Skills
{
    /// <summary>
    /// Options for the web search skill.
    /// </summary>
    public sealed class WebSearchSkillOptions
    {
        /// <summary>
        /// The model used to search. When null, the platform's default model and vendor are used.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// The vendor of the model (e.g., "OpenAI").
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// When true, each invocation of this skill must be approved by the user before it runs.
        /// </summary>
        public bool RequiresApproval { get; set; }
    }
}
