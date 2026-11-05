namespace SerenityStar.Models.AIProxy.Skills
{
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
