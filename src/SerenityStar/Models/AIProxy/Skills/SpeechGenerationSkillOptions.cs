namespace SerenityStar.Models.AIProxy.Skills
{
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
}
