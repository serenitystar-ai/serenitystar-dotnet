using SerenityStar.Errors.Models;

namespace SerenityStar.Errors.Constants
{
    /// <summary>
    /// The keys of <see cref="ResourceNotFoundError.Errors"/>. Each key identifies the missing resource.
    /// </summary>
    public static class ResourceNotFoundKeys
    {
        /// <summary>
        /// The agent code does not resolve to an agent, or has no version resolvable for the request.
        /// </summary>
        public const string AgentCodeInvalid = "agent_code_invalid";

        /// <summary>
        /// The agent has no published version, or a specific requested version does not exist.
        /// </summary>
        public const string AgentVersionNotFound = "agent_version_not_found";

        /// <summary>
        /// The referenced conversation does not exist, or is not visible to the caller.
        /// </summary>
        public const string ConversationNotFound = "conversation_not_found";

        /// <summary>
        /// The requested AI model was not found.
        /// </summary>
        public const string AIModelNotFound = "aimodel_not_found";
    }
}
