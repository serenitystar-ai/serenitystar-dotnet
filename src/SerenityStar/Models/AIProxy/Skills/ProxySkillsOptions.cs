namespace SerenityStar.Models.AIProxy.Skills
{
    /// <summary>
    /// The skills available to a proxy execution. Each skill is enabled by setting its options; the ones left null are
    /// not available to the model.
    /// </summary>
    public sealed class ProxySkillsOptions
    {
        /// <summary>
        /// Enables the image generation skill.
        /// </summary>
        public ImageGenerationSkillOptions? ImageGeneration { get; set; }

        /// <summary>
        /// Enables the speech generation skill.
        /// </summary>
        public SpeechGenerationSkillOptions? SpeechGeneration { get; set; }

        /// <summary>
        /// Enables the web search skill.
        /// </summary>
        public WebSearchSkillOptions? WebSearch { get; set; }

        /// <summary>
        /// Enables the workbench skill.
        /// </summary>
        public WorkbenchSkillOptions? Workbench { get; set; }

        /// <summary>
        /// Enables the Request User Choice skill, so the model can pause and ask the user to choose from a set of
        /// options (<see cref="Execute.UserChoicePendingAction"/>).
        /// </summary>
        public bool UserChoice { get; set; }
    }
}
