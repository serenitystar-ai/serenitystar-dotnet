namespace SerenityStar.Models.AIProxy
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

        /// <summary>
        /// Options for the image generation skill.
        /// </summary>
        public sealed class ImageGenerationSkillOptions
        {
            /// <summary>
            /// The model used to generate images. When null, the platform's default model and vendor are used.
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

        /// <summary>
        /// Options for the speech generation skill.
        /// </summary>
        public sealed class SpeechGenerationSkillOptions
        {
            /// <summary>
            /// The model used to generate speech. When null, the platform's default model and vendor are used.
            /// </summary>
            public string? Model { get; set; }

            /// <summary>
            /// The vendor of the model (e.g., "OpenAI").
            /// </summary>
            public string? Vendor { get; set; }

            /// <summary>
            /// The name of the voice to use.
            /// </summary>
            public string? Voice { get; set; }

            /// <summary>
            /// The identifier of the voice to use, for vendors that identify voices by id.
            /// </summary>
            public string? VoiceId { get; set; }

            /// <summary>
            /// The speed of the generated speech.
            /// </summary>
            public float? Speed { get; set; }

            /// <summary>
            /// When true, each invocation of this skill must be approved by the user before it runs.
            /// </summary>
            public bool RequiresApproval { get; set; }
        }

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

        /// <summary>
        /// Options for the workbench skill.
        /// </summary>
        public sealed class WorkbenchSkillOptions
        {
            /// <summary>
            /// When true, each invocation of this skill must be approved by the user before it runs.
            /// </summary>
            public bool RequiresApproval { get; set; }
        }
    }
}
