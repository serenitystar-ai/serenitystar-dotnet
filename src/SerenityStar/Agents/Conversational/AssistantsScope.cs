using System.Net.Http;

namespace SerenityStar.Agents.Conversational
{
    /// <summary>
    /// Provides methods for interacting with Assistant agents.
    /// </summary>
    public sealed class AssistantsScope : ConversationalAgentBase
    {
        internal AssistantsScope(HttpClient httpClient)
            : base(httpClient)
        {
        }
    }
}
