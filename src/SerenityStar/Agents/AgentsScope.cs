using SerenityStar.Agents.Conversational;
using SerenityStar.Agents.System;
using SerenityStar.Client;

namespace SerenityStar.Agents
{
    /// <summary>
    /// Provides access to all agent types.
    /// </summary>
    public sealed class AgentsScope
    {
        /// <summary>
        /// Interact with Activity agents.
        /// </summary>
        public ActivitiesScope Activities { get; }

        /// <summary>
        /// Interact with ChatCompletion agents.
        /// </summary>
        public ChatCompletionsScope ChatCompletions { get; }

        /// <summary>
        /// Interact with AI Proxy agents.
        /// </summary>
        public AIProxiesScope AIProxies { get; }

        /// <summary>
        /// Interact with Assistant agents.
        /// </summary>
        public AssistantsScope Assistants { get; }

        /// <summary>
        /// Interact with Copilot agents.
        /// </summary>
        public CopilotScope Copilots { get; }

        internal AgentsScope(SerenityApiClient apiClient)
        {
            // Agents
            Activities = new ActivitiesScope(apiClient);
            ChatCompletions = new ChatCompletionsScope(apiClient);
            AIProxies = new AIProxiesScope(apiClient);
            Assistants = new AssistantsScope(apiClient);
            Copilots = new CopilotScope(apiClient);
        }
    }
}
