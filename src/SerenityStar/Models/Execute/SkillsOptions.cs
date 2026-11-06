namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Per-execution options for the agent's skills.
    /// </summary>
    public sealed class SkillsOptions
    {
        /// <summary>
        /// When true, every skill invocation that requires approval is approved automatically and the agent never
        /// pauses for user approval. Defaults to false.
        /// </summary>
        /// <remarks>
        /// This bypasses the human review configured for the agent's skills. Only enable it in trusted or fully
        /// automated contexts, where nobody is available to review the invocations.
        /// </remarks>
        public bool AutoMode { get; set; }
    }
}
