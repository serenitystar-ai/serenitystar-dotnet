using SerenityStar.Client;

namespace SerenityStar.Agents.Conversational
{
    /// <summary>
    /// Provides methods for interacting with Copilot agents.
    /// </summary>
    public sealed class CopilotScope : ConversationalAgentBase
    {
        internal CopilotScope(SerenityApiClient apiClient)
            : base(apiClient)
        {
        }
    }
}
