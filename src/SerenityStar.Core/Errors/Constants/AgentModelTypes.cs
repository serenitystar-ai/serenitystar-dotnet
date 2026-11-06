using SerenityStar.Errors.Models;

namespace SerenityStar.Errors.Constants
{
    /// <summary>
    /// The values of <see cref="AgentExecutionAttempt.ModelType"/>.
    /// </summary>
    public static class AgentModelTypes
    {
        /// <summary>
        /// The agent's main model.
        /// </summary>
        public const string Main = "main";

        /// <summary>
        /// The agent's fallback model.
        /// </summary>
        public const string Fallback = "fallback";
    }
}
